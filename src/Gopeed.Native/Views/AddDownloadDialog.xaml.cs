using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Services;
using Gopeed_Native.Models;
using System.Net.Http;
using System.Text.Json.Nodes;
using Windows.Storage.Pickers;

namespace Gopeed_Native.Views;

public sealed partial class AddDownloadDialog : ContentDialog
{
 private readonly CoreClient core;
 private string? resolved;
 public AddDownloadDialog(CoreClient core)
 {
  this.core = core; InitializeComponent();
  Loaded += async (_, _) => { try { var config = await core.GetAsync("config"); Destination.Text = config?["downloadDir"]?.GetValue<string>() ?? ""; if (Destination.Text.Length == 0) Destination.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"); } catch (Exception e) { ShowError(e); } };
 }
 private void InputChanged(object s, TextChangedEventArgs e) { resolved = null; PrimaryButtonText = "檢查連結"; if (Files is not null) Files.Visibility = Visibility.Collapsed; }
 private async void PickFolder(object s, RoutedEventArgs e)
 {
  var picker = new FolderPicker(); picker.FileTypeFilter.Add("*"); WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);
  var folder = await picker.PickSingleFolderAsync(); if (folder is not null) Destination.Text = folder.Path;
 }
 private async void PickTorrent(object s, RoutedEventArgs e)
 {
  var picker = new FileOpenPicker(); picker.FileTypeFilter.Add(".torrent"); WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);
  var file = await picker.PickSingleFileAsync(); if (file is not null) Links.Text = file.Path;
 }
 private JsonObject BuildRequest(string url)
 {
  var headers = new JsonObject();
  foreach (var line in Headers.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
  {
   var separator = line.IndexOf(':'); if (separator < 1) throw new FormatException("HTTP 標頭請使用「名稱: 值」格式。");
   headers[line[..separator].Trim()] = line[(separator + 1)..].Trim();
  }
  return new JsonObject { ["req"] = new JsonObject { ["url"] = url, ["extra"] = new JsonObject { ["header"] = headers } }, ["opts"] = new JsonObject { ["path"] = Destination.Text.Trim(), ["name"] = FileName.Text.Trim(), ["extra"] = new JsonObject { ["connections"] = (int)Connections.Value } } };
 }
 private async void Submit(ContentDialog sender, ContentDialogButtonClickEventArgs args)
 {
  args.Cancel = true; var deferral = args.GetDeferral(); bool complete = false;
  IsPrimaryButtonEnabled = false; Busy.Visibility = Visibility.Visible; Busy.IsActive = true; Message.IsOpen = false;
  try
  {
   var links = Links.Text.Split(['\r','\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
   if (links.Length == 0) throw new FormatException("請輸入下載連結。");
   if (!Path.IsPathFullyQualified(Destination.Text.Trim())) throw new FormatException("請選擇完整的儲存路徑。");
   if (FileName.Text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new FormatException("檔名含有無法使用的字元。");
   Directory.CreateDirectory(Destination.Text.Trim());
   if (links.Length > 1)
   {
    if (FileName.Text.Length > 0) throw new FormatException("批次下載請留空檔名，避免檔案名稱重複。");
    foreach (var link in links) await core.SendAsync(HttpMethod.Post, "tasks", BuildRequest(link)); complete = true;
   }
   else if (resolved is null)
   {
    var result = (await core.SendAsync(HttpMethod.Post, "resolve", BuildRequest(links[0])))!;
    resolved = result["id"]!.GetValue<string>(); var resource = result["res"]!;
    var displayName = resource["name"]?.GetValue<string>();
    if (string.IsNullOrEmpty(displayName)) displayName = resource["files"]!.AsArray()[0]!["name"]!.GetValue<string>();
    var size = resource["size"]!.GetValue<long>();
    Preview.Text = $"{displayName}\n{(size > 0 ? DownloadItem.FormatBytes(size) : "大小由伺服器於下載時提供")}";
    Files.Items.Clear(); foreach (var file in resource["files"]!.AsArray()) Files.Items.Add(file!["name"]!.GetValue<string>());
    Files.SelectAll(); Files.Visibility = Files.Items.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
    PrimaryButtonText = "開始下載";
   }
   else
   {
    // Update resolution options if destination, file name or file selection changed after probing.
    var request = BuildRequest(links[0]);
    request["opts"]!["selectFiles"] = new JsonArray(Files.SelectedItems.Cast<string>().Select(name => JsonValue.Create(Files.Items.IndexOf(name)) as JsonNode).ToArray());
    if (Files.Items.Count > 1 && Files.SelectedItems.Count == 0) throw new FormatException("請至少選擇一個檔案。");
    await core.SendAsync(HttpMethod.Post, "tasks", request); complete = true;
   }
  }
  catch (Exception e) { ShowError(e); }
  finally { Busy.IsActive = false; Busy.Visibility = Visibility.Collapsed; IsPrimaryButtonEnabled = true; deferral.Complete(); }
  if (complete) Hide();
 }
 private void ShowError(Exception e) { Message.Message = e.Message; Message.IsOpen = true; }
}
