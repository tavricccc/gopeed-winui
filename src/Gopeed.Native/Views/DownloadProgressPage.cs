using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Models;
using Gopeed_Native.Services;
using System.Diagnostics;
using System.Net.Http;

namespace Gopeed_Native.Views;

public sealed class DownloadProgressPage : Page
{
    private readonly CoreClient core;
    private readonly string id;
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly TextBlock name = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock status = new();
    private readonly ProgressBar progress = new() { Maximum = 100 };
    private readonly TextBlock transfer = new();
    private readonly TextBlock source = new() { TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
    private readonly TextBlock folder = new() { TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
    private readonly Button pause = new() { Content = "暫停" };
    private readonly Button resume = new() { Content = "繼續 / 重試" };
    private readonly Button open = new() { Content = "開啟檔案" };
    private readonly InfoBar error = new() { Severity = InfoBarSeverity.Error };
    private DownloadItem? item;
    private bool refreshing;

    public DownloadProgressPage(CoreClient core, string id)
    {
        this.core = core; this.id = id;
        name.Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"];
        var content = new StackPanel { Spacing = 16 };
        foreach (var element in new UIElement[] { error, name, status, progress, transfer,
            new TextBlock { Text = "儲存位置", Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"] }, folder,
            new TextBlock { Text = "來源", Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"] }, source }) content.Children.Add(element);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        buttons.Children.Add(pause); buttons.Children.Add(resume); buttons.Children.Add(open);
        var browse = new Button { Content = "資料夾" }; buttons.Children.Add(browse);
        content.Children.Add(new TextBlock { Text = "關閉此視窗仍會繼續下載。", Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"] });
        var grid = new Grid { Padding = new Thickness(24), RowSpacing = 20 };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.Children.Add(new ScrollViewer { Content = content }); Grid.SetRow(buttons, 1); grid.Children.Add(buttons); Content = grid;
        pause.Click += async (_, _) => await Act("pause"); resume.Click += async (_, _) => await Act("continue");
        open.Click += (_, _) => Open(item!.FilePath); browse.Click += (_, _) => { if (item is not null) Open(item.Folder); };
        timer.Tick += async (_, _) => await Refresh();
        Loaded += async (_, _) => { await Refresh(); timer.Start(); }; Unloaded += (_, _) => timer.Stop();
    }
    private async Task Refresh()
    {
        if (refreshing) return; refreshing = true;
        try
        {
            item = new DownloadItem((await core.GetAsync("tasks/" + id))!.AsObject());
            name.Text = item.Name; status.Text = item.StatusText + $" · {item.Percent:0.0}%";
            progress.Value = item.Percent; progress.IsIndeterminate = item.IsIndeterminate;
            transfer.Text = $"{item.TransferText} · {item.SpeedText} · 剩餘 {item.RemainingText}";
            folder.Text = item.FilePath; source.Text = item.Url;
            pause.IsEnabled = item.CanPause; resume.IsEnabled = item.CanResume; open.IsEnabled = item.IsComplete;
        }
        catch (Exception e) { ShowError(e); }
        finally { refreshing = false; }
    }
    public void Stop() => timer.Stop();
    private async Task Act(string action) { try { await core.SendAsync(HttpMethod.Put, $"tasks/{id}/{action}"); await Refresh(); } catch (Exception e) { ShowError(e); } }
    private void Open(string path) { try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch (Exception e) { ShowError(e); } }
    private void ShowError(Exception e) { error.Message = e.Message; error.IsOpen = true; }
}
