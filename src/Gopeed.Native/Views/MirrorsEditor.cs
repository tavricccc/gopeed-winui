using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json.Nodes;
using Gopeed_Native.Services;

namespace Gopeed_Native.Views;

internal sealed class MirrorsEditor : StackPanel
{
    private readonly ToggleSwitch enabled = new() { Header = "使用 GitHub 鏡像" };
    private readonly ListView list = new() { MaxHeight = 160 };
    private readonly ComboBox type = new() { Header = "鏡像類型", Items = { "GitHub Proxy", "jsDelivr" }, SelectedIndex = 0 };
    private readonly TextBox url = new() { Header = "鏡像網址", PlaceholderText = "https://…" };
    private readonly InfoBar message = new() { Severity = InfoBarSeverity.Error };
    private sealed record Mirror(string Type, string Url) { public override string ToString() => $"{Type} · {Url}"; }
    public MirrorsEditor()
    {
        Spacing = 12; foreach (var control in new UIElement[] { enabled, new TextBlock { Text = "用於 Tracker 訂閱與更新下載；優先使用清單第一個鏡像。", TextWrapping = TextWrapping.Wrap }, message, list, type, url }) Children.Add(control);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var add = new Button { Content = "新增鏡像" }; add.Click += (_, _) => { if (!Uri.TryCreate(url.Text.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http")) { message.Message = "請輸入有效的 HTTP 或 HTTPS 鏡像網址。"; message.IsOpen = true; return; } list.Items.Add(new Mirror(type.SelectedIndex == 0 ? "ghProxy" : "jsdelivr", uri.AbsoluteUri.TrimEnd('/'))); url.Text = ""; message.IsOpen = false; }; actions.Children.Add(add);
        var first = new Button { Content = "設為優先" }; first.Click += (_, _) => { var index = list.SelectedIndex; if (index > 0) { var value = list.Items[index]; list.Items.RemoveAt(index); list.Items.Insert(0, value); list.SelectedIndex = 0; } }; actions.Children.Add(first);
        var remove = new Button { Content = "移除" }; remove.Click += (_, _) => { if (list.SelectedIndex >= 0) list.Items.RemoveAt(list.SelectedIndex); }; actions.Children.Add(remove); Children.Add(actions);
    }
    public void Load(JsonObject config) { enabled.IsOn = config["extra"]?["githubMirror"]?["enabled"]?.GetValue<bool>() == true; list.Items.Clear(); foreach (var mirror in config["extra"]?["githubMirror"]?["mirrors"]?.AsArray() ?? []) if (mirror?["isDeleted"]?.GetValue<bool>() != true) list.Items.Add(new Mirror(mirror!["type"]!.GetValue<string>(), mirror["url"]!.GetValue<string>())); }
    public void Save(JsonObject config) { if (enabled.IsOn && list.Items.Count == 0) throw new FormatException("請先新增 GitHub 鏡像。"); ConfigJson.Set(config, "extra.githubMirror", new JsonObject { ["enabled"] = enabled.IsOn, ["mirrors"] = new JsonArray(list.Items.Cast<Mirror>().Select(x => (JsonNode?)new JsonObject { ["type"] = x.Type, ["url"] = x.Url, ["isBuiltIn"] = false, ["isDeleted"] = false }).ToArray()) }); }
}
