using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;

namespace Gopeed_Native.Models;

public sealed class DownloadItem : ObservableObject
{
    public string Id { get; }
    public JsonObject Data { get; private set; }
    public DownloadItem(JsonObject data) { Id = data["id"]!.GetValue<string>(); Data = data; }
    public string Name => Data["name"]!.GetValue<string>();
    public string Status => Data["status"]!.GetValue<string>();
    public string StatusText => Status switch { "running" => "下載中", "done" => Data["uploading"]?.GetValue<bool>() == true ? "做種中" : "已完成", "pause" => "已暫停", "error" => "下載失敗", "wait" => "等待中", _ => "準備中" };
    public string Url => Data["meta"]?["req"]?["url"]?.GetValue<string>() ?? "";
    public string Folder => Data["meta"]?["opts"]?["path"]?.GetValue<string>() ?? "";
    public string Protocol => Data["protocol"]?.GetValue<string>().ToUpperInvariant() ?? "";
    public long Size => Data["meta"]?["res"]?["size"]?.GetValue<long>() ?? 0;
    public long Downloaded => Data["progress"]?["downloaded"]?.GetValue<long>() ?? 0;
    public long Speed => Data["progress"]?["speed"]?.GetValue<long>() ?? 0;
    public double Percent => Status == "done" ? 100 : Size > 0 ? Math.Min(100, Downloaded * 100.0 / Size) : 0;
    public bool IsIndeterminate => Size <= 0 && Status == "running";
    public bool CanPause => Status is "running" or "wait" or "ready" || Data["uploading"]?.GetValue<bool>() == true;
    public bool CanResume => Status is "pause" or "error";
    public bool IsComplete => Status == "done";
    public string SizeText => Size > 0 ? FormatBytes(Size) : "大小未知";
    public string TransferText => $"{FormatBytes(Downloaded)} / {SizeText}";
    public string SpeedText => Status == "running" ? FormatBytes(Speed) + "/s" : "—";
    public string RemainingText => Speed > 0 && Size > Downloaded ? FormatTime((Size - Downloaded) / Speed) : "—";
    public string FilePath
    {
        get
        {
            var resource = Data["meta"]?["res"];
            var name = resource?["name"]?.GetValue<string>();
            var file = resource?["files"]?.AsArray().FirstOrDefault();
            var custom = Data["meta"]?["opts"]?["name"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(name)) return Path.Combine(Folder, string.IsNullOrEmpty(custom) ? name : custom);
            return Path.Combine(Folder, file?["path"]?.GetValue<string>() ?? "", string.IsNullOrEmpty(custom) ? file?["name"]?.GetValue<string>() ?? Name : custom);
        }
    }
    public void Update(JsonObject data)
    {
        if (Data.ToJsonString() == data.ToJsonString()) return;
        Data = data;
        OnPropertyChanged(string.Empty);
    }
    public static string FormatBytes(long value) => value switch
    {
        >= 1L << 30 => $"{value / (double)(1L << 30):0.00} GB",
        >= 1L << 20 => $"{value / (double)(1L << 20):0.0} MB",
        >= 1L << 10 => $"{value / 1024.0:0.0} KB",
        _ => $"{value} B"
    };
    private static string FormatTime(long seconds) => seconds >= 3600 ? $"{seconds / 3600} 小時 {seconds % 3600 / 60} 分" : seconds >= 60 ? $"{seconds / 60} 分 {seconds % 60} 秒" : $"{seconds} 秒";
}
