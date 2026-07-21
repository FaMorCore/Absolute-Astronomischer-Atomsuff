namespace UdpTextModifier.Server;

/// <summary>
/// Thread-sicherer Zustand, welche Textmodifikationen aktuell aktiv sind.
/// Wird von der Webseite (HTTP) gesetzt und vom UDP-Handler gelesen.
/// </summary>
public sealed class ModificationSettings
{
    private readonly object _lock = new();

    private bool _upperCase;
    private bool _camelCase;
    private bool _asciiArt;

    public bool UpperCase
    {
        get { lock (_lock) return _upperCase; }
        set { lock (_lock) _upperCase = value; }
    }

    public bool CamelCase
    {
        get { lock (_lock) return _camelCase; }
        set { lock (_lock) _camelCase = value; }
    }

    public bool AsciiArt
    {
        get { lock (_lock) return _asciiArt; }
        set { lock (_lock) _asciiArt = value; }
    }

    /// <summary>Liefert eine konsistente Momentaufnahme aller Flags.</summary>
    public (bool upperCase, bool camelCase, bool asciiArt) Snapshot()
    {
        lock (_lock)
        {
            return (_upperCase, _camelCase, _asciiArt);
        }
    }
}
