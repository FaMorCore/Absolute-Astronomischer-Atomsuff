"""Die eingebettete Konfigurations-Webseite (HTML/CSS/JS als String)."""

HTML = """<!DOCTYPE html>
<html lang="de">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>UDP-Text-Modifier - Steuerung</title>
  <style>
    :root {
      --bg: #0f172a;
      --card: #1e293b;
      --accent: #38bdf8;
      --text: #e2e8f0;
      --muted: #94a3b8;
      --on: #22c55e;
      --off: #475569;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      font-family: system-ui, -apple-system, "Segoe UI", Roboto, sans-serif;
      background: var(--bg);
      color: var(--text);
      min-height: 100vh;
      display: flex;
      justify-content: center;
      padding: 2rem 1rem;
    }
    .wrap { width: 100%; max-width: 640px; }
    h1 { font-size: 1.6rem; margin: 0 0 .25rem; }
    .sub { color: var(--muted); margin: 0 0 1.5rem; }
    .card {
      background: var(--card);
      border-radius: 14px;
      padding: 1.25rem 1.5rem;
      margin-bottom: 1.25rem;
      box-shadow: 0 8px 24px rgba(0,0,0,.35);
    }
    .toggle-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: .75rem 0;
      border-bottom: 1px solid rgba(148,163,184,.15);
    }
    .toggle-row:last-child { border-bottom: none; }
    .toggle-label { font-weight: 600; }
    .toggle-desc { color: var(--muted); font-size: .85rem; margin-top: .15rem; }
    .switch { position: relative; width: 54px; height: 30px; flex: 0 0 auto; }
    .switch input { opacity: 0; width: 0; height: 0; }
    .slider {
      position: absolute; cursor: pointer; inset: 0;
      background: var(--off); border-radius: 30px; transition: .2s;
    }
    .slider::before {
      content: ""; position: absolute; height: 22px; width: 22px;
      left: 4px; bottom: 4px; background: #fff; border-radius: 50%; transition: .2s;
    }
    input:checked + .slider { background: var(--on); }
    input:checked + .slider::before { transform: translateX(24px); }
    label.field { display: block; font-weight: 600; margin-bottom: .5rem; }
    textarea {
      width: 100%; min-height: 90px; resize: vertical;
      background: #0b1220; color: var(--text);
      border: 1px solid rgba(148,163,184,.25); border-radius: 10px;
      padding: .75rem; font-size: 1rem; font-family: inherit;
    }
    button {
      margin-top: .75rem; background: var(--accent); color: #082f49;
      border: none; border-radius: 10px; padding: .6rem 1.1rem;
      font-weight: 700; cursor: pointer; font-size: .95rem;
    }
    button:hover { filter: brightness(1.05); }
    pre {
      background: #0b1220; border: 1px solid rgba(148,163,184,.25);
      border-radius: 10px; padding: .75rem; overflow-x: auto;
      white-space: pre; font-size: .8rem; line-height: 1.15; margin: .75rem 0 0;
      min-height: 60px;
    }
    .status { font-size: .8rem; color: var(--muted); margin-top: .5rem; }
    .status.ok { color: var(--on); }
  </style>
</head>
<body>
  <div class="wrap">
    <h1>UDP-Text-Modifier</h1>
    <p class="sub">Steuerung der Textmodifikationen des UDP-Servers.</p>

    <div class="card">
      <div class="toggle-row">
        <div>
          <div class="toggle-label">GROSSBUCHSTABEN</div>
          <div class="toggle-desc">Wandelt den kompletten Text in Grossbuchstaben um.</div>
        </div>
        <label class="switch">
          <input type="checkbox" id="upperCase" />
          <span class="slider"></span>
        </label>
      </div>
      <div class="toggle-row">
        <div>
          <div class="toggle-label">CaMeLcAsE</div>
          <div class="toggle-desc">Jeder zweite Buchstabe wird gross bzw. klein geschrieben.</div>
        </div>
        <label class="switch">
          <input type="checkbox" id="camelCase" />
          <span class="slider"></span>
        </label>
      </div>
      <div class="toggle-row">
        <div>
          <div class="toggle-label">ASCII-Art</div>
          <div class="toggle-desc">Stellt den Text als grosse Blockschrift dar.</div>
        </div>
        <label class="switch">
          <input type="checkbox" id="asciiArt" />
          <span class="slider"></span>
        </label>
      </div>
      <div class="status" id="status">Lade Einstellungen ...</div>
    </div>

    <div class="card">
      <label class="field" for="preview-in">Vorschau (wendet die aktiven Modifikationen an)</label>
      <textarea id="preview-in" placeholder="Text eingeben ...">Hallo Welt</textarea>
      <button id="preview-btn">Vorschau anzeigen</button>
      <pre id="preview-out"></pre>
    </div>
  </div>

  <script>
    const ids = ["upperCase", "camelCase", "asciiArt"];
    const statusEl = document.getElementById("status");

    async function loadSettings() {
      const res = await fetch("/api/settings");
      const s = await res.json();
      ids.forEach(id => document.getElementById(id).checked = !!s[id]);
      setStatus("Einstellungen geladen.");
    }

    async function saveSettings() {
      const payload = {};
      ids.forEach(id => payload[id] = document.getElementById(id).checked);
      const res = await fetch("/api/settings", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });
      const s = await res.json();
      ids.forEach(id => document.getElementById(id).checked = !!s[id]);
      setStatus("Gespeichert");
      refreshPreview();
    }

    function setStatus(msg) {
      statusEl.textContent = msg;
      statusEl.classList.add("ok");
      setTimeout(() => statusEl.classList.remove("ok"), 1200);
    }

    async function refreshPreview() {
      const text = document.getElementById("preview-in").value;
      const res = await fetch("/api/preview", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ text })
      });
      const data = await res.json();
      document.getElementById("preview-out").textContent = data.result;
    }

    ids.forEach(id => document.getElementById(id).addEventListener("change", saveSettings));
    document.getElementById("preview-btn").addEventListener("click", refreshPreview);

    loadSettings().then(refreshPreview);
  </script>
</body>
</html>
"""
