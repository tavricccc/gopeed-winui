using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Gopeed_Native.Models;
using Gopeed_Native.ViewModels;
using Gopeed_Native.Views;
using System.Diagnostics;

namespace Gopeed_Native;

public sealed partial class MainPage : Page
{
 public DownloadsViewModel ViewModel { get; } = new();
 private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
 private bool refreshing;
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
  timer.Tick += async (_, _) => { if (refreshing || !ViewModel.IsConnected) return; refreshing = true; await ViewModel.RefreshAsync(); refreshing = false; };
 }
 private async void Start(object sender, RoutedEventArgs e) { Loaded -= Start; await ViewModel.InitializeAsync(); timer.Start(); }
 private async void AddDownload(object sender, RoutedEventArgs e)
 {
  if (!ViewModel.IsConnected) return; timer.Stop();
  try { await new AddDownloadDialog(ViewModel.Core) { XamlRoot = XamlRoot }.ShowAsync(); await ViewModel.RefreshAsync(); }
  catch (Exception error) { ViewModel.Error = error.Message; } finally { timer.Start(); }
 }
 private void FilterChanged(object s, SelectionChangedEventArgs e) { if (FilterBox?.SelectedItem is ComboBoxItem item) { ViewModel.Filter = item.Tag.ToString()!; ViewModel.ApplyFilter(); } }
 private void SearchChanged(AutoSuggestBox s, AutoSuggestBoxTextChangedEventArgs e) { ViewModel.Search = s.Text; ViewModel.ApplyFilter(); }
 private async void PauseSelected(object s, RoutedEventArgs e) { if (ViewModel.Selected is { } item) await ViewModel.ActAsync("pause", [item]); }
 private async void ResumeSelected(object s, RoutedEventArgs e) { if (ViewModel.Selected is { } item) await ViewModel.ActAsync("continue", [item]); }
 private async void PauseAll(object s, RoutedEventArgs e) => await ViewModel.ActAsync("pause", ViewModel.VisibleItems.Where(i => i.CanPause));
 private async void ResumeAll(object s, RoutedEventArgs e) => await ViewModel.ActAsync("continue", ViewModel.VisibleItems.Where(i => i.CanResume));
 private async void RefreshClicked(object s, RoutedEventArgs e) => await ViewModel.RefreshAsync();
 private void OpenSelected(object s, RoutedEventArgs e) { if (ViewModel.Selected is { IsComplete: true } item) OpenPath(item.FilePath); }
 private void OpenFolder(object s, RoutedEventArgs e) { if (ViewModel.Selected is { } item) OpenPath(item.Folder); }
 private void OpenPath(string path) { try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch (Exception e) { ViewModel.Error = e.Message; } }
 private async void DeleteSelected(object s, RoutedEventArgs e) { if (ViewModel.Selected is { } item) await DeleteAsync(item); }
 private async Task DeleteAsync(DownloadItem item)
 {
  var files = new CheckBox { Content = "同時刪除已下載的檔案" };
  var dialog = new ContentDialog { Title = "移除下載？", Content = new StackPanel { Spacing = 12, Children = { new TextBlock { Text = item.Name, TextWrapping = TextWrapping.Wrap }, files } }, PrimaryButtonText = "移除", CloseButtonText = "取消", DefaultButton = ContentDialogButton.Close, XamlRoot = XamlRoot };
  if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.ActAsync("delete", [item], files.IsChecked == true);
 }
 private async void ShowDetails(object s, RoutedEventArgs e)
 {
  if (ViewModel.Selected is not { } item) return;
  var text = new TextBlock { Text = $"{item.Name}\n\n{item.StatusText} · {item.Protocol}\n{item.TransferText}\n速度：{item.SpeedText}\n剩餘時間：{item.RemainingText}\n\n儲存位置\n{item.FilePath}\n\n來源\n{item.Url}", TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
  await new ContentDialog { Title = "下載詳細資訊", Content = new ScrollViewer { Content = text, MaxHeight = 450 }, CloseButtonText = "關閉", XamlRoot = XamlRoot }.ShowAsync();
 }
 private void ListDoubleTapped(object s, DoubleTappedRoutedEventArgs e) { if (ViewModel.Selected?.IsComplete == true) OpenSelected(s, new()); else ShowDetails(s, new()); }
 private DownloadItem? ContextItem(object s) => ViewModel.VisibleItems.FirstOrDefault(i => i.Id == (s as MenuFlyoutItem)?.Tag?.ToString());
 private async void ContextPause(object s, RoutedEventArgs e) { if (ContextItem(s) is { CanPause: true } item) await ViewModel.ActAsync("pause", [item]); }
 private async void ContextResume(object s, RoutedEventArgs e) { if (ContextItem(s) is { CanResume: true } item) await ViewModel.ActAsync("continue", [item]); }
 private void ContextFolder(object s, RoutedEventArgs e) { if (ContextItem(s) is { } item) OpenPath(item.Folder); }
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
  if (!downloads) SettingsFrame.Navigate(e.IsSettingsSelected ? typeof(SettingsPage) : typeof(ExtensionsPage), ViewModel);
 }
}
