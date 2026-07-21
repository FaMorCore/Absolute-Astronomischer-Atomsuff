#!/usr/bin/env python3
"""UDP-Text-Modifier - Client.

Der Client sendet Text an den UDP-Server, empfaengt den vom Server
modifizierten Text zurueck und gibt ihn aus.

Aufruf:
    python3 client.py [host] [udpPort]

Standard:
    host    = Umgebungsvariable SERVER_HOST oder "if11c.berufsschule.fun"
    udpPort = Umgebungsvariable UDP_PORT    oder 62500

Bedienung:
    - Zeile eingeben und Enter -> Text wird gesendet, Antwort wird angezeigt.
    - Leere Zeile oder "exit" beendet das Programm.

Nur Python-Standardbibliothek - keine Installation noetig.
"""

import os
import socket
import sys


def resolve_port(arg, env_name, fallback):
    for candidate in (arg, os.environ.get(env_name)):
        if candidate:
            try:
                value = int(candidate)
                if 0 < value < 65536:
                    return value
            except ValueError:
                pass
    return fallback


def main():
    host = (
        (sys.argv[1] if len(sys.argv) > 1 else None)
        or os.environ.get("SERVER_HOST")
        or "if11c.berufsschule.fun"
    )
    port = resolve_port(sys.argv[2] if len(sys.argv) > 2 else None, "UDP_PORT", 62500)

    print("=== UDP-Text-Modifier Client ===")
    print(f"Server: {host}:{port}")
    print("Text eingeben und Enter druecken. Leere Zeile oder 'exit' beendet.\n")

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.settimeout(5.0)  # nicht ewig auf eine Antwort warten

    try:
        while True:
            try:
                line = input("> ")
            except EOFError:
                print("\nBeende Client.")
                break

            if line == "" or line.strip().lower() == "exit":
                print("Beende Client.")
                break

            try:
                sock.sendto(line.encode("utf-8"), (host, port))
                data, _ = sock.recvfrom(65535)
                response = data.decode("utf-8", errors="replace")

                print("--- Antwort vom Server ---")
                print(response)
                print("--------------------------\n")
            except socket.timeout:
                print("Zeitueberschreitung: keine Antwort vom Server erhalten.\n")
            except socket.gaierror as exc:
                print(f"Host nicht erreichbar ({host}): {exc}\n")
            except OSError as exc:
                print(f"Netzwerkfehler: {exc}\n")
    except KeyboardInterrupt:
        print("\nBeende Client.")
    finally:
        sock.close()


if __name__ == "__main__":
    main()
