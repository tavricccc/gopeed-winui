using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using System.Text.Json.Nodes;
using Gopeed_Native.Services;

namespace Gopeed_Native.Views;

internal sealed class SettingsFields
{
    private readonly List<(Action<JsonObject> Load, Action<JsonObject> Save)> bindings = [];
    public void Load(JsonObject config) { foreach (var field in bindings) field.Load(config); }
    public void Save(JsonObject config) { foreach (var field in bindings) field.Save(config); }
    public TextBox Text(Panel panel, string title, string path, bool lines = false, string initial = "", bool array = true)
    {
        var box = new TextBox { Header = title, AcceptsReturn = lines, TextWrapping = lines ? TextWrapping.Wrap : TextWrapping.NoWrap, MinHeight = lines ? 90 : 0, MaxHeight = lines ? 180 : double.PositiveInfinity };
        AutomationProperties.SetAutomationId(box, "Setting-" + path); panel.Children.Add(box);
        bindings.Add((c => box.Text = lines && array ? string.Join("\n", ConfigJson.Get(c, path)?.AsArray().Select(x => x!.GetValue<string>()) ?? []) : ConfigJson.Get(c, path)?.GetValue<string>() ?? initial,
            c => ConfigJson.Set(c, path, lines && array ? ConfigJson.Array(ConfigJson.Lines(box.Text)) : JsonValue.Create(box.Text.Trim())))); return box;
    }
    public PasswordBox Password(Panel panel, string title, string path)
    {
        var box = new PasswordBox { Header = title }; panel.Children.Add(box); AutomationProperties.SetAutomationId(box, "Setting-" + path);
        bindings.Add((c => box.Password = ConfigJson.Get(c, path)?.GetValue<string>() ?? "", c => ConfigJson.Set(c, path, JsonValue.Create(box.Password)))); return box;
    }
    public NumberBox Number(Panel panel, string title, string path, double initial, double min, double max, double scale = 1, bool fractional = false)
    {
        var box = new NumberBox { Header = title, Minimum = min, Maximum = max, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
        AutomationProperties.SetAutomationId(box, "Setting-" + path); panel.Children.Add(box);
        bindings.Add((c => box.Value = (ConfigJson.Get(c, path)?.GetValue<double>() ?? initial * scale) / scale, c =>
        {
            if (!double.IsFinite(box.Value) || box.Value < min || box.Value > max) throw new FormatException($"請輸入有效的{title}。 ");
            ConfigJson.Set(c, path, fractional ? JsonValue.Create(box.Value * scale) : JsonValue.Create((long)(box.Value * scale)));
        })); return box;
    }
    public CheckBox Toggle(Panel panel, string title, string path, bool initial = false)
    {
        var box = new CheckBox { Content = title }; panel.Children.Add(box); AutomationProperties.SetAutomationId(box, "Setting-" + path);
        bindings.Add((c => box.IsChecked = ConfigJson.Get(c, path)?.GetValue<bool>() ?? initial, c => ConfigJson.Set(c, path, JsonValue.Create(box.IsChecked == true)))); return box;
    }
}
