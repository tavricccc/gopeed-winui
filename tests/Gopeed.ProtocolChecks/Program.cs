using System.Text;
using System.Text.Json.Nodes;
using Gopeed_Native.Services;

var payload = """{"req":{"url":"https://example.com/file?a=1&b=2","extra":{"header":{"Referer":"https://example.com/","Cookie":"key=value"}}},"opts":{"name":"測試下載.zip"}}""";
var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
var link = "gopeed:///create?params=" + Uri.EscapeDataString(encoded);
var parsed = GopeedLink.Parse(link);
if (parsed.Route != "create" || !JsonNode.DeepEquals(parsed.Parameters, JsonNode.Parse(payload))) throw new Exception("Payload mismatch");
if (GopeedLink.FromCommandLine("\"C:\\Program Files\\app.exe\" \"" + link + "\"") != link) throw new Exception("Command line mismatch");
if (GopeedLink.Parse("gopeed:///create").Parameters != null) throw new Exception("Empty create mismatch");
if (GopeedLink.Parse("gopeed:///extension?params=" + Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"url":"https://github.com/example/extension"}"""))).Route != "extension") throw new Exception("Extension mismatch");
if (GopeedLink.Parse("gopeed:///create?params=" + encoded.TrimEnd('=').Replace('+', '-').Replace('/', '_')).Parameters?["opts"]?["name"]?.GetValue<string>() != "測試下載.zip") throw new Exception("Base64url mismatch");
try { GopeedLink.Parse("gopeed:///create?params=invalid!"); throw new Exception("Invalid payload accepted"); } catch (FormatException) { }
Console.WriteLine("Protocol checks passed: command line, UTF-8 payload, query escaping, headers, empty create, extension, base64url, invalid payload.");
