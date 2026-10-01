using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Services;
using System.Text.Json.Nodes;

namespace Gopeed_Native.Views;

public sealed class DownloadWindow : Window
{
    private readonly Grid surface = new();
    private readonly CoreClient core = new();
    private readonly JsonObject request;
    private bool closed;
    private DownloadProgressPage? progress;
    private Func<double,double>? preferredHeight;
    private double contentWidth = 560;
    private bool fitQueued;
    private Windows.Graphics.SizeInt32 lastClientSize;

    public DownloadWindow(JsonObject request)
    {
        this.request = request;
        Title = "確認下載 · Gopeed Native";
        surface.RequestedTheme = WindowAppearance.Theme;
        Content = surface;
        WindowAppearance.SetIcon(this);
        var scale = GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0;
        var area = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary).WorkArea;
        AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(Math.Min((int)(560 * scale), area.Width), Math.Min((int)(280 * scale), area.Height)));
        AppWindow.Move(new Windows.Graphics.PointInt32(area.X + (area.Width - AppWindow.Size.Width) / 2, area.Y + (area.Height - AppWindow.Size.Height) / 2));
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(480 * scale);
            presenter.PreferredMinimumHeight = (int)(150 * scale);
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsAlwaysOnTop = true;
        }
        Closed += (_, _) => { closed = true; progress?.Stop(); core.Dispose(); };
        surface.Loaded += Confirm;
    }

    private async void Confirm(object sender, RoutedEventArgs e)
    {
        surface.Loaded -= Confirm;
        try { await core.ConnectAsync(); }
        catch (Exception error)
        {
            if (!closed) { await NativeDialogs.ShowAsync(new ContentDialog { Title = "無法載入下載", Content = UserError.Message(error), CloseButtonText = "關閉" }, surface.XamlRoot); Close(); }
            return;
        }
        if (closed) return;
        if (await TryUpdateSource()) return;
        var page = new DownloadConfirmationPage(core, request, WinRT.Interop.WindowNative.GetWindowHandle(this));
        preferredHeight = page.PreferredHeight;
        page.LayoutChanged += RequestFit;
        page.Started += ShowProgress;
        page.Cancelled += Close;
        surface.Children.Add(page);
        RequestFit();
    }

    private async Task<bool> TryUpdateSource()
    {
        var pending = UiPreferences.Load().PendingUpdateTaskId; if (pending.Length == 0) return false;
        try
        {
            var item = new Gopeed_Native.Models.DownloadItem((await core.GetAsync("tasks/" + Uri.EscapeDataString(pending)))!.AsObject());
            var error = new InfoBar { Severity = InfoBarSeverity.Error };
            var panel = new StackPanel { Spacing = 12, Children = { error, new TextBlock { Text = item.Name, TextWrapping = TextWrapping.Wrap }, new TextBlock { Text = request["req"]?["url"]?.GetValue<string>() ?? "", TextWrapping = TextWrapping.Wrap } } };
            var dialog = new ContentDialog { Title = "用此連結繼續原本的下載？", Content = panel, PrimaryButtonText = "更新並繼續", IsPrimaryButtonEnabled = item.CanEditSource, SecondaryButtonText = "建立新下載", CloseButtonText = "取消" };
            dialog.PrimaryButtonClick += async (_, click) => { var deferral = click.GetDeferral(); try { await core.SendAsync(System.Net.Http.HttpMethod.Patch, "tasks/" + pending, new JsonObject { ["req"] = request["req"]!.DeepClone() }); await core.SendAsync(System.Net.Http.HttpMethod.Put, "tasks/" + pending + "/continue"); } catch (Exception failure) { click.Cancel = true; error.Message = UserError.Message(failure); error.IsOpen = true; } finally { deferral.Complete(); } };
            var result = await NativeDialogs.ShowAsync(dialog, surface.XamlRoot);
            if (result == ContentDialogResult.None) { Close(); return true; }
            var prefs = UiPreferences.Load(); prefs.PendingUpdateTaskId = ""; prefs.Save();
            if (result == ContentDialogResult.Primary) { ShowProgress(pending); return true; }
            return false;
        }
        catch (Exception error)
        {
            var dialog = new ContentDialog { Title = "無法更新原本的下載", Content = UserError.Message(error), PrimaryButtonText = "建立新下載", CloseButtonText = "取消" };
            if (await NativeDialogs.ShowAsync(dialog, surface.XamlRoot) != ContentDialogResult.Primary) { Close(); return true; }
            var prefs = UiPreferences.Load(); prefs.PendingUpdateTaskId = ""; prefs.Save(); return false;
        }
    }

    private void ShowProgress(string id)
    {
        Title = "下載進度 · Gopeed Native";
        surface.Children.Clear();
        progress = new DownloadProgressPage(core, id, Close); surface.Children.Add(progress);
        contentWidth = 520; preferredHeight = progress.PreferredHeight;
        progress.LayoutChanged += RequestFit; progress.TitleChanged += title => Title = title + " · Gopeed Native";
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) { presenter.IsAlwaysOnTop = false; presenter.IsMinimizable = true; }
        RequestFit();
    }

    private void RequestFit()
    {
        if (closed || fitQueued) return; fitQueued = true;
        surface.DispatcherQueue.TryEnqueue(() =>
        {
            fitQueued = false; if (closed || preferredHeight is null) return;
            var scale = surface.XamlRoot.RasterizationScale;
            var area = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary).WorkArea;
            var width = Math.Min(contentWidth, area.Width / scale - 16);
            var height = Math.Clamp(preferredHeight(width), 150, Math.Min(480, area.Height / scale - 64));
            var size = new Windows.Graphics.SizeInt32((int)Math.Round(width * scale), (int)Math.Ceiling(height * scale));
            if (size.Width == lastClientSize.Width && size.Height == lastClientSize.Height) return;
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.IsResizable = true;
            AppWindow.ResizeClient(size); lastClientSize = size;
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter fixedPresenter) fixedPresenter.IsResizable = false;
        });
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hwnd);
}
