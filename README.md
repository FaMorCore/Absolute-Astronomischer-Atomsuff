# Absolute-Astronomischer-Atomsuff

UDP-Client/-Server in **Python 3** mit einer Weboberfläche zum Ein- und
Ausschalten von Textmodifikationen. Es wird **nur die Python-Standardbibliothek**
verwendet – keine Installation zusätzlicher Pakete nötig.

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
python/
  server.py          UDP-Listener + HTTP-Server (Webseite) in einem Programm
  client.py          UDP-Client (Konsole)
  text_modifier.py   Modifikationen + thread-sicherer Einstellungs-Zustand
  ascii_art.py       Blockschrift für ASCII-Art
  web_page.py        eingebettete HTML-/JS-Oberfläche
scripts/
  deploy-server.sh   Deployment auf den Linux-Server (nur Kopieren + Starten)
```

## Voraussetzung

**Python 3** (getestet mit 3.11). Prüfen:

```powershell
python --version    # Windows
python3 --version   # Linux/macOS
```

Es muss **nichts** zusätzlich installiert werden.

## Lokal ausführen (zum Testen)

Zwei Terminals. Wechsle jeweils zuerst in den Ordner `python`:

```powershell
cd python

# Terminal 1 – Server (UDP 62500, Webseite http://localhost:62501)
python server.py

# Terminal 2 – Client gegen localhost
python client.py localhost 62500
```

(Unter Linux/macOS `python3` statt `python`.)

Im Browser <http://localhost:62501/> öffnen, Modifikationen einschalten, im
Client Text eingeben – der Client gibt den modifizierten Text aus.

### Ports / Konfiguration

Beide Programme lesen Argumente **oder** Umgebungsvariablen:

- Server: `python server.py [udpPort] [httpPort]` bzw. `UDP_PORT`, `HTTP_PORT`
  (Standard `62500` / `62501` – im vorgegebenen Bereich **62500–62599**).
- Client: `python client.py [host] [udpPort]` bzw. `SERVER_HOST`, `UDP_PORT`
  (Standard `if11c.berufsschule.fun` / `62500`).

## Deployment auf den Schul-Server

Login: `ssh fabian@if11c.berufsschule.fun` (Benutzer `fabian`, Passwort vom
Kursleiter). Port-Bereich für die Anwendung: **62500–62599**.

> Das Passwort wird bewusst **nicht** im Repository gespeichert. Es wird beim
> `ssh`/`scp` interaktiv abgefragt (oder per `sshpass` bereitgestellt).

Automatisch (kopiert die `.py`-Dateien und startet den Server):

```bash
./scripts/deploy-server.sh
```

Oder von Hand:

```bash
scp python/*.py fabian@if11c.berufsschule.fun:~/udp-text-modifier/
ssh fabian@if11c.berufsschule.fun
cd ~/udp-text-modifier
UDP_PORT=62500 HTTP_PORT=62501 python3 server.py
```

Danach:

- Webseite: <http://if11c.berufsschule.fun:62501/>
- Client gegen den Server:

```powershell
python client.py if11c.berufsschule.fun 62500
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
