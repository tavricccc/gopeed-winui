using System.Text.Json.Nodes;

namespace Gopeed_Native.Services;

public sealed record AvailableUpdate(string Version, string Url);
public static class UpdateService
{
    public static async Task<AvailableUpdate?> CheckAsync(CoreClient core)
    {
        var releases = JsonNode.Parse(await core.FetchTextAsync("https://api.github.com/repos/tavricccc/gopeed-winui/releases?per_page=10"))!.AsArray();
        var current = typeof(App).Assembly.GetName().Version!;
        foreach (var release in releases.Where(x => x?["draft"]?.GetValue<bool>() != true))
        {
            var tag = release!["tag_name"]!.GetValue<string>();
            if (Version.TryParse(tag.TrimStart('v'), out var version) && version > current) return new(tag, release["html_url"]!.GetValue<string>());
        }
        return null;
    }
}
