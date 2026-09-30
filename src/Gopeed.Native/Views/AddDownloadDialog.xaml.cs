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
 private readonly JsonObject? initial;
 private readonly nint owner;
 private string lastInput = "";
 private readonly bool compact;
 public string? CreatedTaskId { get; private set; }
 public AddDownloadDialog(CoreClient core, JsonObject? initial = null, nint? owner = null, bool compact = false)
 {
  this.core = core; this.initial = initial; this.owner = owner ?? App.WindowHandle; this.compact = compact; InitializeComponent();
  if (compact) { Title = "確認下載"; Links.Header = "來源網址"; Links.AcceptsReturn = false; Links.TextWrapping = TextWrapping.NoWrap; Links.MinHeight = 0; Links.MaxHeight = double.PositiveInfinity; TorrentPickerButton.Visibility = Visibility.Collapsed; FileName.Header = "檔名"; }
  Loaded += async (_, _) => { IsPrimaryButtonEnabled = false; try { var config = await core.GetAsync("config"); Destination.Text = config?["downloadDir"]?.GetValue<string>() ?? ""; if (Destination.Text.Length == 0) Destination.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"); ApplyInitial(); if (initial is not null && Links.Text.Length > 0) await InspectAsync(Links.Text); } catch (Exception e) { ShowError(e); } finally { IsPrimaryButtonEnabled = true; } };
 }
 private void ApplyInitial()
 {
  if (initial is null) return;
  Links.Text = initial["req"]?["url"]?.GetValue<string>() ?? "";
  lastInput = Links.Text;
  if (initial["opts"]?["path"]?.GetValue<string>() is { Length: > 0 } path) Destination.Text = path;
  FileName.Text = initial["opts"]?["name"]?.GetValue<string>() ?? "";
  if (initial["opts"]?["extra"]?["connections"] is JsonValue connections) Connections.Value = connections.GetValue<int>();
  if (initial["req"]?["extra"]?["header"] is JsonObject headers) Headers.Text = string.Join("\n", headers.Select(pair => $"{pair.Key}: {pair.Value}"));
 }
 private void InputChanged(object s, TextChangedEventArgs e) { if (lastInput == Links.Text) return; lastInput = Links.Text; resolved = null; var count = Links.Text.Split(['\r','\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length; PrimaryButtonText = count > 1 ? $"開始 {count} 個下載" : "檢查連結"; if (Files is not null) Files.Visibility = Visibility.Collapsed; }
 private async void PickFolder(object s, RoutedEventArgs e)
 {
  var picker = new FolderPicker(); picker.FileTypeFilter.Add("*"); WinRT.Interop.InitializeWithWindow.Initialize(picker, owner);
  var folder = await picker.PickSingleFolderAsync(); if (folder is not null) Destination.Text = folder.Path;
 }
 private async void PickTorrent(object s, RoutedEventArgs e)
 {
  var picker = new FileOpenPicker(); picker.FileTypeFilter.Add(".torrent"); WinRT.Interop.InitializeWithWindow.Initialize(picker, owner);
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
  var request = initial?.DeepClone().AsObject() ?? new JsonObject();
  var req = request["req"] as JsonObject ?? new JsonObject();
  var requestExtra = req["extra"] as JsonObject ?? new JsonObject();
  var opts = request["opts"] as JsonObject ?? new JsonObject();
  var optionExtra = opts["extra"] as JsonObject ?? new JsonObject();
  requestExtra["header"] = headers; req["url"] = url;
  if (req["extra"] is null) req["extra"] = requestExtra;
  optionExtra["connections"] = (int)Connections.Value; opts["path"] = Destination.Text.Trim(); opts["name"] = FileName.Text.Trim();
  if (opts["extra"] is null) opts["extra"] = optionExtra;
  if (request["req"] is null) request["req"] = req;
  if (request["opts"] is null) request["opts"] = opts;
  return request;
 }
 private async void Submit(ContentDialog sender, ContentDialogButtonClickEventArgs args)
 {
  args.Cancel = true; var deferral = args.GetDeferral(); bool complete = false;
  IsPrimaryButtonEnabled = false; Busy.Visibility = Visibility.Visible; Busy.IsActive = true; Message.IsOpen = false;
  try
  {
   var links = Links.Text.Split(['\r','\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
   if (links.Length == 0) throw new FormatException("請輸入下載連結。");
   if (double.IsNaN(Connections.Value)) throw new FormatException("請輸入連線數。");
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
    await InspectAsync(links[0]);
   }
   else
   {
    // Update resolution options if destination, file name or file selection changed after probing.
    var request = BuildRequest(links[0]);
    request["opts"]!["selectFiles"] = new JsonArray(Files.SelectedItems.Cast<ResolvedFile>().Select(file => JsonValue.Create(file.Index) as JsonNode).ToArray());
    if (Files.Items.Count > 1 && Files.SelectedItems.Count == 0) throw new FormatException("請至少選擇一個檔案。");
    CreatedTaskId = (await core.SendAsync(HttpMethod.Post, "tasks", request))!.GetValue<string>(); complete = true;
   }
  }
  catch (Exception e) { ShowError(e); }
  finally { Busy.IsActive = false; Busy.Visibility = Visibility.Collapsed; IsPrimaryButtonEnabled = true; deferral.Complete(); }
  if (complete) Hide();
 }
 private async Task InspectAsync(string url)
 {
  Busy.Visibility = Visibility.Visible; Busy.IsActive = true;
  try
  {
   var result = (await core.SendAsync(HttpMethod.Post, "resolve", BuildRequest(url)))!;
   resolved = result["id"]!.GetValue<string>(); var resource = result["res"]!;
   var displayName = resource["name"]?.GetValue<string>();
   if (string.IsNullOrEmpty(displayName)) displayName = resource["files"]!.AsArray()[0]!["name"]!.GetValue<string>();
   var size = resource["size"]!.GetValue<long>();
   Preview.Text = compact ? $"大小：{(size > 0 ? DownloadItem.FormatBytes(size) : "由伺服器於下載時提供")}" : $"{displayName}\n{(size > 0 ? DownloadItem.FormatBytes(size) : "大小由伺服器於下載時提供")}";
   Files.Items.Clear(); var index = 0;
   foreach (var file in resource["files"]!.AsArray()) Files.Items.Add(new ResolvedFile(index++, Path.Combine(file!["path"]?.GetValue<string>() ?? "", file["name"]!.GetValue<string>())));
   Files.SelectAll(); Files.Visibility = Files.Items.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
   if (initial is not null && Files.Items.Count == 1 && FileName.Text.Length == 0) FileName.Text = displayName;
   PrimaryButtonText = "開始下載";
  }
  finally { Busy.IsActive = false; Busy.Visibility = Visibility.Collapsed; }
 }
 private void ShowError(Exception e) { Message.Message = e.Message; Message.IsOpen = true; }
 private sealed record ResolvedFile(int Index, string Name) { public override string ToString() => Name; }
}
