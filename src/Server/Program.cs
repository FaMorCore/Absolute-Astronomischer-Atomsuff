using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UdpTextModifier.Server;

// ---------------------------------------------------------------------------
// UDP-Text-Modifier – Server
//
// Der Server besteht aus zwei Teilen, die sich denselben Einstellungs-Zustand
// (ModificationSettings) teilen:
//   1. Ein UDP-Listener empfängt Text, modifiziert ihn und schickt ihn zurück.
//   2. Ein kleiner HTTP-Server liefert eine Webseite, über die man die
//      Modifikationen ein- und ausschalten kann.
//
// Aufruf:
//   Server [udpPort] [httpPort]
// Standard:
//   udpPort  = Umgebungsvariable UDP_PORT  oder 62500
//   httpPort = Umgebungsvariable HTTP_PORT oder 62501
// Beide Ports liegen sinnvollerweise im zugewiesenen Bereich 62500–62599.
// ---------------------------------------------------------------------------

int udpPort = ResolvePort(args.ElementAtOrDefault(0), "UDP_PORT", 62500);
int httpPort = ResolvePort(args.ElementAtOrDefault(1), "HTTP_PORT", 62501);

var settings = new ModificationSettings();
using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nBeende Server ...");
    cts.Cancel();
};

Console.WriteLine("=== UDP-Text-Modifier Server ===");
Console.WriteLine($"UDP-Listener : 0.0.0.0:{udpPort}");
Console.WriteLine($"Webseite     : http://0.0.0.0:{httpPort}/");
Console.WriteLine("Strg+C zum Beenden.\n");

var udpTask = RunUdpServerAsync(udpPort, settings, cts.Token);
var httpTask = RunHttpServerAsync(httpPort, settings, cts.Token);

try
{
    await Task.WhenAll(udpTask, httpTask);
}
catch (OperationCanceledException)
{
    // Erwartet beim Beenden.
}

// ---------------------------------------------------------------------------
// UDP-Server
// ---------------------------------------------------------------------------
static async Task RunUdpServerAsync(int port, ModificationSettings settings, CancellationToken token)
{
    using var udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));

    while (!token.IsCancellationRequested)
    {
        UdpReceiveResult received;
        try
        {
            received = await udp.ReceiveAsync(token);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"[UDP] Socket-Fehler: {ex.Message}");
            continue;
        }

        string input = Encoding.UTF8.GetString(received.Buffer);
        string output = TextModifier.Apply(input, settings);

        var (u, c, a) = settings.Snapshot();
        Console.WriteLine($"[UDP] {received.RemoteEndPoint} | aktiv: " +
                          $"UPPER={u} CAMEL={c} ASCII={a} | \"{Shorten(input)}\"");

        byte[] response = Encoding.UTF8.GetBytes(output);
        try
        {
            await udp.SendAsync(response, response.Length, received.RemoteEndPoint);
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"[UDP] Antwort fehlgeschlagen: {ex.Message}");
        }
    }
}

// ---------------------------------------------------------------------------
// HTTP-Server (Konfigurations-Webseite + kleine JSON-API)
// ---------------------------------------------------------------------------
static async Task RunHttpServerAsync(int port, ModificationSettings settings, CancellationToken token)
{
    using var listener = new HttpListener();
    listener.Prefixes.Add($"http://+:{port}/");

    try
    {
        listener.Start();
    }
    catch (HttpListenerException ex)
    {
        Console.WriteLine($"[HTTP] Konnte nicht starten: {ex.Message}");
        Console.WriteLine("[HTTP] Hinweis: Unter Linux ggf. mit passenden Rechten / offenem Port starten.");
        return;
    }

    token.Register(() =>
    {
        try { listener.Stop(); } catch { /* ignore */ }
    });

    while (!token.IsCancellationRequested)
    {
        HttpListenerContext ctx;
        try
        {
            ctx = await listener.GetContextAsync();
        }
        catch (Exception) when (token.IsCancellationRequested)
        {
            break;
        }
        catch (HttpListenerException)
        {
            break;
        }

        _ = Task.Run(() => HandleHttpRequestAsync(ctx, settings));
    }
}

static async Task HandleHttpRequestAsync(HttpListenerContext ctx, ModificationSettings settings)
{
    try
    {
        string path = ctx.Request.Url?.AbsolutePath ?? "/";
        string method = ctx.Request.HttpMethod;

        switch (path)
        {
            case "/" when method == "GET":
                await WriteTextAsync(ctx, WebPage.Html, "text/html; charset=utf-8");
                break;

            case "/api/settings" when method == "GET":
                await WriteJsonAsync(ctx, SettingsToJson(settings));
                break;

            case "/api/settings" when method == "POST":
                await UpdateSettingsFromBodyAsync(ctx, settings);
                await WriteJsonAsync(ctx, SettingsToJson(settings));
                break;

            case "/api/preview" when method == "POST":
                await PreviewFromBodyAsync(ctx, settings);
                break;

            default:
                ctx.Response.StatusCode = 404;
                await WriteTextAsync(ctx, "Not Found", "text/plain; charset=utf-8");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[HTTP] Fehler: {ex.Message}");
        try
        {
            ctx.Response.StatusCode = 500;
            ctx.Response.Close();
        }
        catch { /* ignore */ }
    }
}

static async Task UpdateSettingsFromBodyAsync(HttpListenerContext ctx, ModificationSettings settings)
{
    using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding ?? Encoding.UTF8);
    string body = await reader.ReadToEndAsync();

    if (string.IsNullOrWhiteSpace(body))
    {
        return;
    }

    using var doc = JsonDocument.Parse(body);
    JsonElement root = doc.RootElement;

    if (root.TryGetProperty("upperCase", out var u) && u.ValueKind is JsonValueKind.True or JsonValueKind.False)
        settings.UpperCase = u.GetBoolean();

    if (root.TryGetProperty("camelCase", out var c) && c.ValueKind is JsonValueKind.True or JsonValueKind.False)
        settings.CamelCase = c.GetBoolean();

    if (root.TryGetProperty("asciiArt", out var a) && a.ValueKind is JsonValueKind.True or JsonValueKind.False)
        settings.AsciiArt = a.GetBoolean();

    var (nu, nc, na) = settings.Snapshot();
    Console.WriteLine($"[WEB] Einstellungen geändert -> UPPER={nu} CAMEL={nc} ASCII={na}");
}

static async Task PreviewFromBodyAsync(HttpListenerContext ctx, ModificationSettings settings)
{
    using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding ?? Encoding.UTF8);
    string body = await reader.ReadToEndAsync();

    string text = string.Empty;
    if (!string.IsNullOrWhiteSpace(body))
    {
        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
            text = t.GetString() ?? string.Empty;
    }

    string result = TextModifier.Apply(text, settings);
    await WriteJsonAsync(ctx, JsonSerializer.Serialize(new { result }));
}

static string SettingsToJson(ModificationSettings settings)
{
    var (u, c, a) = settings.Snapshot();
    return JsonSerializer.Serialize(new
    {
        upperCase = u,
        camelCase = c,
        asciiArt = a,
    });
}

static async Task WriteJsonAsync(HttpListenerContext ctx, string json)
    => await WriteTextAsync(ctx, json, "application/json; charset=utf-8");

static async Task WriteTextAsync(HttpListenerContext ctx, string content, string contentType)
{
    byte[] buffer = Encoding.UTF8.GetBytes(content);
    ctx.Response.ContentType = contentType;
    ctx.Response.ContentLength64 = buffer.Length;
    ctx.Response.Headers["Cache-Control"] = "no-store";
    await ctx.Response.OutputStream.WriteAsync(buffer);
    ctx.Response.Close();
}

// ---------------------------------------------------------------------------
// Hilfsfunktionen
// ---------------------------------------------------------------------------
static int ResolvePort(string? arg, string envName, int fallback)
{
    if (int.TryParse(arg, out int fromArg) && fromArg is > 0 and < 65536)
        return fromArg;

    if (int.TryParse(Environment.GetEnvironmentVariable(envName), out int fromEnv) && fromEnv is > 0 and < 65536)
        return fromEnv;

    return fallback;
}

static string Shorten(string s)
{
    s = s.Replace("\n", "\\n").Replace("\r", "");
    return s.Length <= 60 ? s : s[..60] + "...";
}
