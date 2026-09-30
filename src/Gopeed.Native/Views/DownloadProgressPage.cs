using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Gopeed_Native.Models;
using Gopeed_Native.Services;
using System.Net.Http;

namespace Gopeed_Native.Views;

public sealed class DownloadProgressPage : Page
{
    private readonly CoreClient core;
    private readonly string id;
    private readonly Action close;
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly FontIcon stateIcon = new() { FontSize = 28 };
    private readonly TextBlock heading = new();
    private readonly TextBlock name = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock status = new();
    private readonly ProgressBar progress = new() { Maximum = 100 };
    private readonly TextBlock transfer = new();
    private readonly TextBlock source = new() { TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
    private readonly TextBlock folder = new() { TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
    private readonly Button primary = NativeButtons.Create("正在連接…", "\uE896", true);
    private readonly Button browse = NativeButtons.Create("儲存資料夾", "\uE8B7");
    private readonly CheckBox closeAfterOpen = new() { Content = "開啟檔案後關閉此視窗" };
    private readonly Expander details = new() { Header = "來源與詳細資訊", HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly InfoBar error = new() { Severity = InfoBarSeverity.Error };
    private DownloadItem? item;
    private string lastStatus = "";
    private bool refreshing;

    public DownloadProgressPage(CoreClient core, string id, Action close)
    {
        this.core = core; this.id = id; this.close = close;
        heading.Style = (Style)Application.Current.Resources["TitleTextBlockStyle"];
        name.Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"];
        primary.IsEnabled = false;
        closeAfterOpen.IsChecked = UiPreferences.Load().CloseProgressAfterOpen;
        closeAfterOpen.Checked += SaveClosePreference; closeAfterOpen.Unchecked += SaveClosePreference;
        var detailContent = new StackPanel { Spacing = 12 };
        detailContent.Children.Add(new TextBlock { Text = "來源網址", Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"] }); detailContent.Children.Add(source);
        var copy = NativeButtons.Create("複製下載連結", "\uE8C8"); copy.Click += (_, _) => { if (item is not null) FileActions.Copy(item.Url); }; detailContent.Children.Add(copy); details.Content = detailContent;
        var content = new StackPanel { Spacing = 16 };
        content.Children.Add(error);
        content.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { stateIcon, heading } });
        foreach (var element in new UIElement[] { name, status, progress, transfer,
            new TextBlock { Text = "儲存位置", Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"] }, folder, closeAfterOpen, details }) content.Children.Add(element);
        content.Children.Add(new TextBlock { Text = "關閉視窗仍會繼續下載。", Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"] });
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(primary); buttons.Children.Add(browse);
        var dismiss = NativeButtons.Create("關閉", "\uE711"); dismiss.Click += (_, _) => close(); buttons.Children.Add(dismiss);
        var grid = new Grid { Padding = new Thickness(24), RowSpacing = 20 };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.Children.Add(new ScrollViewer { Content = content }); Grid.SetRow(buttons, 1); grid.Children.Add(buttons); Content = grid;
        primary.Click += async (_, _) => await PrimaryAction();
        browse.Click += (_, _) => { try { if (item is not null) FileActions.Reveal(item.FilePath, item.Folder); } catch (Exception e) { ShowError(e); } };
        timer.Tick += async (_, _) => await Refresh();
        Loaded += async (_, _) => { await Refresh(); timer.Start(); }; Unloaded += (_, _) => Stop();
    }
    private async Task Refresh()
    {
        if (refreshing) return; refreshing = true;
        try
        {
            item = new DownloadItem((await core.GetAsync("tasks/" + id))!.AsObject());
            var action = DownloadPresentation.ForStatus(item.Status);
            NativeButtons.SetContent(primary, action.Label, action.Glyph); primary.IsEnabled = action.Key != "none";
            heading.Text = item.IsComplete ? "下載完成" : item.StatusText;
            stateIcon.Glyph = item.IsComplete ? "\uE73E" : item.Status == "error" ? "\uE783" : "\uE896";
            stateIcon.Foreground = (Brush)Application.Current.Resources[item.IsComplete ? "SystemFillColorSuccessBrush" : "TextFillColorSecondaryBrush"];
            name.Text = item.Name; status.Text = item.IsComplete ? "檔案已儲存，可直接開啟或在資料夾中顯示。" : $"{item.Percent:0.0}% · {item.Protocol}";
            progress.Value = item.Percent; progress.IsIndeterminate = item.IsIndeterminate; progress.Visibility = item.IsComplete ? Visibility.Collapsed : Visibility.Visible;
            transfer.Text = item.IsComplete ? $"大小：{item.SizeText}" : $"{item.TransferText} · {item.SpeedText} · 剩餘 {item.RemainingText}";
            folder.Text = item.FilePath; source.Text = item.Url;
            closeAfterOpen.Visibility = item.IsComplete ? Visibility.Visible : Visibility.Collapsed;
            NativeButtons.SetContent(browse, item.IsComplete ? "在資料夾中顯示" : "儲存資料夾", "\uE8B7");
            if (lastStatus != item.Status) { details.IsExpanded = item.Status == "error"; lastStatus = item.Status; }
        }
        catch (Exception e) { ShowError(e); }
        finally { refreshing = false; }
    }
    private async Task PrimaryAction()
    {
        if (item is null) return;
        primary.IsEnabled = false;
        try
        {
            var action = DownloadPresentation.ForStatus(item.Status);
            if (action.Key == "open") { FileActions.Open(item.FilePath); if (closeAfterOpen.IsChecked == true) close(); }
            else if (action.Key != "none") { await core.SendAsync(HttpMethod.Put, $"tasks/{id}/{action.Key}"); await Refresh(); }
        }
        catch (Exception e) { ShowError(e); }
        finally { primary.IsEnabled = true; }
    }
    private void SaveClosePreference(object sender, RoutedEventArgs e) { var prefs = UiPreferences.Load(); prefs.CloseProgressAfterOpen = closeAfterOpen.IsChecked == true; prefs.Save(); }
    public void Stop() => timer.Stop();
    private void ShowError(Exception e) { error.Message = e.Message; error.IsOpen = true; }
}
