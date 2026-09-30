using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Services;
using System.Text.Json.Nodes;

namespace Gopeed_Native.Views;

public sealed class DownloadWindow : Window
{
    private readonly Grid surface = new();
    private readonly CoreClient core;
    private readonly JsonObject request;
    private bool closed;
    private AddDownloadDialog? dialog;
    private DownloadProgressPage? progress;

    public DownloadWindow(CoreClient core, JsonObject request)
    {
        this.core = core; this.request = request;
        Title = "確認下載 · Gopeed Native";
        surface.RequestedTheme = ((FrameworkElement)App.Window.Content).RequestedTheme;
        Content = surface;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"));
        var scale = GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0;
        var area = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary).WorkArea;
        AppWindow.Resize(new Windows.Graphics.SizeInt32(Math.Min((int)(640 * scale), area.Width), Math.Min((int)(700 * scale), area.Height)));
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(540 * scale);
            presenter.PreferredMinimumHeight = (int)(600 * scale);
        }
        Closed += (_, _) => { closed = true; dialog?.Hide(); progress?.Stop(); };
        surface.Loaded += Confirm;
    }

    private async void Confirm(object sender, RoutedEventArgs e)
    {
        surface.Loaded -= Confirm;
        dialog = new AddDownloadDialog(core, request, WinRT.Interop.WindowNative.GetWindowHandle(this));
        await NativeDialogs.ShowAsync(dialog, surface.XamlRoot);
        if (closed) return;
        if (dialog.CreatedTaskId is not { } id) { Close(); return; }
        Title = "下載進度 · Gopeed Native";
        progress = new DownloadProgressPage(core, id); surface.Children.Add(progress);
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.PreferredMinimumHeight = (int)(400 * GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(AppWindow.Size.Width, (int)(480 * GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0)));
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hwnd);
}
