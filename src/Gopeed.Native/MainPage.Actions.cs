using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Net.Http;
using System.Text.Json.Nodes;
using Gopeed_Native.Models;
using Gopeed_Native.Services;
using Gopeed_Native.Views;

namespace Gopeed_Native;

public sealed partial class MainPage
{
    private void DownloadSelectionChanged(object sender, SelectionChangedEventArgs args) => ViewModel.SetSelection(DownloadList.SelectedItems.Cast<DownloadItem>());
    private void SortChanged(object sender, SelectionChangedEventArgs args) { if (((ComboBox)sender).SelectedItem is ComboBoxItem item) { ViewModel.Sort = item.Tag.ToString()!; ViewModel.ApplyFilter(); } }
    private void SelectAll(object sender, RoutedEventArgs args) => DownloadList.SelectAll();
    private void SelectAllShortcut(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) { if (DownloadList.FocusState != FocusState.Unfocused) { DownloadList.SelectAll(); args.Handled = true; } }
    private async void DeleteShortcut(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) { if (DownloadList.FocusState != FocusState.Unfocused && ViewModel.HasSelection) { args.Handled = true; await DeleteItemsAsync(ViewModel.Selection); } }
    private async Task DeleteItemsAsync(IEnumerable<DownloadItem> items)
    {
        var targets = items.ToList(); if (targets.Count == 0) return;
        var files = new CheckBox { Content = "同時刪除下載的檔案", IsChecked = !UiPreferences.Load().KeepFilesOnRemove };
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = targets.Count == 1 ? targets[0].Name : string.Join("\n", targets.Take(5).Select(x => x.Name)), TextWrapping = TextWrapping.Wrap });
        if (targets.Any(x => x.CanPause)) panel.Children.Add(new TextBlock { Text = "尚未結束的下載與做種將停止。", TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(files);
        var dialog = new ContentDialog { Title = $"移除 {targets.Count} 個下載？", Content = panel, PrimaryButtonText = "移除", CloseButtonText = "取消", DefaultButton = ContentDialogButton.Close };
        if (await NativeDialogs.ShowAsync(dialog, XamlRoot) != ContentDialogResult.Primary) return;
        await ViewModel.ActAsync("delete", targets, files.IsChecked == true);
        var prefs = UiPreferences.Load(); prefs.KeepFilesOnRemove = files.IsChecked != true; prefs.Save();
    }
    private async void ClearCompleted(object sender, RoutedEventArgs args)
    {
        var targets = ViewModel.VisibleItems.Where(x => x.IsComplete && !x.Uploading && !x.IsProcessing).ToList(); if (targets.Count == 0) return;
        var dialog = new ContentDialog { Title = $"清除 {targets.Count} 個完成紀錄？", Content = "已下載的檔案會保留。", PrimaryButtonText = "清除紀錄", CloseButtonText = "取消" };
        if (await NativeDialogs.ShowAsync(dialog, XamlRoot) == ContentDialogResult.Primary) await ViewModel.ActAsync("delete", targets);
    }
    private async void Redownload(object sender, RoutedEventArgs args)
    {
        if (ViewModel.Selected is not { } item) return;
        var meta = item.Data["meta"]!;
        await AddDownloadAsync(new JsonObject { ["req"] = meta["req"]!.DeepClone(), ["opts"] = meta["opts"]!.DeepClone() });
    }
    private async void EditSelectedSource(object sender, RoutedEventArgs args) { if (ViewModel.Selected is { CanEditSource: true } item) await EditSource(item); }
    private async void ContextEditSource(object sender, RoutedEventArgs args) { if (ContextItem(sender) is { CanEditSource: true } item) await EditSource(item); }
    private async Task EditSource(DownloadItem item)
    {
        var source = new TextBox { Header = "來源網址", Text = item.Url }; var headers = new TextBox { Header = "HTTP 標頭", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 100, MaxHeight = 200 };
        var request = item.Data["meta"]!["req"]!.DeepClone().AsObject();
        if (request["extra"]?["header"] is JsonObject values) headers.Text = string.Join("\n", values.Select(x => $"{x.Key}: {x.Value}"));
        var resume = new CheckBox { Content = "更新後繼續下載", IsChecked = true };
        var panel = new StackPanel { Spacing = 12, Children = { source, headers, resume } };
        var dialog = new ContentDialog { Title = "修改下載來源", Content = panel, PrimaryButtonText = "更新", CloseButtonText = "取消" };
        dialog.PrimaryButtonClick += async (_, click) =>
        {
            var deferral = click.GetDeferral();
            try
            {
                if (!Uri.TryCreate(source.Text.Trim(), UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https")) throw new FormatException("請輸入有效的 HTTP 或 HTTPS 來源網址。");
                request["url"] = url.AbsoluteUri; request["extra"] ??= new JsonObject(); request["extra"]!["header"] = HttpHeaders.Parse(headers.Text);
                await ViewModel.Core.SendAsync(HttpMethod.Patch, "tasks/" + item.Id, new JsonObject { ["req"] = request.DeepClone() });
                if (resume.IsChecked == true) await ViewModel.Core.SendAsync(HttpMethod.Put, "tasks/" + item.Id + "/continue");
            }
            catch (Exception error) { click.Cancel = true; ViewModel.Error = UserError.Message(error); }
            finally { deferral.Complete(); }
        };
        await NativeDialogs.ShowAsync(dialog, XamlRoot); await ViewModel.RefreshAsync();
    }
    private async void ContextDetails(object sender, RoutedEventArgs args) { if (ContextItem(sender) is { } item) await NativeDialogs.ShowAsync(new TaskDetailsDialog(ViewModel.Core, item), XamlRoot); }
    private void ContextListenSource(object sender, RoutedEventArgs args)
    {
        if (ContextItem(sender) is not { CanEditSource: true } item) return;
        var prefs = UiPreferences.Load(); prefs.PendingUpdateTaskId = item.Id; prefs.Save();
        ErrorBar.Severity = InfoBarSeverity.Informational; ErrorBar.Message = $"等待瀏覽器下載連結：{item.Name}"; ErrorBar.IsOpen = true;
        var cancel = new Button { Content = "取消等待" }; cancel.Click += (_, _) => { var settings = UiPreferences.Load(); settings.PendingUpdateTaskId = ""; settings.Save(); ErrorBar.IsOpen = false; ErrorBar.ActionButton = null; }; ErrorBar.ActionButton = cancel;
    }
}
