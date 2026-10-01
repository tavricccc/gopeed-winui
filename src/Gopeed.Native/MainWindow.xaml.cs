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

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        Closed += (_, _) => Services.ShareFiles.Release(hwnd);
        var scale = GetDpiForWindow(hwnd) / 96.0;
        AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(1120 * scale), (int)(640 * scale)));
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(780 * scale);
            presenter.PreferredMinimumHeight = (int)(520 * scale);
        }
        ((FrameworkElement)Content).RequestedTheme = Services.WindowAppearance.Theme;
        Services.WindowAppearance.ApplyFrame(this, (FrameworkElement)Content);

        // Navigate the root frame to the main page on startup.
        RootFrame.Navigate(typeof(MainPage));
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hwnd);
    public void OpenProtocol(string link) => ((MainPage)RootFrame.Content).OpenProtocol(link);
    public void ReportError(string message) => ((MainPage)RootFrame.Content).ViewModel.Error = message;
}
