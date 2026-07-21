using System.Net.Sockets;
using System.Text;

// ---------------------------------------------------------------------------
// UDP-Text-Modifier – Client
//
// Der Client sendet Text an den UDP-Server, empfängt den vom Server
// modifizierten Text zurück und gibt ihn aus.
//
// Aufruf:
//   Client [host] [udpPort]
// Standard:
//   host    = Umgebungsvariable SERVER_HOST oder "if11c.berufsschule.fun"
//   udpPort = Umgebungsvariable UDP_PORT    oder 62500
//
// Bedienung:
//   - Zeile eingeben und Enter -> Text wird gesendet, Antwort wird angezeigt.
//   - Leere Zeile oder "exit" beendet das Programm.
// ---------------------------------------------------------------------------

string host = args.ElementAtOrDefault(0)
    ?? Environment.GetEnvironmentVariable("SERVER_HOST")
    ?? "if11c.berufsschule.fun";

int port = ResolvePort(args.ElementAtOrDefault(1), "UDP_PORT", 62500);

Console.WriteLine("=== UDP-Text-Modifier Client ===");
Console.WriteLine($"Server: {host}:{port}");
Console.WriteLine("Text eingeben und Enter drücken. Leere Zeile oder 'exit' beendet.\n");

using var udp = new UdpClient();
try
{
    udp.Connect(host, port);
}
catch (SocketException ex)
{
    Console.Error.WriteLine($"Verbindung zum Server fehlgeschlagen: {ex.Message}");
    return 1;
}

// Timeout, damit der Client bei Paketverlust nicht ewig hängt.
udp.Client.ReceiveTimeout = 5000;

while (true)
{
    Console.Write("> ");
    string? line = Console.ReadLine();

    if (line is null || line.Length == 0 || line.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Beende Client.");
        break;
    }

    byte[] payload = Encoding.UTF8.GetBytes(line);

    try
    {
        await udp.SendAsync(payload, payload.Length);

        // Antwort empfangen. ReceiveAsync respektiert ReceiveTimeout nicht direkt,
        // daher zusätzlich mit CancellationToken absichern.
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        UdpReceiveResult result = await udp.ReceiveAsync(cts.Token);
        string response = Encoding.UTF8.GetString(result.Buffer);

        Console.WriteLine("--- Antwort vom Server ---");
        Console.WriteLine(response);
        Console.WriteLine("--------------------------\n");
    }
    catch (OperationCanceledException)
    {
        Console.Error.WriteLine("Zeitüberschreitung: keine Antwort vom Server erhalten.\n");
    }
    catch (SocketException ex)
    {
        Console.Error.WriteLine($"Netzwerkfehler: {ex.Message}\n");
    }
}

return 0;

static int ResolvePort(string? arg, string envName, int fallback)
{
    if (int.TryParse(arg, out int fromArg) && fromArg is > 0 and < 65536)
        return fromArg;

    if (int.TryParse(Environment.GetEnvironmentVariable(envName), out int fromEnv) && fromEnv is > 0 and < 65536)
        return fromEnv;

    return fallback;
}
