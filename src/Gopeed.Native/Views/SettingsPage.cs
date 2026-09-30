using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Gopeed_Native.ViewModels;
using System.Net.Http;
using System.Text.Json.Nodes;
using Windows.Storage.Pickers;

namespace Gopeed_Native.Views;

public sealed class SettingsPage : Page
{
 private DownloadsViewModel vm = null!;
 private JsonObject config = null!;
 private readonly TextBox folder = new() { Header = "預設下載位置" };
 private readonly NumberBox running = new() { Header = "同時下載數", Minimum = 1, Maximum = 32, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
 private readonly NumberBox connections = new() { Header = "HTTP 連線數", Minimum = 1, Maximum = 64, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
 private readonly ComboBox theme = new() { Header = "外觀", Items = { "跟隨 Windows", "淺色", "深色" }, SelectedIndex = 0 };
 private readonly TextBox proxy = new() { Header = "自訂代理伺服器", PlaceholderText = "http://127.0.0.1:7890 或 socks5://127.0.0.1:1080" };
 private readonly ComboBox proxyMode = new() { Header = "代理模式", Items = { "跟隨系統", "直接連線", "自訂" }, SelectedIndex = 0 };
 private readonly InfoBar message = new() { IsClosable = true };
 private readonly TextBox endpoint = new() { Header = "瀏覽器擴充套件伺服器位址", IsReadOnly = true };
 private readonly PasswordBox token = new() { Header = "API Token", PasswordRevealMode = PasswordRevealMode.Peek };
 protected override void OnNavigatedTo(NavigationEventArgs e) { vm = (DownloadsViewModel)e.Parameter; Build(); Loaded += Load; }
 private void Build()
 {
  var content = new StackPanel { Spacing = 18, MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
  content.Children.Add(new TextBlock { Text = "設定", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] }); content.Children.Add(message);
  var browse = new Button { Content = "選擇資料夾…" }; browse.Click += async (_, _) => { var picker = new FolderPicker(); picker.FileTypeFilter.Add("*"); WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle); var picked = await picker.PickSingleFolderAsync(); if (picked is not null) folder.Text = picked.Path; };
  content.Children.Add(folder); content.Children.Add(browse); content.Children.Add(running); content.Children.Add(connections); content.Children.Add(proxyMode); content.Children.Add(proxy); content.Children.Add(theme);
  var save = new Button { Content = "儲存設定", Style = (Style)Application.Current.Resources["AccentButtonStyle"] }; save.Click += Save; content.Children.Add(save);
  content.Children.Add(new TextBlock { Text = "瀏覽器整合", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"] });
  content.Children.Add(new TextBlock { Text = "安裝 Gopeed 官方瀏覽器擴充套件，新增下方伺服器與 Token。核心重新啟動後需更新位址與 Token。", TextWrapping = TextWrapping.Wrap });
  var browser = new HyperlinkButton { Content = "取得 Gopeed 瀏覽器擴充套件", NavigateUri = new Uri("https://github.com/GopeedLab/browser-extension") }; content.Children.Add(browser); content.Children.Add(endpoint); content.Children.Add(token);
  var copy = new Button { Content = "複製 API Token" }; copy.Click += (_, _) => { var data = new Windows.ApplicationModel.DataTransfer.DataPackage(); data.SetText(vm.Core.Token); Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(data); message.Severity = InfoBarSeverity.Success; message.Message = "Token 已複製。"; message.IsOpen = true; }; content.Children.Add(copy);
  content.Children.Add(new TextBlock { Text = "背景下載", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"] });
  content.Children.Add(new TextBlock { Text = "關閉視窗會釋放前端記憶體，下載核心仍繼續執行。再次開啟即可接回佇列。", TextWrapping = TextWrapping.Wrap });
  var stop = new Button { Content = "停止下載核心並結束" }; stop.Click += async (_, _) => { var dialog = new ContentDialog { Title = "停止所有背景下載？", Content = "下載進度會保存，下一次開啟可繼續。", PrimaryButtonText = "停止並結束", CloseButtonText = "取消", XamlRoot = XamlRoot }; if (await dialog.ShowAsync() == ContentDialogResult.Primary) { try { await vm.Core.StopAsync(); App.Window.Close(); } catch (Exception ex) { Error(ex); } } }; content.Children.Add(stop);
  content.Children.Add(new TextBlock { Text = "Gopeed Native 0.1.0 · Gopeed 核心 1.9.3 · GPL-3.0\n獨立 WinUI 3 前端，非 Gopeed 官方版本。", TextWrapping = TextWrapping.Wrap });
  Content = new ScrollViewer { Content = content, Padding = new Thickness(0,0,16,24), HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
 }
 private async void Load(object s, RoutedEventArgs e)
 {
  Loaded -= Load;
  try
  {
   config = (await vm.Core.GetAsync("config"))!.AsObject(); folder.Text = config["downloadDir"]?.GetValue<string>() ?? ""; running.Value = config["maxRunning"]!.GetValue<int>();
   connections.Value = config["protocolConfig"]?["http"]?["connections"]?.GetValue<int>() ?? 8;
   endpoint.Text = vm.Core.ApiAddress; token.Password = vm.Core.Token;
   var p = config["proxy"]; proxyMode.SelectedIndex = p?["enable"]?.GetValue<bool>() == true ? p["system"]?.GetValue<bool>() == true ? 0 : 2 : 1;
   if (proxyMode.SelectedIndex == 2) proxy.Text = $"{p!["scheme"]}://{p["host"]}";
   theme.SelectedIndex = ((FrameworkElement)App.Window.Content).RequestedTheme switch { ElementTheme.Light => 1, ElementTheme.Dark => 2, _ => 0 };
  }
  catch (Exception ex) { Error(ex); }
 }
 private async void Save(object s, RoutedEventArgs e)
 {
  try
  {
   if (config is null) throw new InvalidOperationException("下載核心尚未連接。");
   if (!Path.IsPathFullyQualified(folder.Text)) throw new FormatException("請選擇完整的下載路徑。");
   Directory.CreateDirectory(folder.Text); config["downloadDir"] = folder.Text; config["maxRunning"] = (int)running.Value;
   var protocols = config["protocolConfig"]!.AsObject(); protocols["http"] ??= new JsonObject(); protocols["http"]!["connections"] = (int)connections.Value;
   var p = config["proxy"]!.AsObject(); p["enable"] = proxyMode.SelectedIndex != 1; p["system"] = proxyMode.SelectedIndex == 0;
   if (proxyMode.SelectedIndex == 2) { var uri = new Uri(proxy.Text); if (uri.Scheme is not ("http" or "https" or "socks5")) throw new FormatException("代理協定需為 HTTP、HTTPS 或 SOCKS5。"); p["scheme"] = uri.Scheme; p["host"] = uri.Authority; }
   await vm.Core.SendAsync(HttpMethod.Put, "config", config);
   ((FrameworkElement)App.Window.Content).RequestedTheme = theme.SelectedIndex switch { 1 => ElementTheme.Light, 2 => ElementTheme.Dark, _ => ElementTheme.Default };
   await File.WriteAllTextAsync(Path.Combine(Services.CoreClient.DataDirectory,"theme.txt"), theme.SelectedIndex.ToString());
   message.Severity = InfoBarSeverity.Success; message.Message = "設定已儲存。"; message.IsOpen = true;
  }
  catch (Exception ex) { Error(ex); }
 }
 private void Error(Exception ex) { message.Severity = InfoBarSeverity.Error; message.Message = ex.Message; message.IsOpen = true; }
}
