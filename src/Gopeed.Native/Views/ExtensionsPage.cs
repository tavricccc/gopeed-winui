using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Gopeed_Native.ViewModels;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace Gopeed_Native.Views;

public sealed class ExtensionsPage : Page
{
 private DownloadsViewModel vm = null!;
 private readonly StackPanel list = new() { Spacing = 16 };
 private readonly InfoBar message = new() { IsClosable = true };
 private readonly TextBox url = new() { Header = "擴充功能 Git repository", PlaceholderText = "https://github.com/owner/gopeed-extension", MinWidth = 320 };
 public ExtensionsPage(DownloadsViewModel viewModel)
 {
  vm = viewModel;
  var panel = new StackPanel { Spacing = 20, MaxWidth = 800, HorizontalAlignment = HorizontalAlignment.Left };
  panel.Children.Add(new TextBlock { Text = "擴充功能", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] });
  panel.Children.Add(new TextBlock { Text = "讓 Gopeed 解析更多下載來源。擴充功能會在本機執行程式碼，請選擇你信任的專案。", TextWrapping = TextWrapping.Wrap }); panel.Children.Add(message); panel.Children.Add(url);
  var install = new Button { Content = "安裝擴充功能", Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
  install.Click += async (_, _) => { install.IsEnabled = false; try { await vm.Core.SendAsync(HttpMethod.Post,"extensions",new JsonObject { ["url"] = url.Text.Trim() }); url.Text = ""; await Reload(); } catch (Exception ex) { Error(ex); } finally { install.IsEnabled = true; } };
  panel.Children.Add(install); panel.Children.Add(new HyperlinkButton { Content = "查看官方擴充功能範例", NavigateUri = new Uri("https://github.com/GopeedLab/gopeed-extension-samples") }); panel.Children.Add(list);
  Content = new ScrollViewer { Content = panel, Padding = new Thickness(0,0,16,24) }; Loaded += async (_, _) => await Reload();
 }
 private async Task Reload()
 {
  try
  {
   list.Children.Clear(); var data = (await vm.Core.GetAsync("extensions"))?.AsArray();
   if (data is null || data.Count == 0) { list.Children.Add(new TextBlock { Text = "尚未安裝擴充功能。" }); return; }
   foreach (var node in data)
   {
    var ext = node!.AsObject(); var identity = Uri.EscapeDataString(ext["identity"]!.GetValue<string>()); var route = "extensions/" + identity;
    var section = new StackPanel { Spacing = 10 };
    section.Children.Add(new TextBlock { Text = $"{ext["title"]}  {ext["version"]}", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"] });
    section.Children.Add(new TextBlock { Text = ext["description"]?.GetValue<string>() ?? "", TextWrapping = TextWrapping.Wrap });
    var enabled = new ToggleSwitch { Header = "啟用", IsOn = !ext["disabled"]!.GetValue<bool>() };
    enabled.Toggled += async (_, _) => { try { await vm.Core.SendAsync(HttpMethod.Put,route+"/switch",new JsonObject { ["status"] = enabled.IsOn }); } catch (Exception ex) { Error(ex); } }; section.Children.Add(enabled);
    var commands = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
    var settings = new Button { Content = "設定" }; settings.Click += async (_, _) => await EditSettings(ext,route); commands.Children.Add(settings);
    var update = new Button { Content = "更新" }; update.Click += async (_, _) => { update.IsEnabled = false; try { await vm.Core.SendAsync(HttpMethod.Post,route+"/update"); await Reload(); } catch (Exception ex) { Error(ex); } finally { update.IsEnabled = true; } }; commands.Children.Add(update);
    var remove = new Button { Content = "解除安裝" }; remove.Click += async (_, _) => { var dialog = new ContentDialog { Title = "解除安裝擴充功能？", Content = ext["title"]!.GetValue<string>(), PrimaryButtonText = "解除安裝", CloseButtonText = "取消", XamlRoot = XamlRoot }; if (await Gopeed_Native.Services.NativeDialogs.ShowAsync(dialog, XamlRoot) == ContentDialogResult.Primary) { try { await vm.Core.SendAsync(HttpMethod.Delete,route); await Reload(); } catch (Exception ex) { Error(ex); } } }; commands.Children.Add(remove); section.Children.Add(commands); list.Children.Add(section);
   }
  }
  catch (Exception ex) { Error(ex); }
 }
 private async Task EditSettings(JsonObject ext, string route)
 {
  var panel = new StackPanel { Spacing = 12 }; var readers = new Dictionary<string, Func<JsonNode?>>();
  foreach (var node in ext["settings"]?.AsArray() ?? [])
  {
   var item = node!; var name = item["name"]!.GetValue<string>(); var title = item["title"]?.GetValue<string>() ?? name;
   if (item["options"] is JsonArray options && options.Count > 0)
   {
    var box = new ComboBox { Header = title, HorizontalAlignment = HorizontalAlignment.Stretch }; foreach (var option in options) box.Items.Add(new ComboBoxItem { Content = option!["label"]!.GetValue<string>(), Tag = option["value"]!.DeepClone() });
    box.SelectedIndex = Math.Max(0, options.ToList().FindIndex(o => JsonNode.DeepEquals(o?["value"],item["value"]))); panel.Children.Add(box); readers[name] = () => ((JsonNode)((ComboBoxItem)box.SelectedItem).Tag).DeepClone();
   }
   else if (item["type"]!.GetValue<string>() == "boolean") { var box = new ToggleSwitch { Header = title, IsOn = item["value"]?.GetValue<bool>() ?? false }; panel.Children.Add(box); readers[name] = () => JsonValue.Create(box.IsOn); }
   else if (item["type"]!.GetValue<string>() == "number") { var box = new NumberBox { Header = title, Value = item["value"]?.GetValue<double>() ?? 0 }; panel.Children.Add(box); readers[name] = () => JsonValue.Create(box.Value); }
   else { var box = new TextBox { Header = title, Text = item["value"]?.ToString() ?? "" }; panel.Children.Add(box); readers[name] = () => JsonValue.Create(box.Text); }
   if (item["description"]?.GetValue<string>() is { Length: > 0 } description) panel.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap });
  }
  if (readers.Count == 0) panel.Children.Add(new TextBlock { Text = "此擴充功能沒有可調整的設定。" });
  var dialog = new ContentDialog { Title = ext["title"]!.GetValue<string>(), Content = new ScrollViewer { Content = panel, MaxHeight = 400 }, PrimaryButtonText = "儲存", CloseButtonText = "取消", XamlRoot = XamlRoot };
  if (await Gopeed_Native.Services.NativeDialogs.ShowAsync(dialog, XamlRoot) == ContentDialogResult.Primary) { try { var settings = new JsonObject(); foreach (var pair in readers) settings[pair.Key] = pair.Value(); await vm.Core.SendAsync(HttpMethod.Put,route+"/settings", new JsonObject { ["settings"] = settings }); await Reload(); } catch (Exception ex) { Error(ex); } }
 }
 private void Error(Exception e) { message.Severity = InfoBarSeverity.Error; message.Message = e.Message; message.IsOpen = true; }
}
