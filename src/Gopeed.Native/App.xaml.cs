using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Gopeed_Native;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// The main application window. Use <c>App.Window</c> from any class that needs
    /// the window reference (for dialogs, pickers, interop, etc.).
    /// </summary>
    public static Window Window { get; private set; } = null!;

    /// <summary>
    /// The UI thread dispatcher. Use <c>App.DispatcherQueue</c> to marshal calls
    /// to the UI thread. Fully qualified to avoid CS0104 ambiguity with
    /// <see cref="Windows.System.DispatcherQueue"/>.
    /// </summary>
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;

    /// <summary>
    /// The native window handle (HWND). Use for file pickers,
    /// <c>DataTransferManager</c>, and any WinRT interop that requires
    /// <c>InitializeWithWindow</c>.
    /// </summary>
    public static nint WindowHandle =>
        WinRT.Interop.WindowNative.GetWindowHandle(Window);

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            System.IO.Directory.CreateDirectory(Services.CoreClient.DataDirectory);
            System.IO.File.AppendAllText(System.IO.Path.Combine(Services.CoreClient.DataDirectory, "frontend-error.log"), $"{DateTimeOffset.Now}\n{e.Exception}\n");
        };
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        if (Window is not null) { Window.Activate(); return; }
        var instance = AppInstance.FindOrRegisterForKey("GopeedNative.Main");
        if (!instance.IsCurrent)
        {
            await instance.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs());
            Exit(); return;
        }
        Window = new MainWindow();
        Window.Closed += (_, _) => Window = null!;
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        instance.Activated += (_, activation) => DispatcherQueue.TryEnqueue(() => HandleActivation(activation));
        Window.Activate();
        HandleActivation(AppInstance.GetCurrent().GetActivatedEventArgs());
    }

    private void HandleActivation(AppActivationArguments activation)
    {
        if (Window is null) { Window = new MainWindow(); Window.Closed += (_, _) => Window = null!; }
        Window.Activate();
        var link = activation.Data switch
        {
            Windows.ApplicationModel.Activation.IProtocolActivatedEventArgs protocol => protocol.Uri.AbsoluteUri,
            Windows.ApplicationModel.Activation.ILaunchActivatedEventArgs launch => Services.GopeedLink.FromCommandLine(launch.Arguments),
            _ => null
        };
        if (link is not null) ((MainWindow)Window).OpenProtocol(link);
        if (activation.Data is Windows.ApplicationModel.Activation.ILaunchActivatedEventArgs launchArgs)
        {
            var request = System.Text.RegularExpressions.Regex.Match(launchArgs.Arguments, @"--download-request\s+([a-f0-9]{32})");
            if (request.Success) ((MainWindow)Window).OpenDownloadRequest(request.Groups[1].Value);
        }
    }
}
