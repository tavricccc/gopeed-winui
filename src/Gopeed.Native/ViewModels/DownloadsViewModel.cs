using CommunityToolkit.Mvvm.ComponentModel;
using Gopeed_Native.Models;
using Gopeed_Native.Services;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace Gopeed_Native.ViewModels;

public sealed partial class DownloadsViewModel : ObservableObject, IDisposable
{
    public CoreClient Core { get; } = new();
    private readonly Dictionary<string, DownloadItem> items = [];
    public ObservableCollection<DownloadItem> VisibleItems { get; } = [];
    [ObservableProperty] public partial DownloadItem? Selected { get; set; }
    [ObservableProperty] public partial string Summary { get; set; } = "正在連接下載核心…";
    [ObservableProperty] public partial string Error { get; set; } = "";
    [ObservableProperty] public partial bool IsConnected { get; set; }
    public string Filter { get; set; } = "all";
    public string Search { get; set; } = "";

    public async Task InitializeAsync()
    {
        try { await Core.ConnectAsync(); IsConnected = true; await RefreshAsync(); }
        catch (Exception e) { Error = e.Message; Summary = "下載核心未連接"; }
    }
    public async Task RefreshAsync()
    {
        try
        {
            var data = (await Core.GetAsync("tasks"))!.AsArray();
            var ids = new HashSet<string>();
            foreach (var node in data)
            {
                var entry = node!.AsObject(); var id = entry["id"]!.GetValue<string>(); ids.Add(id);
                if (items.TryGetValue(id, out var item)) item.Update(entry); else items[id] = new(entry);
            }
            foreach (var id in items.Keys.Where(id => !ids.Contains(id)).ToList()) items.Remove(id);
            ApplyFilter();
            var active = items.Values.Count(i => i.Status == "running");
            Summary = $"{items.Count} 個下載 · {active} 個進行中 · {DownloadItem.FormatBytes(items.Values.Sum(i => i.Speed))}/s";
        }
        catch (Exception e) { Error = e.Message; }
    }
    public void ApplyFilter()
    {
        var filtered = items.Values.Where(i => Filter switch { "active" => i.Status is "running" or "wait" or "ready", "done" => i.Status == "done", "pause" => i.Status == "pause", "error" => i.Status == "error", _ => true })
            .Where(i => i.Name.Contains(Search, StringComparison.OrdinalIgnoreCase) || i.Url.Contains(Search, StringComparison.OrdinalIgnoreCase)).Reverse().ToList();
        for (var index = VisibleItems.Count - 1; index >= 0; index--) if (!filtered.Contains(VisibleItems[index])) VisibleItems.RemoveAt(index);
        for (var index = 0; index < filtered.Count; index++)
        {
            var current = VisibleItems.IndexOf(filtered[index]);
            if (current < 0) VisibleItems.Insert(index, filtered[index]); else if (current != index) VisibleItems.Move(current, index);
        }
        if (Selected is not null && !VisibleItems.Contains(Selected)) Selected = null;
    }
    public async Task ActAsync(string action, IEnumerable<DownloadItem> targets, bool deleteFiles = false)
    {
        try
        {
            foreach (var item in targets.ToList())
                await Core.SendAsync(action == "delete" ? HttpMethod.Delete : HttpMethod.Put, action == "delete" ? $"tasks/{item.Id}?force={deleteFiles.ToString().ToLowerInvariant()}" : $"tasks/{item.Id}/{action}");
            await RefreshAsync();
        }
        catch (Exception e) { Error = e.Message; }
    }
    public void Dispose() => Core.Dispose();
}
