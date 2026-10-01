using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Models;
using Gopeed_Native.Services;
using System.Text.Json.Nodes;

namespace Gopeed_Native.Views;

public sealed class TaskDetailsDialog : ContentDialog
{
    private readonly InfoBar errorBar = new() { Severity = InfoBarSeverity.Error };
    private sealed record FileEntry(string Name, string Path, long Size) { public override string ToString() => $"{Name} · {DownloadItem.FormatBytes(Size)}"; }
    public TaskDetailsDialog(CoreClient core, DownloadItem item)
    {
        Title = item.Name; CloseButtonText = "關閉";
        var tabs = new Pivot { MaxHeight = 440, MinWidth = 420 };
        var info = new TextBlock { Text = $"{item.StatusText} · {item.Protocol}\n{item.TransferText}\n下載速度：{item.SpeedText}\n上傳：{DownloadItem.FormatBytes(item.Uploaded)} · {DownloadItem.FormatBytes(item.UploadSpeed)}/s\n剩餘時間：{item.RemainingText}\n新增時間：{item.CreatedAt.LocalDateTime:g}\n\n儲存位置\n{item.OpenPath}\n\n來源\n{item.Url}", TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
        tabs.Items.Add(new PivotItem { Header = "資訊", Content = new ScrollViewer { Content = info } });
        var entries = new List<FileEntry>(); var resource = item.Data["meta"]?["res"];
        var selection = item.Data["meta"]?["opts"]?["selectFiles"]?.AsArray().Select(x => x!.GetValue<int>()).ToHashSet() ?? [];
        var index = 0;
        foreach (var file in resource?["files"]?.AsArray() ?? [])
        {
            var fileIndex = index++; if (selection.Count > 0 && !selection.Contains(fileIndex)) continue;
            var relative = Path.Combine(file!["path"]?.GetValue<string>() ?? "", file["name"]!.GetValue<string>());
            var path = resource?["name"]?.GetValue<string>() is { Length: > 0 } ? Path.Combine(item.FilePath, relative.TrimStart('/', '\\')) : item.FilePath;
            entries.Add(new FileEntry(relative, path, file["size"]!.GetValue<long>()));
        }
        var list = new ListView { ItemsSource = entries, Height = 250, SelectionMode = ListViewSelectionMode.Single };
        var selectedPath = new TextBlock { TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
        var commands = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var open = NativeButtons.Create("開啟", "\uE8E5", true); var folder = NativeButtons.Create("在資料夾中顯示", "\uE8B7"); var share = NativeButtons.Create("分享", "\uE72D");
        open.IsEnabled = folder.IsEnabled = share.IsEnabled = false;
        list.SelectionChanged += (_, _) => { var entry = list.SelectedItem as FileEntry; selectedPath.Text = entry?.Path ?? ""; folder.IsEnabled = entry is not null; open.IsEnabled = share.IsEnabled = item.IsComplete && !item.IsProcessing && entry is not null && File.Exists(entry.Path); };
        open.Click += (_, _) => { if (list.SelectedItem is FileEntry entry) Run(() => FileActions.Open(entry.Path)); };
        folder.Click += (_, _) => { if (list.SelectedItem is FileEntry entry) Run(() => FileActions.Reveal(entry.Path, item.Folder)); };
        share.Click += (_, _) => { if (list.SelectedItem is FileEntry entry) Run(() => ShareFiles.Show(App.WindowHandle, entry.Path)); };
        commands.Children.Add(open); commands.Children.Add(folder); commands.Children.Add(share);
        var files = new StackPanel { Spacing = 12, Children = { list, selectedPath, commands } }; if (entries.Count == 0) files.Children.Add(new TextBlock { Text = "取得檔案資訊後將顯示在這裡。" }); else list.SelectedIndex = 0;
        tabs.Items.Add(new PivotItem { Header = "檔案", Content = files });
        var stats = new TextBlock { Text = "正在取得連線資訊…", TextWrapping = TextWrapping.Wrap }; tabs.Items.Add(new PivotItem { Header = "連線", Content = new ScrollViewer { Content = stats } });
        Content = new StackPanel { Spacing = 12, Children = { errorBar, tabs } };
        Opened += async (_, _) =>
        {
            try
            {
                var data = await core.GetAsync("tasks/" + item.Id + "/stats");
                if (data is null) { stats.Text = "目前沒有連線資訊。"; return; }
                if (item.Protocol == "BT") stats.Text = $"已知節點：{data["totalPeers"]}\n連線節點：{data["activePeers"]}\n完整種子：{data["connectedSeeders"]}\n已分享：{DownloadItem.FormatBytes(data["seedBytes"]!.GetValue<long>())}\n分享比例：{data["seedRatio"]}\n做種時間：{data["seedTime"]} 秒";
                else if (data["connections"] is JsonArray connections) stats.Text = string.Join("\n", connections.Select((x, i) => $"連線 {i + 1} · {DownloadItem.FormatBytes(x!["downloaded"]!.GetValue<long>())} · {(x["completed"]!.GetValue<bool>() ? "已完成" : x["failed"]!.GetValue<bool>() ? "重試中" : "下載中")} · 重試 {x["retryTimes"]} 次"));
                else stats.Text = "目前沒有連線資訊。";
            }
            catch (Exception error) { stats.Text = UserError.Message(error); }
        };
    }
    private void Run(Action action) { try { action(); } catch (Exception error) { errorBar.Message = UserError.Message(error); errorBar.IsOpen = true; } }
}
