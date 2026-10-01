using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Services;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Diagnostics;

namespace Gopeed_Native.Views;

public sealed partial class SettingsPage
{
    private void BuildProtocols()
    {
        var http = Section("HTTP");
        fields.Number(http, "每個下載的連線數", "protocolConfig.http.connections", 8, 1, 256);
        fields.Text(http, "User-Agent", "protocolConfig.http.userAgent");
        fields.Toggle(http, "使用伺服器提供的檔案時間", "protocolConfig.http.useServerCtime");
        var bt = Section("BT");
        fields.Number(bt, "監聽連接埠（0 為自動）", "protocolConfig.bt.listenPort", 0, 0, 65535);
        customTrackers = fields.Text(bt, "自訂 Tracker（每行一個）", "extra.bt.customTrackers", true);
        var subscriptions = fields.Text(bt, "Tracker 訂閱網址（每行一個）", "extra.bt.trackerSubscribeUrls", true);
        fields.Toggle(bt, "每天更新 Tracker 訂閱", "extra.bt.autoUpdateTrackers", true);
        var update = new Button { Content = "更新 Tracker 訂閱" };
        update.Click += async (_, _) =>
        {
            update.IsEnabled = false;
            try
            {
                var trackers = new List<string>();
                var current = (await vm.Core.GetAsync("config"))!.AsObject(); mirrors.Save(current);
                foreach (var url in ConfigJson.Lines(subscriptions.Text)) { if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) throw new FormatException("請輸入有效的 Tracker 訂閱網址。"); trackers.AddRange(ConfigJson.Lines(await vm.Core.FetchTextAsync(GitHubMirror.Apply(url, current)))); }
                var config = (await vm.Core.GetAsync("config"))!.AsObject();
                ConfigJson.Set(config, "extra.bt.subscribeTrackers", ConfigJson.Array(trackers.Distinct())); ConfigJson.Set(config, "extra.bt.trackerSubscribeUrls", ConfigJson.Array(ConfigJson.Lines(subscriptions.Text)));
                ConfigJson.Set(config, "extra.bt.lastTrackerUpdateTime", JsonValue.Create(DateTimeOffset.UtcNow.ToString("O")));
                ConfigJson.Set(config, "protocolConfig.bt.trackers", ConfigJson.Array(trackers.Concat(ConfigJson.Lines(customTrackers.Text)).Distinct()));
                await vm.Core.SendAsync(HttpMethod.Put, "config", config); Success($"已更新 {trackers.Distinct().Count()} 個 Tracker。");
            }
            catch (Exception error) { Report(error); } finally { update.IsEnabled = true; }
        }; bt.Children.Add(update);
        fields.Toggle(bt, "下載完成後持續做種", "protocolConfig.bt.seedKeep");
        fields.Number(bt, "停止做種的分享比例", "protocolConfig.bt.seedRatio", 0, 0, 10000, fractional: true);
        fields.Number(bt, "停止做種的時間（分鐘）", "protocolConfig.bt.seedTime", 0, 0, 100000000, 60);
        var defaults = new Button { Content = "設定預設 Torrent 與磁力連結程式" }; defaults.Click += (_, _) => WindowsIntegration.OpenDefaultApps(); bt.Children.Add(defaults);
        var ed2k = Section("eD2k");
        fields.Number(ed2k, "TCP 連接埠（0 為自動）", "protocolConfig.ed2k.listenPort", 0, 0, 65535);
        fields.Number(ed2k, "UDP 連接埠（0 為自動）", "protocolConfig.ed2k.udpPort", 0, 0, 65535);
        fields.Text(ed2k, "伺服器（每行一個 host:port）", "protocolConfig.ed2k.serverAddr", true, array: false);
        fields.Text(ed2k, "server.met 網址（每行一個）", "protocolConfig.ed2k.serverMet", true, array: false);
        fields.Text(ed2k, "nodes.dat 網址（每行一個）", "protocolConfig.ed2k.nodesDat", true, array: false);
    }
    private void BuildNetwork()
    {
        Section("GitHub 鏡像").Children.Add(mirrors);
        var proxy = Section("代理伺服器"); proxy.Children.Add(proxyMode);
        fields.Text(proxy, "協定", "proxy.scheme", initial: "http"); fields.Text(proxy, "主機與連接埠", "proxy.host");
        fields.Text(proxy, "使用者名稱", "proxy.usr"); fields.Password(proxy, "密碼", "proxy.pwd");
        var browser = Section("瀏覽器接管");
        browser.Children.Add(new HyperlinkButton { Content = "取得瀏覽器擴充套件", NavigateUri = new Uri("https://github.com/GopeedLab/browser-extension") });
        var enable = new Button { Content = "啟用瀏覽器下載接管" }; enable.Click += (_, _) => { try { WindowsIntegration.InstallBrowserHost(); Success("已啟用。請在擴充套件關閉遠端下載，改用本機接管。"); } catch (Exception error) { Report(error); } }; browser.Children.Add(enable);
        var remote = new StackPanel { Spacing = 12 };
        remote.Children.Add(new TextBlock { Text = "通訊協定選 HTTP，填入以下位址與 Token。", TextWrapping = TextWrapping.Wrap });
        remote.Children.Add(new TextBox { Header = "伺服器位址", IsReadOnly = true, Text = new Uri(vm.Core.ApiAddress).Authority });
        remote.Children.Add(new PasswordBox { Header = "API Token", Password = vm.Core.Token, PasswordRevealMode = PasswordRevealMode.Peek });
        var copy = new Button { Content = "複製 API Token" }; copy.Click += (_, _) => { FileActions.Copy(vm.Core.Token); Success("Token 已複製。"); }; remote.Children.Add(copy); remote.Children.Add(apiPort);
        browser.Children.Add(new Expander { Header = "遠端下載連線", Content = remote, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch });
    }
    private void BuildAutomation()
    {
        var panel = Section("完成後的動作");
        fields.Toggle(panel, "傳送 Webhook 通知", "webhook.enable"); var urls = fields.Text(panel, "Webhook 網址（每行一個）", "webhook.urls", true);
        var test = new Button { Content = "測試 Webhook" }; test.Click += async (_, _) => { test.IsEnabled = false; try { var targets = ConfigJson.Lines(urls.Text); if (targets.Length == 0) throw new FormatException("請先輸入 Webhook 網址。"); foreach (var url in targets) { if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) throw new FormatException("請輸入有效的 Webhook 網址。"); await vm.Core.SendAsync(HttpMethod.Post, "webhook/test", new JsonObject { ["url"] = url }); } Success("測試通知已傳送。"); } catch (Exception error) { Report(error); } finally { test.IsEnabled = true; } }; panel.Children.Add(test);
        fields.Toggle(panel, "完成或失敗後執行程式", "script.enable"); fields.Text(panel, "程式或指令檔路徑（每行一個）", "script.paths", true);
    }
    private void BuildAbout()
    {
        var panel = Section("關於");
        panel.Children.Add(new TextBlock { Text = $"Gopeed Native {typeof(App).Assembly.GetName().Version?.ToString(3)}", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"] });
        panel.Children.Add(checkUpdates);
        var update = new Button { Content = "檢查更新" }; update.Click += async (_, _) => { update.IsEnabled = false; try { var release = await UpdateService.CheckAsync(vm.Core); if (release is null) Success("已是最新版本。"); else await UpdateService.PromptAsync(vm.Core, release, XamlRoot); } catch (Exception error) { Report(error); } finally { update.IsEnabled = true; } }; panel.Children.Add(update);
        panel.Children.Add(new HyperlinkButton { Content = "專案首頁", NavigateUri = new Uri("https://github.com/tavricccc/gopeed-winui") });
        panel.Children.Add(new HyperlinkButton { Content = "授權與致謝", NavigateUri = new Uri("https://github.com/tavricccc/gopeed-winui/blob/winui-native/LICENSE") });
        var logs = new Button { Content = "開啟記錄資料夾" }; logs.Click += (_, _) => FileActions.Open(Path.Combine(CoreClient.DataDirectory, "logs")); panel.Children.Add(logs);
        var stop = new Button { Content = "結束程式並停止下載" }; stop.Click += async (_, _) => { var dialog = new ContentDialog { Title = "停止所有下載並結束？", Content = "下載進度會保留，下次開啟可繼續。", PrimaryButtonText = "停止並結束", CloseButtonText = "取消" }; if (await NativeDialogs.ShowAsync(dialog, XamlRoot) == ContentDialogResult.Primary) { try { await vm.Core.StopAsync(); Application.Current.Exit(); } catch (Exception error) { Report(error); } } }; panel.Children.Add(stop);
    }
}
