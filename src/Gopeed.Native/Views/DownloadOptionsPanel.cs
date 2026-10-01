using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json.Nodes;
using Gopeed_Native.Services;

namespace Gopeed_Native.Views;

internal sealed class DownloadOptionsPanel : StackPanel
{
    private readonly ComboBox method = new() { Header = "HTTP 方法", IsEditable = true, Items = { "GET", "POST", "PUT", "HEAD" }, SelectedIndex = 0 };
    private readonly TextBox body = new() { Header = "請求內容", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 80, MaxHeight = 160 };
    private readonly CheckBox skipCert = new() { Content = "略過 HTTPS 憑證驗證" };
    private readonly TextBox trackers = new() { Header = "BT Tracker（每行一個）", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 80, MaxHeight = 160 };
    private readonly ComboBox proxyMode = new() { Header = "代理伺服器", Items = { "使用預設值", "直接連線", "自訂" }, SelectedIndex = 0 };
    private readonly ComboBox scheme = new() { Header = "代理協定", Items = { "http", "https", "socks5" }, SelectedIndex = 0 };
    private readonly TextBox host = new() { Header = "主機與連接埠" };
    private readonly TextBox user = new() { Header = "使用者名稱" };
    private readonly PasswordBox password = new() { Header = "密碼" };
    private readonly ComboBox autoTorrent = Choice("Torrent 檔案自動下載");
    private readonly ComboBox deleteTorrent = Choice("開始 BT 下載後刪除 Torrent 檔案");
    private readonly ComboBox extract = Choice("自動解壓縮");
    private readonly PasswordBox archivePassword = new() { Header = "壓縮檔密碼" };
    private readonly CheckBox deleteArchive = new() { Content = "解壓縮成功後刪除壓縮檔" };
    public event Action? RequestChanged;
    public DownloadOptionsPanel()
    {
        Spacing = 12;
        foreach (var control in new UIElement[] { method, body, skipCert, trackers, proxyMode, scheme, host, user, password, autoTorrent, deleteTorrent, extract, archivePassword, deleteArchive }) Children.Add(control);
        method.SelectionChanged += (_, _) => RequestChanged?.Invoke(); method.TextSubmitted += (_, _) => RequestChanged?.Invoke();
        body.TextChanged += (_, _) => RequestChanged?.Invoke(); trackers.TextChanged += (_, _) => RequestChanged?.Invoke();
        proxyMode.SelectionChanged += (_, _) => { var custom = proxyMode.SelectedIndex == 2; foreach (var field in new Control[] { scheme, host, user, password }) field.IsEnabled = custom; RequestChanged?.Invoke(); };
        skipCert.Checked += (_, _) => RequestChanged?.Invoke(); skipCert.Unchecked += (_, _) => RequestChanged?.Invoke();
    }
    private static ComboBox Choice(string title) => new() { Header = title, Items = { "使用預設值", "啟用", "停用" }, SelectedIndex = 0 };
    public void Load(JsonObject? initial, JsonNode config)
    {
        var req = initial?["req"]; method.Text = req?["extra"]?["method"]?.GetValue<string>() ?? "GET"; body.Text = req?["extra"]?["body"]?.GetValue<string>() ?? "";
        skipCert.IsChecked = req?["skipVerifyCert"]?.GetValue<bool>() == true;
        trackers.Text = string.Join("\n", req?["extra"]?["trackers"]?.AsArray().Select(x => x!.GetValue<string>()) ?? []);
        proxyMode.SelectedIndex = req?["proxy"]?["mode"]?.GetValue<string>() switch { "none" => 1, "custom" => 2, _ => 0 };
        scheme.SelectedIndex = req?["proxy"]?["scheme"]?.GetValue<string>() switch { "https" => 1, "socks5" => 2, _ => 0 };
        host.Text = req?["proxy"]?["host"]?.GetValue<string>() ?? ""; user.Text = req?["proxy"]?["usr"]?.GetValue<string>() ?? ""; password.Password = req?["proxy"]?["pwd"]?.GetValue<string>() ?? "";
        foreach (var (control, key) in new[] { (autoTorrent, "autoTorrent"), (deleteTorrent, "deleteTorrentAfterDownload"), (extract, "autoExtract") }) control.SelectedIndex = initial?["opts"]?["extra"]?[key] is JsonValue value ? value.GetValue<bool>() ? 1 : 2 : 0;
        archivePassword.Password = initial?["opts"]?["extra"]?["archivePassword"]?.GetValue<string>() ?? "";
        deleteArchive.IsChecked = initial?["opts"]?["extra"]?["deleteAfterExtract"]?.GetValue<bool>() ?? config["archive"]?["deleteAfterExtract"]?.GetValue<bool>() ?? false;
    }
    public void Apply(JsonObject request)
    {
        var req = request["req"]!.AsObject(); req["extra"] ??= new JsonObject(); var extra = req["extra"]!.AsObject();
        extra["method"] = method.Text.Trim().Length == 0 ? "GET" : method.Text.Trim().ToUpperInvariant(); extra["body"] = body.Text;
        if (trackers.Text.Length > 0) extra["trackers"] = ConfigJson.Array(ConfigJson.Lines(trackers.Text));
        req["skipVerifyCert"] = skipCert.IsChecked == true;
        if (proxyMode.SelectedIndex == 2 && host.Text.Trim().Length == 0) throw new FormatException("請輸入代理伺服器的主機與連接埠。");
        req["proxy"] = new JsonObject { ["mode"] = proxyMode.SelectedIndex switch { 1 => "none", 2 => "custom", _ => "follow" }, ["scheme"] = scheme.SelectedItem?.ToString() ?? "http", ["host"] = host.Text.Trim(), ["usr"] = user.Text, ["pwd"] = password.Password };
        var opts = request["opts"]!.AsObject(); opts["extra"] ??= new JsonObject();
        foreach (var (control, key) in new[] { (autoTorrent, "autoTorrent"), (deleteTorrent, "deleteTorrentAfterDownload"), (extract, "autoExtract") }) opts["extra"]![key] = control.SelectedIndex == 0 ? null : JsonValue.Create(control.SelectedIndex == 1);
        opts["extra"]!["archivePassword"] = archivePassword.Password; opts["extra"]!["deleteAfterExtract"] = deleteArchive.IsChecked == true;
    }
}
