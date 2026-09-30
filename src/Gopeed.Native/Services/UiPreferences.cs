using System.Text.Json;

namespace Gopeed_Native.Services;

public sealed class UiPreferences
{
    public bool RememberDownloadDirectory { get; set; } = true;
    public string LastDownloadDirectory { get; set; } = "";
    public bool CloseProgressAfterOpen { get; set; } = true;
    private static string FilePath => Path.Combine(CoreClient.DataDirectory, "preferences.json");
    public static UiPreferences Load() => File.Exists(FilePath)
        ? JsonSerializer.Deserialize<UiPreferences>(File.ReadAllText(FilePath))!
        : new();
    public void Save()
    {
        Directory.CreateDirectory(CoreClient.DataDirectory);
        File.WriteAllText(FilePath + ".tmp", JsonSerializer.Serialize(this));
        File.Move(FilePath + ".tmp", FilePath, true);
    }
}
