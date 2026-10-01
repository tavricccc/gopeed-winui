using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Runtime.InteropServices;

namespace Gopeed_Native.Services;

public static class WindowAppearance
{
    public static ElementTheme Theme
    {
        get
        {
            var path = Path.Combine(CoreClient.DataDirectory, "theme.txt");
            return File.Exists(path) ? File.ReadAllText(path).Trim() switch { "1" => ElementTheme.Light, "2" => ElementTheme.Dark, _ => ElementTheme.Default } : ElementTheme.Default;
        }
    }
    public static void SetIcon(Window window) => window.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"));

    public static void ApplyFrame(Window window, FrameworkElement root)
    {
        SetIcon(window);
        window.SystemBackdrop = new MicaBackdrop();
        var corners = 2;
        DwmSetWindowAttribute(WinRT.Interop.WindowNative.GetWindowHandle(window), 33, ref corners, sizeof(int));
        root.ActualThemeChanged += (_, _) => SyncTitleBar(window, root);
        SyncTitleBar(window, root);
    }

    private static void SyncTitleBar(Window window, FrameworkElement root)
    {
        var foreground = new Windows.UI.ViewManagement.AccessibilitySettings().HighContrast
            ? new Windows.UI.ViewManagement.UISettings().GetColorValue(Windows.UI.ViewManagement.UIColorType.Foreground)
            : root.ActualTheme == ElementTheme.Dark ? Microsoft.UI.Colors.White : Microsoft.UI.Colors.Black;
        var bar = window.AppWindow.TitleBar;
        bar.ButtonForegroundColor = foreground;
        bar.ButtonInactiveForegroundColor = foreground;
        bar.ButtonHoverForegroundColor = foreground;
        bar.ButtonPressedForegroundColor = foreground;
        bar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        bar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
        bar.ButtonHoverBackgroundColor = root.ActualTheme == ElementTheme.Dark ? Microsoft.UI.Colors.DarkGray : Microsoft.UI.Colors.LightGray;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);
}
