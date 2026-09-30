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

    public DownloadWindow(JsonObject request)
    {
        this.request = request;
        Title = "確認下載 · Gopeed Native";
        surface.RequestedTheme = ((FrameworkElement)App.Window.Content).RequestedTheme;
        Content = surface;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"));
        var scale = GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0;
        var area = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary).WorkArea;
        AppWindow.Resize(new Windows.Graphics.SizeInt32(Math.Min((int)(640 * scale), area.Width), Math.Min((int)(540 * scale), area.Height)));
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(540 * scale);
            presenter.PreferredMinimumHeight = (int)(500 * scale);
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
            if (!closed) { await NativeDialogs.ShowAsync(new ContentDialog { Title = "無法連接下載核心", Content = error.Message, CloseButtonText = "關閉" }, surface.XamlRoot); Close(); }
            return;
        }
        if (closed) return;
        var page = new DownloadConfirmationPage(core, request, WinRT.Interop.WindowNative.GetWindowHandle(this));
        page.Started += ShowProgress;
        page.Cancelled += Close;
        surface.Children.Add(page);
    }

    private void ShowProgress(string id)
    {
        Title = "下載進度 · Gopeed Native";
        surface.Children.Clear();
        progress = new DownloadProgressPage(core, id); surface.Children.Add(progress);
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.PreferredMinimumHeight = (int)(400 * GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(AppWindow.Size.Width, (int)(480 * GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0)));
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hwnd);
}
