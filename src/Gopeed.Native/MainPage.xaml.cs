using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Gopeed_Native.Models;
using Gopeed_Native.ViewModels;
using Gopeed_Native.Views;
using Gopeed_Native.Services;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

namespace Gopeed_Native;

public sealed partial class MainPage : Page
{
 public DownloadsViewModel ViewModel { get; } = new();
 private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
 private bool refreshing;
 private readonly TaskCompletionSource ready = new();
 private readonly SemaphoreSlim addDialogGate = new(1);
 public MainPage()
 {
  InitializeComponent(); Loaded += Start;
  SizeChanged += (_, e) =>
  {
   var wide = e.NewSize.Width >= 1000;
   DetailColumn.Width = new GridLength(wide ? 280 : 0);
   DetailsPane.Visibility = wide ? Visibility.Visible : Visibility.Collapsed;
  };
  Unloaded += (_, _) => { timer.Stop(); ViewModel.Dispose(); };
  ViewModel.VisibleItems.CollectionChanged += (_, _) => EmptyState.Visibility = ViewModel.VisibleItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
  ViewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ViewModel.Error) && ViewModel.Error.Length > 0) { ErrorBar.Message = ViewModel.Error; ErrorBar.IsOpen = true; } };
  ViewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ViewModel.Selected)) { SelectionDetails.Visibility = ViewModel.HasSingleSelection ? Visibility.Visible : Visibility.Collapsed; DetailsHint.Visibility = ViewModel.HasSingleSelection ? Visibility.Collapsed : Visibility.Visible; } };
  timer.Tick += async (_, _) => { if (refreshing || !ViewModel.IsConnected) return; refreshing = true; await ViewModel.RefreshAsync(); refreshing = false; };
 }
 private async void Start(object sender, RoutedEventArgs e) { Loaded -= Start; await ViewModel.InitializeAsync(); timer.Start(); ready.SetResult(); if (ViewModel.IsConnected && UiPreferences.Load().CheckForUpdates) { try { var update = await UpdateService.CheckAsync(ViewModel.Core); if (update is not null) { ErrorBar.Severity = InfoBarSeverity.Informational; ErrorBar.Message = $"有新版本：{update.Version}"; var button = new Button { Content = "下載更新" }; button.Click += async (_, _) => await UpdateService.PromptAsync(ViewModel.Core, update, XamlRoot); ErrorBar.ActionButton = button; ErrorBar.IsOpen = true; } } catch (Exception) { /* A background update check must not interrupt downloads. Manual checks report errors. */ } } }
 private async void AddDownload(object sender, RoutedEventArgs e)
 {
  await AddDownloadAsync(null);
 }
 private async Task AddDownloadAsync(System.Text.Json.Nodes.JsonObject? parameters)
 {
  await ready.Task;
  if (!ViewModel.IsConnected) return;
  await addDialogGate.WaitAsync(); timer.Stop();
  try { await NativeDialogs.ShowAsync(new AddDownloadDialog(ViewModel.Core, parameters), XamlRoot); await ViewModel.RefreshAsync(); }
  catch (Exception error) { ViewModel.Error = UserError.Message(error); } finally { timer.Start(); addDialogGate.Release(); }
 }
 public async void OpenProtocol(string value)
 {
  try
  {
   var link = GopeedLink.Parse(value); await ready.Task;
   if (link.Route == "extension")
   {
    Navigation.SelectedItem = Navigation.MenuItems.OfType<NavigationViewItem>().First(i => i.Tag?.ToString() == "extensions");
    SettingsFrame.Content = new ExtensionsPage(ViewModel, link.Parameters?["url"]?.GetValue<string>());
   }
   else
   {
    Navigation.SelectedItem = Navigation.MenuItems.OfType<NavigationViewItem>().First(i => i.Tag?.ToString() == "downloads");
    if (link.Route == "create") new DownloadWindow(link.Parameters ?? new System.Text.Json.Nodes.JsonObject()).Activate();
   }
  }
  catch (Exception error) { ViewModel.Error = $"無法開啟 Gopeed 連結：{error.Message}"; }
 }
 private void FilterChanged(object s, SelectionChangedEventArgs e) { if (FilterBox?.SelectedItem is ComboBoxItem item) { ViewModel.Filter = item.Tag.ToString()!; ViewModel.ApplyFilter(); } }
 private void SearchChanged(AutoSuggestBox s, AutoSuggestBoxTextChangedEventArgs e) { ViewModel.Search = s.Text; ViewModel.ApplyFilter(); }
 private async void PauseSelected(object s, RoutedEventArgs e) => await ViewModel.ActAsync("pause", ViewModel.Selection.Where(x => x.CanPause));
 private async void ResumeSelected(object s, RoutedEventArgs e) => await ViewModel.ActAsync("continue", ViewModel.Selection.Where(x => x.CanResume));
 private async void PauseAll(object s, RoutedEventArgs e) => await ViewModel.ActAsync("pause", ViewModel.AllItems.Where(i => i.CanPause));
 private async void ResumeAll(object s, RoutedEventArgs e) => await ViewModel.ActAsync("continue", ViewModel.AllItems.Where(i => i.CanResume));
 private async void RefreshClicked(object s, RoutedEventArgs e) => await ViewModel.RefreshAsync();
 private async void PrimarySelected(object s, RoutedEventArgs e)
 {
  var action = ViewModel.PrimaryActionKey;
  if (action == "open") OpenSelected(s, e);
  else if (action == "folder") { foreach (var folder in ViewModel.Selection.Select(x => x.Folder).Distinct()) FileActions.Open(folder); }
  else if (action == "pause") await ViewModel.ActAsync(action, ViewModel.Selection.Where(x => x.CanPause));
  else if (action == "continue") await ViewModel.ActAsync(action, ViewModel.Selection.Where(x => x.CanResume));
 }
 private void OpenSelected(object s, RoutedEventArgs e) { try { if (ViewModel.Selected is { IsComplete: true } item) FileActions.Open(item.OpenPath); } catch (Exception ex) { ViewModel.Error = UserError.Message(ex); } }
 private void OpenFolder(object s, RoutedEventArgs e) { try { if (ViewModel.Selected is { } item) FileActions.Reveal(item.FilePath, item.Folder); } catch (Exception ex) { ViewModel.Error = UserError.Message(ex); } }
 private void CopySelected(object s, RoutedEventArgs e) => FileActions.Copy(string.Join("\n", ViewModel.Selection.Select(x => x.Url)));
 private async void PasteDownload(object s, RoutedEventArgs e)
 {
  try { var content = Clipboard.GetContent(); if (!content.Contains(StandardDataFormats.Text)) throw new FormatException("剪貼簿沒有文字連結。"); await AddText(await content.GetTextAsync()); }
  catch (Exception ex) { ViewModel.Error = UserError.Message(ex); }
 }
 private async Task AddText(string text)
 {
  var links = text.Split(['\r','\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
  if (links.Length == 0 || links.Any(link => !Uri.TryCreate(link, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https" or "magnet" or "ed2k" or "file"))) throw new FormatException("請貼上 HTTP、HTTPS、磁力或 eD2k 下載連結。");
  await AddDownloadAsync(new System.Text.Json.Nodes.JsonObject { ["req"] = new System.Text.Json.Nodes.JsonObject { ["url"] = string.Join("\n", links) } });
 }
 private void DownloadDragOver(object s, DragEventArgs e) { e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.Text) || e.DataView.Contains(StandardDataFormats.WebLink) || e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None; }
 private async void DownloadDrop(object s, DragEventArgs e)
 {
  var deferral = e.GetDeferral();
  string? text = null;
  System.Text.Json.Nodes.JsonObject? parameters = null;
  try
  {
   if (e.DataView.Contains(StandardDataFormats.WebLink)) text = (await e.DataView.GetWebLinkAsync()).AbsoluteUri;
   else if (e.DataView.Contains(StandardDataFormats.Text)) text = await e.DataView.GetTextAsync();
   else if (e.DataView.Contains(StandardDataFormats.StorageItems))
   {
    var files = await e.DataView.GetStorageItemsAsync();
    if (files.Count == 0 || files.Any(f => !f.Path.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase))) throw new FormatException("拖放檔案目前接受 torrent 檔案。");
    parameters = new System.Text.Json.Nodes.JsonObject { ["req"] = new System.Text.Json.Nodes.JsonObject { ["url"] = string.Join("\n", files.Select(f => f.Path)) } };
   }
  }
  catch (Exception ex) { ViewModel.Error = UserError.Message(ex); return; }
  finally { deferral.Complete(); }
  try { if (parameters is not null) await AddDownloadAsync(parameters); else if (text is not null) await AddText(text); }
  catch (Exception ex) { ViewModel.Error = UserError.Message(ex); }
 }
 private async void DeleteSelected(object s, RoutedEventArgs e) => await DeleteItemsAsync(ViewModel.Selection);
 private async Task DeleteAsync(DownloadItem item)
 {
  await DeleteItemsAsync([item]);
 }
 private async void ShowDetails(object s, RoutedEventArgs e)
 {
  if (ViewModel.Selected is not { } item) return;
  await NativeDialogs.ShowAsync(new TaskDetailsDialog(ViewModel.Core, item), XamlRoot);
 }
 private void ListDoubleTapped(object s, DoubleTappedRoutedEventArgs e) { if (ViewModel.Selected?.IsComplete == true) OpenSelected(s, new()); else ShowDetails(s, new()); }
 private DownloadItem? ContextItem(object s) => ViewModel.VisibleItems.FirstOrDefault(i => i.Id == (s as MenuFlyoutItem)?.Tag?.ToString());
 private async void ContextPrimary(object s, RoutedEventArgs e)
 {
  if (ContextItem(s) is not { } item) return;
  var action = item.PrimaryAction;
  if (action.Key == "open") { try { FileActions.Open(item.OpenPath); } catch (Exception ex) { ViewModel.Error = UserError.Message(ex); } }
  else if (action.Key != "none") await ViewModel.ActAsync(action.Key, [item]);
 }
 private async void ContextPause(object s, RoutedEventArgs e) { if (ContextItem(s) is { CanPause: true } item) await ViewModel.ActAsync("pause", [item]); }
 private async void ContextResume(object s, RoutedEventArgs e) { if (ContextItem(s) is { CanResume: true } item) await ViewModel.ActAsync("continue", [item]); }
 private void ContextFolder(object s, RoutedEventArgs e) { try { if (ContextItem(s) is { } item) FileActions.Reveal(item.FilePath, item.Folder); } catch (Exception ex) { ViewModel.Error = UserError.Message(ex); } }
 private async void ContextDelete(object s, RoutedEventArgs e) { if (ContextItem(s) is { } item) await DeleteAsync(item); }
 private void NewShortcut(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { AddDownload(s, new()); e.Handled = true; }
 private void SearchShortcut(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { SearchBox.Focus(FocusState.Keyboard); e.Handled = true; }
 private async void RefreshShortcut(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { await ViewModel.RefreshAsync(); e.Handled = true; }
 private void NavigationChanged(NavigationView s, NavigationViewSelectionChangedEventArgs e)
 {
  if (SettingsFrame is null) return;
  if (!ViewModel.IsConnected && (e.IsSettingsSelected || (e.SelectedItem as NavigationViewItem)?.Tag?.ToString() == "extensions")) { Navigation.SelectedItem = Navigation.MenuItems[0]; return; }
  var downloads = !e.IsSettingsSelected && (e.SelectedItem as NavigationViewItem)?.Tag?.ToString() != "extensions";
  DownloadsSurface.Visibility = downloads ? Visibility.Visible : Visibility.Collapsed;
  SettingsFrame.Visibility = downloads ? Visibility.Collapsed : Visibility.Visible;
  if (!downloads && ViewModel.IsConnected) SettingsFrame.Content = e.IsSettingsSelected ? new SettingsPage(ViewModel) : new ExtensionsPage(ViewModel);
 }
}
