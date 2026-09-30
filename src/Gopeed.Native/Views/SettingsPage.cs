using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Gopeed_Native.ViewModels;
using System.Net.Http;
using System.Text.Json.Nodes;
using Gopeed_Native.Services;
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
 private readonly CheckBox rememberFolder = new() { Content = "記住上次使用的下載位置" };
 private readonly CheckBox closeProgress = new() { Content = "開啟檔案後關閉下載進度視窗" };
 public SettingsPage(DownloadsViewModel viewModel) { vm = viewModel; Build(); Loaded += Load; }
 private void Build()
 {
  var content = new StackPanel { Spacing = 18, MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
  content.Children.Add(new TextBlock { Text = "設定", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] }); content.Children.Add(message);
  var browse = new Button { Content = "選擇資料夾…" }; browse.Click += async (_, _) => { var picker = new FolderPicker(); picker.FileTypeFilter.Add("*"); WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle); var picked = await picker.PickSingleFolderAsync(); if (picked is not null) folder.Text = picked.Path; };
  content.Children.Add(folder); content.Children.Add(browse); content.Children.Add(rememberFolder); content.Children.Add(closeProgress); content.Children.Add(running); content.Children.Add(connections); content.Children.Add(proxyMode); content.Children.Add(proxy); content.Children.Add(theme);
  var save = new Button { Content = "儲存設定", Style = (Style)Application.Current.Resources["AccentButtonStyle"], HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0,12,0,0) }; save.Click += Save;
  content.Children.Add(new TextBlock { Text = "瀏覽器整合", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"] });
  content.Children.Add(new TextBlock { Text = "官方擴充套件 → 遠端下載：通訊協定選 HTTP，位址填下方的主機與連接埠，再貼上 Token。啟用遠端下載後，會先開原生確認視窗。", TextWrapping = TextWrapping.Wrap });
  var browser = new HyperlinkButton { Content = "取得 Gopeed 瀏覽器擴充套件", NavigateUri = new Uri("https://github.com/GopeedLab/browser-extension") }; content.Children.Add(browser); content.Children.Add(endpoint); content.Children.Add(token);
  var copy = new Button { Content = "複製 API Token" }; copy.Click += (_, _) => { var data = new Windows.ApplicationModel.DataTransfer.DataPackage(); data.SetText(vm.Core.Token); Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(data); message.Severity = InfoBarSeverity.Success; message.Message = "Token 已複製。"; message.IsOpen = true; }; content.Children.Add(copy);
  content.Children.Add(new TextBlock { Text = "背景下載", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"] });
  content.Children.Add(new TextBlock { Text = "關閉視窗會釋放前端記憶體，下載核心仍繼續執行。再次開啟即可接回佇列。", TextWrapping = TextWrapping.Wrap });
  var stop = new Button { Content = "停止下載核心並結束" }; stop.Click += async (_, _) => { var dialog = new ContentDialog { Title = "停止所有背景下載？", Content = "下載進度會保存，下一次開啟可繼續。", PrimaryButtonText = "停止並結束", CloseButtonText = "取消", XamlRoot = XamlRoot }; if (await Gopeed_Native.Services.NativeDialogs.ShowAsync(dialog, XamlRoot) == ContentDialogResult.Primary) { try { await vm.Core.StopAsync(); App.Window.Close(); } catch (Exception ex) { Error(ex); } } }; content.Children.Add(stop);
  content.Children.Add(new TextBlock { Text = $"Gopeed Native {typeof(App).Assembly.GetName().Version?.ToString(3)} · Gopeed 核心 1.9.3 · GPL-3.0\n獨立 WinUI 3 前端，非 Gopeed 官方版本。", TextWrapping = TextWrapping.Wrap });
  var surface = new Grid(); surface.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1,GridUnitType.Star) }); surface.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
  surface.Children.Add(new ScrollViewer { Content = content, Padding = new Thickness(0,0,16,24), HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
  Grid.SetRow(save,1); surface.Children.Add(save); Content = surface;
 }
 private async void Load(object s, RoutedEventArgs e)
 {
  Loaded -= Load;
  try
  {
   config = (await vm.Core.GetAsync("config"))!.AsObject(); folder.Text = config["downloadDir"]?.GetValue<string>() ?? ""; running.Value = config["maxRunning"]!.GetValue<int>();
   connections.Value = config["protocolConfig"]?["http"]?["connections"]?.GetValue<int>() ?? 8;
   endpoint.Text = new Uri(vm.Core.ApiAddress).Authority; token.Password = vm.Core.Token;
   var prefs = UiPreferences.Load(); rememberFolder.IsChecked = prefs.RememberDownloadDirectory; closeProgress.IsChecked = prefs.CloseProgressAfterOpen;
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
   if (double.IsNaN(running.Value) || double.IsNaN(connections.Value)) throw new FormatException("請輸入同時下載數與連線數。");
   if (!Path.IsPathFullyQualified(folder.Text)) throw new FormatException("請選擇完整的下載路徑。");
   Directory.CreateDirectory(folder.Text); config["downloadDir"] = folder.Text; config["maxRunning"] = (int)running.Value;
   var protocols = config["protocolConfig"]!.AsObject(); protocols["http"] ??= new JsonObject(); protocols["http"]!["connections"] = (int)connections.Value;
   var p = config["proxy"]!.AsObject(); p["enable"] = proxyMode.SelectedIndex != 1; p["system"] = proxyMode.SelectedIndex == 0;
   if (proxyMode.SelectedIndex == 2) { var uri = new Uri(proxy.Text); if (uri.Scheme is not ("http" or "https" or "socks5")) throw new FormatException("代理協定需為 HTTP、HTTPS 或 SOCKS5。"); p["scheme"] = uri.Scheme; p["host"] = uri.Authority; }
   await vm.Core.SendAsync(HttpMethod.Put, "config", config);
   var prefs = UiPreferences.Load(); prefs.RememberDownloadDirectory = rememberFolder.IsChecked == true; prefs.CloseProgressAfterOpen = closeProgress.IsChecked == true; prefs.Save();
   ((FrameworkElement)App.Window.Content).RequestedTheme = theme.SelectedIndex switch { 1 => ElementTheme.Light, 2 => ElementTheme.Dark, _ => ElementTheme.Default };
   await File.WriteAllTextAsync(Path.Combine(Services.CoreClient.DataDirectory,"theme.txt"), theme.SelectedIndex.ToString());
   message.Severity = InfoBarSeverity.Success; message.Message = "設定已儲存。"; message.IsOpen = true;
  }
  catch (Exception ex) { Error(ex); }
 }
 private void Error(Exception ex) { message.Severity = InfoBarSeverity.Error; message.Message = ex.Message; message.IsOpen = true; }
}
