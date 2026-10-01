using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Models;
using Gopeed_Native.Services;
using System.Text.Json.Nodes;

namespace Gopeed_Native.Views;

public sealed class TaskDetailsDialog : ContentDialog
{
    public TaskDetailsDialog(CoreClient core, DownloadItem item)
    {
        Title = item.Name; CloseButtonText = "關閉";
        var tabs = new Pivot { MaxHeight = 440, MinWidth = 420 };
        var info = new TextBlock { Text = $"{item.StatusText} · {item.Protocol}\n{item.TransferText}\n下載速度：{item.SpeedText}\n上傳：{DownloadItem.FormatBytes(item.Uploaded)} · {DownloadItem.FormatBytes(item.UploadSpeed)}/s\n剩餘時間：{item.RemainingText}\n新增時間：{item.CreatedAt.LocalDateTime:g}\n\n儲存位置\n{item.FilePath}\n\n來源\n{item.Url}", TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
        tabs.Items.Add(new PivotItem { Header = "資訊", Content = new ScrollViewer { Content = info } });
        var files = new StackPanel { Spacing = 8 };
        var resource = item.Data["meta"]?["res"];
        var selection = item.Data["meta"]?["opts"]?["selectFiles"]?.AsArray().Select(x => x!.GetValue<int>()).ToHashSet() ?? [];
        var index = 0;
        foreach (var file in resource?["files"]?.AsArray() ?? [])
        {
            var fileIndex = index++; if (selection.Count > 0 && !selection.Contains(fileIndex)) continue;
            var relative = Path.Combine(file!["path"]?.GetValue<string>() ?? "", file["name"]!.GetValue<string>());
            var path = resource?["name"]?.GetValue<string>() is { Length: > 0 } ? Path.Combine(item.FilePath, relative.TrimStart('/', '\\')) : item.FilePath;
            var row = new StackPanel { Spacing = 6 };
            row.Children.Add(new TextBlock { Text = relative, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true });
            row.Children.Add(new TextBlock { Text = DownloadItem.FormatBytes(file["size"]!.GetValue<long>()), Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"] });
            var commands = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            var open = NativeButtons.Create("開啟", "\uE8E5"); open.IsEnabled = item.IsComplete && File.Exists(path); open.Click += (_, _) => Run(() => FileActions.Open(path)); commands.Children.Add(open);
            var folder = NativeButtons.Create("在資料夾中顯示", "\uE8B7"); folder.Click += (_, _) => Run(() => FileActions.Reveal(path, item.Folder)); commands.Children.Add(folder);
            var share = NativeButtons.Create("分享", "\uE72D"); share.IsEnabled = open.IsEnabled; share.Click += (_, _) => Run(() => ShareFiles.Show(App.WindowHandle, path)); commands.Children.Add(share);
            row.Children.Add(commands); files.Children.Add(row);
        }
        if (files.Children.Count == 0) files.Children.Add(new TextBlock { Text = "取得檔案資訊後將顯示在這裡。" });
        tabs.Items.Add(new PivotItem { Header = "檔案", Content = new ScrollViewer { Content = files } });
        var stats = new TextBlock { Text = "正在取得連線資訊…", TextWrapping = TextWrapping.Wrap };
        tabs.Items.Add(new PivotItem { Header = "連線", Content = new ScrollViewer { Content = stats } }); Content = tabs;
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
    private void Run(Action action) { try { action(); } catch (Exception error) { Title = UserError.Message(error); } }
}
