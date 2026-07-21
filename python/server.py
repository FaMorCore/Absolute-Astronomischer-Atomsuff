#!/usr/bin/env python3
"""UDP-Text-Modifier - Server.

Der Server besteht aus zwei Teilen, die sich denselben Einstellungs-Zustand
(ModificationSettings) teilen:
  1. Ein UDP-Listener empfaengt Text, modifiziert ihn und schickt ihn zurueck.
  2. Ein kleiner HTTP-Server liefert eine Webseite, ueber die man die
     Modifikationen ein- und ausschalten kann.

Aufruf:
    python3 server.py [udpPort] [httpPort]

Standard:
    udpPort  = Umgebungsvariable UDP_PORT  oder 62500
    httpPort = Umgebungsvariable HTTP_PORT oder 62501
Beide Ports liegen sinnvollerweise im zugewiesenen Bereich 62500-62599.

Nur Python-Standardbibliothek - keine Installation noetig.
"""

import json
import os
import socket
import sys
import threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

import text_modifier
import web_page
from text_modifier import ModificationSettings


def resolve_port(arg, env_name, fallback):
    """Ermittelt einen Port aus Argument, dann Umgebungsvariable, dann Standard."""
    for candidate in (arg, os.environ.get(env_name)):
        if candidate:
            try:
                value = int(candidate)
                if 0 < value < 65536:
                    return value
            except ValueError:
                pass
    return fallback


def shorten(text):
    text = text.replace("\n", "\\n").replace("\r", "")
    return text if len(text) <= 60 else text[:60] + "..."


# ---------------------------------------------------------------------------
# UDP-Server
# ---------------------------------------------------------------------------
def run_udp_server(port, settings, stop_event):
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    sock.bind(("0.0.0.0", port))
    sock.settimeout(0.5)  # regelmaessig pruefen, ob beendet werden soll

    while not stop_event.is_set():
        try:
            data, addr = sock.recvfrom(65535)
        except socket.timeout:
            continue
        except OSError as exc:
            print(f"[UDP] Socket-Fehler: {exc}")
            continue

        text = data.decode("utf-8", errors="replace")
        output = text_modifier.apply(text, settings)

        u, c, a = settings.snapshot()
        print(f'[UDP] {addr} | aktiv: UPPER={u} CAMEL={c} ASCII={a} | "{shorten(text)}"')

        try:
            sock.sendto(output.encode("utf-8"), addr)
        except OSError as exc:
            print(f"[UDP] Antwort fehlgeschlagen: {exc}")

    sock.close()


# ---------------------------------------------------------------------------
# HTTP-Server (Konfigurations-Webseite + kleine JSON-API)
# ---------------------------------------------------------------------------
def make_handler(settings):
    class Handler(BaseHTTPRequestHandler):
        # eigenes, ruhigeres Logging
        def log_message(self, fmt, *args):
            pass

        def _send(self, status, body_bytes, content_type):
            self.send_response(status)
            self.send_header("Content-Type", content_type)
            self.send_header("Content-Length", str(len(body_bytes)))
            self.send_header("Cache-Control", "no-store")
            self.end_headers()
            self.wfile.write(body_bytes)

        def _send_text(self, status, text, content_type):
            self._send(status, text.encode("utf-8"), content_type)

        def _send_json(self, obj):
            self._send_text(200, json.dumps(obj), "application/json; charset=utf-8")

        def _read_json_body(self):
            length = int(self.headers.get("Content-Length", 0) or 0)
            if length <= 0:
                return {}
            raw = self.rfile.read(length)
            if not raw.strip():
                return {}
            try:
                return json.loads(raw.decode("utf-8"))
            except (ValueError, UnicodeDecodeError):
                return {}

        def do_GET(self):
            if self.path == "/":
                self._send_text(200, web_page.HTML, "text/html; charset=utf-8")
            elif self.path == "/api/settings":
                self._send_json(settings.as_dict())
            else:
                self._send_text(404, "Not Found", "text/plain; charset=utf-8")

        def do_POST(self):
            if self.path == "/api/settings":
                body = self._read_json_body()
                settings.update(
                    upper_case=body.get("upperCase"),
                    camel_case=body.get("camelCase"),
                    ascii_art=body.get("asciiArt"),
                )
                u, c, a = settings.snapshot()
                print(f"[WEB] Einstellungen geaendert -> UPPER={u} CAMEL={c} ASCII={a}")
                self._send_json(settings.as_dict())
            elif self.path == "/api/preview":
                body = self._read_json_body()
                text = body.get("text", "")
                if not isinstance(text, str):
                    text = ""
                result = text_modifier.apply(text, settings)
                self._send_json({"result": result})
            else:
                self._send_text(404, "Not Found", "text/plain; charset=utf-8")

    return Handler


def main():
    udp_port = resolve_port(sys.argv[1] if len(sys.argv) > 1 else None, "UDP_PORT", 62500)
    http_port = resolve_port(sys.argv[2] if len(sys.argv) > 2 else None, "HTTP_PORT", 62501)

    settings = ModificationSettings()
    stop_event = threading.Event()

    print("=== UDP-Text-Modifier Server ===")
    print(f"UDP-Listener : 0.0.0.0:{udp_port}")
    print(f"Webseite     : http://0.0.0.0:{http_port}/")
    print("Strg+C zum Beenden.\n")

    udp_thread = threading.Thread(
        target=run_udp_server, args=(udp_port, settings, stop_event), daemon=True
    )
    udp_thread.start()

    httpd = ThreadingHTTPServer(("0.0.0.0", http_port), make_handler(settings))

    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nBeende Server ...")
    finally:
        stop_event.set()
        httpd.shutdown()
        httpd.server_close()
        udp_thread.join(timeout=2)


if __name__ == "__main__":
    main()
