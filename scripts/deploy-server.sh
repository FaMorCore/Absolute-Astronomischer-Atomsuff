#!/usr/bin/env bash
#
# Baut den UDP-Server als eigenständige Linux-Anwendung und kopiert ihn auf den
# Schul-Server. Startet ihn dort im Port-Bereich 62500–62599.
#
# Voraussetzung: .NET SDK 8, ssh & scp lokal vorhanden.
#
# Verwendung:
#   ./scripts/deploy-server.sh
#
# Konfiguration (per Umgebungsvariable überschreibbar):
set -euo pipefail

SSH_USER="${SSH_USER:-fabian}"
SSH_HOST="${SSH_HOST:-if11c.berufsschule.fun}"
REMOTE_DIR="${REMOTE_DIR:-/home/${SSH_USER}/udp-text-modifier}"
UDP_PORT="${UDP_PORT:-62500}"
HTTP_PORT="${HTTP_PORT:-62501}"

# Das Passwort wird bewusst NICHT im Repository gespeichert. Optionen:
#   - interaktiv bei ssh/scp eingeben (Passwort vom Kursleiter), oder
#   - `SSHPASS=... sshpass -e ...` verwenden (sshpass muss installiert sein).

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo "==> Baue Server (linux-x64, self-contained) ..."
dotnet publish "$ROOT_DIR/src/Server/Server.csproj" \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -o "$ROOT_DIR/publish/server"

echo "==> Lege Remote-Verzeichnis an: $REMOTE_DIR"
ssh "${SSH_USER}@${SSH_HOST}" "mkdir -p '$REMOTE_DIR'"

echo "==> Kopiere Binärdatei ..."
scp "$ROOT_DIR/publish/server/Server" "${SSH_USER}@${SSH_HOST}:${REMOTE_DIR}/Server"

echo "==> Starte Server auf ${SSH_HOST} (UDP $UDP_PORT / HTTP $HTTP_PORT) ..."
ssh "${SSH_USER}@${SSH_HOST}" bash -s <<EOF
  set -e
  cd '$REMOTE_DIR'
  chmod +x Server
  # eventuell laufenden Server beenden
  pkill -f '$REMOTE_DIR/Server' 2>/dev/null || true
  # im Hintergrund starten
  UDP_PORT=$UDP_PORT HTTP_PORT=$HTTP_PORT nohup ./Server > server.log 2>&1 &
  sleep 1
  echo "Server gestartet. Log: $REMOTE_DIR/server.log"
EOF

echo
echo "Fertig."
echo "  UDP-Server : ${SSH_HOST}:${UDP_PORT}"
echo "  Webseite   : http://${SSH_HOST}:${HTTP_PORT}/"
