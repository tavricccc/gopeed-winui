using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;

namespace Gopeed_Native.Services;

public sealed class CoreClient : IDisposable
{
    public static string DataDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GopeedNative");
    private readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private JsonObject session = null!;
    public string ApiAddress => $"http://127.0.0.1:{session["port"]}";
    public string Token => session["token"]!.GetValue<string>();
    public int ProcessId => session["pid"]!.GetValue<int>();

    public async Task ConnectAsync()
    {
        Directory.CreateDirectory(DataDirectory);
        var path = Path.Combine(DataDirectory, "session.json");
        if (File.Exists(path))
        {
            session = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            // A crashed core leaves a stale session. Restart only when its process no longer exists.
            try { using var process = Process.GetProcessById(ProcessId); if (process.ProcessName != "gopeed-core") session = null!; }
            catch (ArgumentException) { session = null!; }
        }
        if (session is null)
        {
            var exe = Path.Combine(AppContext.BaseDirectory, "Engine", "gopeed-core.exe");
            var start = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = DataDirectory };
            start.ArgumentList.Add("--data"); start.ArgumentList.Add(DataDirectory);
            using var child = Process.Start(start)!;
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < deadline)
            {
                if (child.HasExited) throw new IOException("下載核心無法啟動。請查看資料夾內的日誌。");
                if (File.Exists(path))
                {
                    var candidate = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
                    if (candidate["pid"]!.GetValue<int>() == child.Id) { session = candidate; break; }
                }
                await Task.Delay(100);
            }
            if (session is null) throw new TimeoutException("下載核心啟動逾時。");
        }
        http.BaseAddress = new Uri(ApiAddress + "/api/v1/");
        http.DefaultRequestHeaders.Add("X-Api-Token", Token);
        await GetAsync("info");
    }

    public Task<JsonNode?> GetAsync(string route) => SendAsync(HttpMethod.Get, route);
    public async Task<JsonNode?> SendAsync(HttpMethod method, string route, JsonNode? body = null)
    {
        using var request = new HttpRequestMessage(method, route);
        if (body is not null) request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        if (result["code"]!.GetValue<int>() != 0) throw new InvalidOperationException(result["msg"]!.GetValue<string>());
        return result["data"]?.DeepClone();
    }

    public async Task StopAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{session["controlPort"]}/shutdown");
        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
    public void Dispose() => http.Dispose();
}
