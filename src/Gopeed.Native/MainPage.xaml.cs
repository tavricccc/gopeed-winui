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
  ViewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ViewModel.Selected)) { SelectionDetails.Visibility = ViewModel.HasSelection ? Visibility.Visible : Visibility.Collapsed; DetailsHint.Visibility = ViewModel.HasSelection ? Visibility.Collapsed : Visibility.Visible; } };
  timer.Tick += async (_, _) => { if (refreshing || !ViewModel.IsConnected) return; refreshing = true; await ViewModel.RefreshAsync(); refreshing = false; };
 }
 private async void Start(object sender, RoutedEventArgs e) { Loaded -= Start; await ViewModel.InitializeAsync(); timer.Start(); ready.SetResult(); }
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
  catch (Exception error) { ViewModel.Error = error.Message; } finally { timer.Start(); addDialogGate.Release(); }
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
 private async void PauseSelected(object s, RoutedEventArgs e) { if (ViewModel.Selected is { } item) await ViewModel.ActAsync("pause", [item]); }
 private async void ResumeSelected(object s, RoutedEventArgs e) { if (ViewModel.Selected is { } item) await ViewModel.ActAsync("continue", [item]); }
 private async void PauseAll(object s, RoutedEventArgs e) => await ViewModel.ActAsync("pause", ViewModel.VisibleItems.Where(i => i.CanPause));
 private async void ResumeAll(object s, RoutedEventArgs e) => await ViewModel.ActAsync("continue", ViewModel.VisibleItems.Where(i => i.CanResume));
 private async void RefreshClicked(object s, RoutedEventArgs e) => await ViewModel.RefreshAsync();
 private async void PrimarySelected(object s, RoutedEventArgs e)
 {
  if (ViewModel.Selected is not { } item) return;
  var action = DownloadPresentation.ForStatus(item.Status);
  if (action.Key == "open") OpenSelected(s, e); else if (action.Key != "none") await ViewModel.ActAsync(action.Key, [item]);
 }
 private void OpenSelected(object s, RoutedEventArgs e) { try { if (ViewModel.Selected is { IsComplete: true } item) FileActions.Open(item.FilePath); } catch (Exception ex) { ViewModel.Error = ex.Message; } }
 private void OpenFolder(object s, RoutedEventArgs e) { try { if (ViewModel.Selected is { } item) FileActions.Reveal(item.FilePath, item.Folder); } catch (Exception ex) { ViewModel.Error = ex.Message; } }
 private void CopySelected(object s, RoutedEventArgs e) { if (ViewModel.Selected is { } item) FileActions.Copy(item.Url); }
 private async void PasteDownload(object s, RoutedEventArgs e)
 {
  try { var content = Clipboard.GetContent(); if (!content.Contains(StandardDataFormats.Text)) throw new FormatException("剪貼簿沒有文字連結。"); await AddText(await content.GetTextAsync()); }
  catch (Exception ex) { ViewModel.Error = ex.Message; }
 }
 private async Task AddText(string text)
 {
  var links = text.Split(['\r','\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
  if (links.Length == 0 || links.Any(link => !Uri.TryCreate(link, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https" or "magnet" or "file"))) throw new FormatException("請貼上 HTTP、HTTPS 或磁力下載連結。");
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
  catch (Exception ex) { ViewModel.Error = ex.Message; return; }
  finally { deferral.Complete(); }
  try { if (parameters is not null) await AddDownloadAsync(parameters); else if (text is not null) await AddText(text); }
  catch (Exception ex) { ViewModel.Error = ex.Message; }
 }
 private async void DeleteSelected(object s, RoutedEventArgs e) { if (ViewModel.Selected is { } item) await DeleteAsync(item); }
 private async Task DeleteAsync(DownloadItem item)
 {
  var files = new CheckBox { Content = "同時刪除已下載的檔案" };
  var dialog = new ContentDialog { Title = "移除下載？", Content = new StackPanel { Spacing = 12, Children = { new TextBlock { Text = item.Name, TextWrapping = TextWrapping.Wrap }, files } }, PrimaryButtonText = "移除", CloseButtonText = "取消", DefaultButton = ContentDialogButton.Close, XamlRoot = XamlRoot };
  if (await NativeDialogs.ShowAsync(dialog, XamlRoot) == ContentDialogResult.Primary) await ViewModel.ActAsync("delete", [item], files.IsChecked == true);
 }
 private async void ShowDetails(object s, RoutedEventArgs e)
 {
  if (ViewModel.Selected is not { } item) return;
  var text = new TextBlock { Text = $"{item.Name}\n\n{item.StatusText} · {item.Protocol}\n{item.TransferText}\n速度：{item.SpeedText}\n剩餘時間：{item.RemainingText}\n\n儲存位置\n{item.FilePath}\n\n來源\n{item.Url}", TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
  await NativeDialogs.ShowAsync(new ContentDialog { Title = "下載詳細資訊", Content = new ScrollViewer { Content = text, MaxHeight = 450 }, CloseButtonText = "關閉" }, XamlRoot);
 }
 private void ListDoubleTapped(object s, DoubleTappedRoutedEventArgs e) { if (ViewModel.Selected?.IsComplete == true) OpenSelected(s, new()); else ShowDetails(s, new()); }
 private DownloadItem? ContextItem(object s) => ViewModel.VisibleItems.FirstOrDefault(i => i.Id == (s as MenuFlyoutItem)?.Tag?.ToString());
 private async void ContextPrimary(object s, RoutedEventArgs e)
 {
  if (ContextItem(s) is not { } item) return;
  var action = DownloadPresentation.ForStatus(item.Status);
  if (action.Key == "open") { try { FileActions.Open(item.FilePath); } catch (Exception ex) { ViewModel.Error = ex.Message; } }
  else if (action.Key != "none") await ViewModel.ActAsync(action.Key, [item]);
 }
 private async void ContextPause(object s, RoutedEventArgs e) { if (ContextItem(s) is { CanPause: true } item) await ViewModel.ActAsync("pause", [item]); }
 private async void ContextResume(object s, RoutedEventArgs e) { if (ContextItem(s) is { CanResume: true } item) await ViewModel.ActAsync("continue", [item]); }
 private void ContextFolder(object s, RoutedEventArgs e) { try { if (ContextItem(s) is { } item) FileActions.Reveal(item.FilePath, item.Folder); } catch (Exception ex) { ViewModel.Error = ex.Message; } }
 private async void ContextDelete(object s, RoutedEventArgs e) { if (ContextItem(s) is { } item) await DeleteAsync(item); }
 private void NewShortcut(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { AddDownload(s, new()); e.Handled = true; }
 private void SearchShortcut(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { SearchBox.Focus(FocusState.Keyboard); e.Handled = true; }
 private async void RefreshShortcut(KeyboardAccelerator s, KeyboardAcceleratorInvokedEventArgs e) { await ViewModel.RefreshAsync(); e.Handled = true; }
 private void NavigationChanged(NavigationView s, NavigationViewSelectionChangedEventArgs e)
 {
  if (SettingsFrame is null) return;
  var downloads = !e.IsSettingsSelected && (e.SelectedItem as NavigationViewItem)?.Tag?.ToString() != "extensions";
  DownloadsSurface.Visibility = downloads ? Visibility.Visible : Visibility.Collapsed;
  SettingsFrame.Visibility = downloads ? Visibility.Collapsed : Visibility.Visible;
  if (!downloads) SettingsFrame.Content = e.IsSettingsSelected ? new SettingsPage(ViewModel) : new ExtensionsPage(ViewModel);
 }
}
