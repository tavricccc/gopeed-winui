using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Models;
using Gopeed_Native.Services;
using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Gopeed_Native.Views;

public sealed class DownloadProgressPage : Page
{
    private readonly CoreClient core;
    private readonly string id;
    private readonly Action close;
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly StackPanel body = new() { Spacing = 10, Padding = new Thickness(20,16,20,14) };
    private readonly TextBlock name = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock kind = new() { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock status = new() { FontSize = 12, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock transfer = new() { FontSize = 12 };
    private readonly TextBlock speed = new() { FontSize = 12 };
    private readonly TextBlock remaining = new() { FontSize = 12 };
    private readonly ProgressBar progress = new() { Maximum = 100 };
    private readonly TextBlock folder = new() { FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly TextBlock source = new() { FontSize = 12, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
    private readonly Button primary = NativeButtons.Create("正在連接…", "\uE896", true);
    private readonly Button browse = NativeButtons.Create("資料夾", "\uE8B7");
    private readonly Button stopSeed = NativeButtons.Create("停止做種", "\uE769");
    private readonly CheckBox closeAfterOpen = new() { Content = "開啟後關閉", FontSize = 12 };
    private readonly InfoBar error = new() { Severity = InfoBarSeverity.Error };
    private DownloadItem? item;
    private bool refreshing;
    public event Action? LayoutChanged;
    public event Action<string>? TitleChanged;
    public double PreferredHeight(double width) { body.Measure(new Windows.Foundation.Size(width,double.PositiveInfinity)); return body.DesiredSize.Height + 58; }

    public DownloadProgressPage(CoreClient core, string id, Action close)
    {
        this.core = core; this.id = id; this.close = close;
        var muted = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        foreach (var text in new[] { kind, status, transfer, remaining, folder }) text.Foreground = muted;
        name.DragStarting += DragFile; ToolTipService.SetToolTip(name, "下載完成後可拖曳檔案");
        primary.IsEnabled = false; closeAfterOpen.IsChecked = UiPreferences.Load().CloseProgressAfterOpen;
        closeAfterOpen.Checked += SaveClosePreference; closeAfterOpen.Unchecked += SaveClosePreference;
        var heading = new Grid { ColumnSpacing = 8 }; heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }); heading.Children.Add(kind); Grid.SetColumn(name,1); heading.Children.Add(name);
        var metrics = new Grid { ColumnSpacing = 12 }; metrics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }); metrics.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); metrics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }); metrics.Children.Add(transfer); Grid.SetColumn(speed,1); metrics.Children.Add(speed); Grid.SetColumn(remaining,2); remaining.HorizontalAlignment = HorizontalAlignment.Right; metrics.Children.Add(remaining);
        var copy = new Button { Content = "複製網址", Style = (Style)Application.Current.Resources["SubtleButtonStyle"] }; copy.Click += (_, _) => { if (item is not null) FileActions.Copy(item.Url); };
        var sourceButton = new Button { Style = (Style)Application.Current.Resources["SubtleButtonStyle"], Content = new FontIcon { Glyph = "\uE712", FontSize = 14 }, Flyout = new Flyout { Content = new ScrollViewer { Width = 380, MaxHeight = 220, Content = new StackPanel { Spacing = 8, Children = { source, copy } } } } };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(sourceButton,"來源網址"); ToolTipService.SetToolTip(sourceButton,"來源網址");
        foreach (var control in new UIElement[] { error, heading, folder, metrics, progress, status }) body.Children.Add(control);
        body.SizeChanged += (_, _) => LayoutChanged?.Invoke();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right };
        stopSeed.Visibility = Visibility.Collapsed; buttons.Children.Add(primary); buttons.Children.Add(browse); buttons.Children.Add(stopSeed);
        var dismiss = new Button { Content = "關閉" }; dismiss.Click += (_, _) => close(); buttons.Children.Add(dismiss);
        var extras = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { sourceButton, closeAfterOpen } };
        var footer = new Grid { ColumnSpacing = 8 }; footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) }); footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); footer.Children.Add(extras); Grid.SetColumn(buttons,1); footer.Children.Add(buttons);
        var footerBorder = new Border { BorderThickness = new Thickness(0,1,0,0), BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"], Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["LayerFillColorDefaultBrush"], Padding = new Thickness(20,12,20,12), Child = footer };
        var layout = new Grid(); layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1,GridUnitType.Star) }); layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); layout.Children.Add(new ScrollViewer { Content = body, HorizontalScrollMode = ScrollMode.Disabled, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }); Grid.SetRow(footerBorder,1); layout.Children.Add(footerBorder); Content = layout;
        primary.Click += async (_, _) => await PrimaryAction();
        browse.Click += (_, _) => { try { if (item is not null) FileActions.Reveal(item.FilePath,item.Folder); } catch (Exception failure) { ShowError(failure); } };
        stopSeed.Click += async (_, _) => { try { await core.SendAsync(HttpMethod.Put,$"tasks/{id}/pause"); await Refresh(); } catch (Exception failure) { ShowError(failure); } };
        timer.Tick += async (_, _) => await Refresh(); Loaded += async (_, _) => { timer.Start(); await Refresh(); }; Unloaded += (_, _) => Stop();
    }
    private async Task Refresh()
    {
        if (refreshing) return; refreshing = true;
        try
        {
            item = new DownloadItem((await core.GetAsync("tasks/"+id))!.AsObject()); var action = item.PrimaryAction;
            NativeButtons.SetContent(primary,action.Label,action.Glyph); primary.IsEnabled = item.CanAct;
            name.Text = item.Name; name.CanDrag = item.IsComplete && !item.IsProcessing; kind.Text = item.Protocol;
            ToolTipService.SetToolTip(name,item.Name); folder.Text = item.OpenPath; ToolTipService.SetToolTip(folder,item.OpenPath); source.Text = item.Url;
            transfer.Text = item.TransferSizeText; speed.Text = item.SpeedText; remaining.Text = item.IsComplete ? "" : $"剩餘 {item.RemainingText}";
            status.Text = item.IsProcessing ? item.ExtractionText : item.ExtractionStatus == "error" ? "解壓縮失敗，原始檔案仍可開啟。" : item.IsComplete ? item.Uploading ? $"做種中 · 已上傳 {DownloadItem.FormatBytes(item.Uploaded)}" : "下載完成" : $"{item.StatusText}" + (item.Size > 0 ? $" · {item.Percent:0.0}%" : "");
            progress.Value = item.IsProcessing ? item.Data["progress"]?["extractProgress"]?.GetValue<double>() ?? 0 : item.Percent; progress.IsIndeterminate = false; progress.Visibility = item.IsComplete && !item.IsProcessing || item.Size <= 0 ? Visibility.Collapsed : Visibility.Visible;
            closeAfterOpen.Visibility = item.IsComplete && !item.IsProcessing && !item.Uploading ? Visibility.Visible : Visibility.Collapsed;
            stopSeed.Visibility = item.Uploading ? Visibility.Visible : Visibility.Collapsed;
            TitleChanged?.Invoke($"{item.StatusText} · {item.Name}"); LayoutChanged?.Invoke();
            if (item.IsComplete && !item.IsProcessing && !item.Uploading) timer.Stop();
        }
        catch (Exception failure) { ShowError(failure); }
        finally { refreshing = false; }
    }
    private async Task PrimaryAction()
    {
        if (item is null) return; primary.IsEnabled = false;
        try { var action = item.PrimaryAction; if (action.Key == "open") { FileActions.Open(item.OpenPath); if (closeAfterOpen.IsChecked == true) close(); } else if (item.CanAct) { await core.SendAsync(HttpMethod.Put,$"tasks/{id}/{action.Key}"); await Refresh(); } }
        catch (Exception failure) { ShowError(failure); }
        finally { primary.IsEnabled = item?.CanAct == true; }
    }
    private void SaveClosePreference(object sender,RoutedEventArgs args) { var prefs = UiPreferences.Load(); prefs.CloseProgressAfterOpen = closeAfterOpen.IsChecked == true; prefs.Save(); }
    private async void DragFile(UIElement sender,DragStartingEventArgs args)
    {
        if (item is not { IsComplete: true, IsProcessing: false }) { args.Cancel = true; return; }
        var deferral = args.GetDeferral();
        try { IStorageItem file = Directory.Exists(item.OpenPath) ? await StorageFolder.GetFolderFromPathAsync(item.OpenPath) : await StorageFile.GetFileFromPathAsync(item.OpenPath); args.Data.SetStorageItems([file]); args.Data.RequestedOperation = DataPackageOperation.Copy; args.AllowedOperations = DataPackageOperation.Copy; }
        catch (Exception failure) { args.Cancel = true; ShowError(failure); }
        finally { deferral.Complete(); }
    }
    public void Stop() => timer.Stop();
    private void ShowError(Exception failure) { error.Message = UserError.Message(failure); error.IsOpen = true; LayoutChanged?.Invoke(); }
}
