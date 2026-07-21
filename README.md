# Absolute-Astronomischer-Atomsuff

UDP-Client/-Server in C# (.NET 8) mit einer Weboberfläche zum Ein- und
Ausschalten von Textmodifikationen.

## Idee

- Der **UDP-Client** sendet Text an den **UDP-Server**.
- Der **UDP-Server** modifiziert den Text (je nach aktivierten Modifikationen)
  und sendet ihn an den Client zurück.
- Der **UDP-Client** empfängt den modifizierten Text und gibt ihn aus.
- Über eine **Webseite** lassen sich die Modifikationen an- und ausschalten.

### Mögliche Textmodifikationen

| Modifikation      | Beschreibung                                          |
|-------------------|-------------------------------------------------------|
| `GROSSBUCHSTABEN` | Wandelt den gesamten Text in Großbuchstaben um.       |
| `CaMeLcAsE`       | Jeder zweite Buchstabe groß bzw. klein.               |
| `ASCII-Art`       | Stellt den Text als große Blockschrift dar.           |

Sind mehrere Modifikationen aktiv, werden sie in dieser Reihenfolge angewendet:
`GROSSBUCHSTABEN` → `CaMeLcAsE` → `ASCII-Art`.

## Projektstruktur

```
src/
  UdpTextModifier.sln
  Server/            UDP-Server + eingebettete Webseite (HTTP)
    Program.cs         UDP-Listener + HTTP-Server
    TextModifier.cs    Anwenden der Modifikationen
    AsciiArtFont.cs    Blockschrift für ASCII-Art
    ModificationSettings.cs  thread-sicherer Zustand
    WebPage.cs         eingebettete HTML-/JS-Oberfläche
  Client/            UDP-Client (Konsole)
    Program.cs
scripts/
  deploy-server.sh   Build + Deployment auf den Linux-Server
```

## Bauen

Voraussetzung: [.NET SDK 8](https://dotnet.microsoft.com/download).

```bash
cd src
dotnet build
```

## Lokal ausführen (zum Testen)

Zwei Terminals:

```bash
# Terminal 1 – Server (UDP 62500, Webseite http://localhost:62501)
dotnet run --project src/Server

# Terminal 2 – Client gegen localhost
dotnet run --project src/Client -- localhost 62500
```

Im Browser <http://localhost:62501/> öffnen, Modifikationen einschalten, im
Client Text eingeben – der Client gibt den modifizierten Text aus.

### Ports / Konfiguration

Beide Programme lesen Argumente **oder** Umgebungsvariablen:

- Server: `Server [udpPort] [httpPort]` bzw. `UDP_PORT`, `HTTP_PORT`
  (Standard `62500` / `62501` – im vorgegebenen Bereich **62500–62599**).
- Client: `Client [host] [udpPort]` bzw. `SERVER_HOST`, `UDP_PORT`
  (Standard `if11c.berufsschule.fun` / `62500`).

## Deployment auf den Schul-Server

Login: `ssh fabian@if11c.berufsschule.fun` (Benutzer `fabian`, Passwort vom
Kursleiter). Port-Bereich für die Anwendung: **62500–62599**.

> Das Passwort wird bewusst **nicht** im Repository gespeichert. Es wird beim
> `ssh`/`scp` interaktiv abgefragt (oder per `sshpass` bereitgestellt).

```bash
./scripts/deploy-server.sh
```

Das Skript

1. baut den Server als eigenständige `linux-x64`-Binärdatei,
2. kopiert sie per `scp` nach `~/udp-text-modifier` auf den Server,
3. startet sie dort (`UDP 62500`, Webseite `HTTP 62501`).

Danach:

- Webseite: <http://if11c.berufsschule.fun:62501/>
- Client gegen den Server:

```bash
dotnet run --project src/Client -- if11c.berufsschule.fun 62500
```

> Hinweis: Die gewählten Ports müssen in der Firewall des Servers geöffnet
> sein. Bei Bedarf andere Ports aus dem Bereich 62500–62599 verwenden
> (`UDP_PORT`/`HTTP_PORT`).

## Bedienung des Clients

```
> Hallo Welt
--- Antwort vom Server ---
HALLO WELT
--------------------------
```

Leere Zeile oder `exit` beendet den Client.
