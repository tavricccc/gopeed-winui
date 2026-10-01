using System.Text.RegularExpressions;

namespace Gopeed_Native.Services;

public static class DownloadSources
{
    public static string? FromArguments(string arguments)
    {
        foreach (Match match in Regex.Matches(arguments, "\"([^\"]+)\"|(\\S+)"))
        {
            var value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            if (value.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase) && File.Exists(value)) return value;
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "magnet" or "ed2k" or "http" or "https") return value;
        }
        return null;
    }
}
