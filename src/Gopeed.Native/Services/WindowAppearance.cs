using Microsoft.UI.Xaml;

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
}
