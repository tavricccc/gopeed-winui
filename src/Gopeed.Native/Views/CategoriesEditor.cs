using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json.Nodes;
using Gopeed_Native.Services;
using Windows.Storage.Pickers;

namespace Gopeed_Native.Views;

public sealed record DownloadCategory(string Name, string Path) { public override string ToString() => $"{Name} · {Path}"; }
internal sealed class CategoriesEditor : StackPanel
{
    private readonly ListView list = new() { MaxHeight = 180 };
    private readonly TextBox name = new() { Header = "分類名稱" };
    private readonly TextBox path = new() { Header = "儲存位置" };
    public CategoriesEditor()
    {
        Spacing = 12; Children.Add(list); Children.Add(name); Children.Add(path);
        list.SelectionChanged += (_, _) => { if (list.SelectedItem is DownloadCategory item) { name.Text = item.Name; path.Text = item.Path; } };
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var browse = new Button { Content = "瀏覽…" }; browse.Click += async (_, _) => { var picker = new FolderPicker(); picker.FileTypeFilter.Add("*"); WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle); var file = await picker.PickSingleFolderAsync(); if (file is not null) path.Text = file.Path; }; actions.Children.Add(browse);
        var add = new Button { Content = "新增／更新分類" }; add.Click += (_, _) => { if (name.Text.Trim().Length == 0 || !System.IO.Path.IsPathFullyQualified(path.Text)) return; var item = new DownloadCategory(name.Text.Trim(), path.Text.Trim()); var selected = list.SelectedIndex; if (selected >= 0) list.Items[selected] = item; else list.Items.Add(item); name.Text = ""; path.Text = ""; list.SelectedIndex = -1; }; actions.Children.Add(add);
        var remove = new Button { Content = "移除分類" }; remove.Click += (_, _) => { if (list.SelectedIndex >= 0) list.Items.RemoveAt(list.SelectedIndex); }; actions.Children.Add(remove); Children.Add(actions);
    }
    public void Load(JsonObject config) { list.Items.Clear(); foreach (var item in Read(config)) list.Items.Add(item); }
    public void Save(JsonObject config) => ConfigJson.Set(config, "extra.downloadCategories", new JsonArray(list.Items.Cast<DownloadCategory>().Select(x => (JsonNode?)new JsonObject { ["name"] = x.Name, ["path"] = x.Path, ["isBuiltIn"] = false, ["isDeleted"] = false }).ToArray()));
    public static IReadOnlyList<DownloadCategory> Read(JsonNode config) => (ConfigJson.Get(config, "extra.downloadCategories")?.AsArray() ?? []).Where(x => x?["isDeleted"]?.GetValue<bool>() != true).Select(x => new DownloadCategory(x!["name"]?.GetValue<string>() is { Length: > 0 } title ? title : x["nameKey"]?.GetValue<string>() switch { "categoryMusic" => "音樂", "categoryVideo" => "影片", "categoryDocument" => "文件", "categoryProgram" => "程式", "categoryArchive" => "壓縮檔", _ => "其他" }, x["path"]!.GetValue<string>())).ToList();
}
