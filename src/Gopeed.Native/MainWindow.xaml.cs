using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Gopeed_Native;

/// <summary>
/// The application window. This hosts a Frame that displays pages. Add your
/// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
/// can use Page features such as navigation events and the Loaded lifecycle.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"));
        var scale = GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0;
        AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(1120 * scale), (int)(740 * scale)));
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(640 * scale);
            presenter.PreferredMinimumHeight = (int)(480 * scale);
        }
        var themePath = System.IO.Path.Combine(Services.CoreClient.DataDirectory, "theme.txt");
        if (System.IO.File.Exists(themePath))
            ((FrameworkElement)Content).RequestedTheme = System.IO.File.ReadAllText(themePath).Trim() switch { "1" => ElementTheme.Light, "2" => ElementTheme.Dark, _ => ElementTheme.Default };
        ((FrameworkElement)Content).ActualThemeChanged += (_, _) => SyncTitleBarTheme();
        SyncTitleBarTheme();

        // Navigate the root frame to the main page on startup.
        RootFrame.Navigate(typeof(MainPage));
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hwnd);
    public void OpenProtocol(string link) => ((MainPage)RootFrame.Content).OpenProtocol(link);
    public void OpenDownloadRequest(string id) => ((MainPage)RootFrame.Content).OpenDownloadRequest(id);
    private void SyncTitleBarTheme()
    {
        var dark = ((FrameworkElement)Content).ActualTheme == ElementTheme.Dark;
        var foreground = new Windows.UI.ViewManagement.AccessibilitySettings().HighContrast
            ? new Windows.UI.ViewManagement.UISettings().GetColorValue(Windows.UI.ViewManagement.UIColorType.Foreground)
            : dark ? Microsoft.UI.Colors.White : Microsoft.UI.Colors.Black;
        var titlebar = AppWindow.TitleBar;
        titlebar.ButtonForegroundColor = foreground;
        titlebar.ButtonInactiveForegroundColor = foreground;
        titlebar.ButtonHoverForegroundColor = foreground;
        titlebar.ButtonPressedForegroundColor = foreground;
        titlebar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        titlebar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
        titlebar.ButtonHoverBackgroundColor = dark ? Microsoft.UI.Colors.DarkGray : Microsoft.UI.Colors.LightGray;
    }
}
