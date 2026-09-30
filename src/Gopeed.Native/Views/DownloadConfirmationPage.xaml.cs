using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gopeed_Native.Services;
using System.Text.Json.Nodes;

namespace Gopeed_Native.Views;

public sealed partial class DownloadConfirmationPage : Page
{
    private readonly DownloadForm form;
    public event Action<string>? Started;
    public event Action? Cancelled;
    public DownloadConfirmationPage(CoreClient core, JsonObject request, nint owner)
    {
        InitializeComponent();
        form = new DownloadForm(core, request, owner, compact: true);
        FormHost.Content = form;
        form.StateChanged += () => { StartButton.Content = form.ActionText; StartButton.IsEnabled = !form.IsBusy; };
    }
    private async void Start(object sender, RoutedEventArgs e)
    {
        if (!await form.SubmitAsync()) return;
        if (form.CreatedTaskId is { } id) Started?.Invoke(id);
        else Cancelled?.Invoke();
    }
    private void Cancel(object sender, RoutedEventArgs e) => Cancelled?.Invoke();
}
