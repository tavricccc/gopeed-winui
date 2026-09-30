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
        Services.WindowAppearance.InitializeProcess();
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
        var instance = AppInstance.FindOrRegisterForKey("GopeedNative.Main");
        if (!instance.IsCurrent)
        {
            await instance.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs());
            Exit(); return;
        }
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        instance.Activated += (_, activation) => DispatcherQueue.TryEnqueue(() => HandleActivation(activation));
        HandleActivation(AppInstance.GetCurrent().GetActivatedEventArgs());
    }

    private async void HandleActivation(AppActivationArguments activation)
    {
        try
        {
            if (activation.Data is Windows.ApplicationModel.Activation.ILaunchActivatedEventArgs launchArgs)
            {
                var match = System.Text.RegularExpressions.Regex.Match(launchArgs.Arguments, @"--download-request\s+([a-f0-9]{32})");
                if (match.Success)
                {
                    var path = System.IO.Path.Combine(Services.CoreClient.DataDirectory, "pending-downloads", match.Groups[1].Value + ".json");
                    var request = System.Text.Json.Nodes.JsonNode.Parse(await System.IO.File.ReadAllTextAsync(path))!.AsObject();
                    System.IO.File.Delete(path);
                    new Views.DownloadWindow(request).Activate(); return;
                }
            }
            var link = activation.Data switch
            {
                Windows.ApplicationModel.Activation.IProtocolActivatedEventArgs protocol => protocol.Uri.AbsoluteUri,
                Windows.ApplicationModel.Activation.ILaunchActivatedEventArgs launch => Services.GopeedLink.FromCommandLine(launch.Arguments),
                _ => null
            };
            if (link is not null && Services.GopeedLink.Parse(link) is { Route: "create" } create)
            {
                new Views.DownloadWindow(create.Parameters ?? new System.Text.Json.Nodes.JsonObject()).Activate(); return;
            }
            EnsureMainWindow().Activate();
            if (link is not null) ((MainWindow)Window).OpenProtocol(link);
        }
        catch (Exception error)
        {
            var main = EnsureMainWindow(); main.Activate(); main.ReportError("無法開啟下載：" + error.Message);
        }
    }
    private MainWindow EnsureMainWindow()
    {
        if (Window is null) { Window = new MainWindow(); Window.Closed += (_, _) => Window = null!; }
        return (MainWindow)Window;
    }
}
