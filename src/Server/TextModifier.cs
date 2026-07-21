using System.Text;

namespace UdpTextModifier.Server;

/// <summary>
/// Enthält die eigentlichen Textmodifikationen und wendet die aktiven
/// Modifikationen der Reihe nach auf einen Eingabetext an.
/// </summary>
public static class TextModifier
{
    /// <summary>
    /// Wendet alle aktivierten Modifikationen an. Reihenfolge:
    /// 1. GROSSBUCHSTABEN, 2. CaMeLcAsE, 3. ASCII-Art.
    /// Ist keine Modifikation aktiv, wird der Text unverändert zurückgegeben.
    /// </summary>
    public static string Apply(string text, ModificationSettings settings)
    {
        var (upper, camel, ascii) = settings.Snapshot();
        string result = text;

        if (upper)
        {
            result = ToUpperCase(result);
        }

        if (camel)
        {
            result = ToCamelCase(result);
        }

        if (ascii)
        {
            result = ToAsciiArt(result);
        }

        return result;
    }

    /// <summary>Wandelt den gesamten Text in GROSSBUCHSTABEN um.</summary>
    public static string ToUpperCase(string text) => text.ToUpperInvariant();

    /// <summary>
    /// Wandelt den Text in CaMeLcAsE um: jeder zweite Buchstabe groß bzw. klein.
    /// Gezählt werden nur Buchstaben – Leer- und Sonderzeichen unterbrechen das
    /// Muster nicht.
    /// </summary>
    public static string ToCamelCase(string text)
    {
        var sb = new StringBuilder(text.Length);
        int letterIndex = 0;

        foreach (char c in text)
        {
            if (char.IsLetter(c))
            {
                sb.Append(letterIndex % 2 == 0
                    ? char.ToUpperInvariant(c)
                    : char.ToLowerInvariant(c));
                letterIndex++;
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>Wandelt den Text in mehrzeilige ASCII-Art (Blockschrift) um.</summary>
    public static string ToAsciiArt(string text) => AsciiArtFont.Render(text);
}
