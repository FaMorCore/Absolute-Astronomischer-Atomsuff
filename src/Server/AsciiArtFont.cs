namespace UdpTextModifier.Server;

/// <summary>
/// Eine kompakte 5-zeilige Blockschrift (figlet-artig) für die ASCII-Art-Modifikation.
/// Jeder Buchstabe besteht aus genau <see cref="Height"/> Zeilen gleicher Breite.
/// Nicht definierte Zeichen werden als Leerzeichen dargestellt.
/// </summary>
public static class AsciiArtFont
{
    public const int Height = 5;

    // Jedes Zeichen: 5 Zeilen. '#' = gesetzt, Leerzeichen = leer.
    private static readonly Dictionary<char, string[]> Glyphs = new()
    {
        [' '] = new[]
        {
            "    ",
            "    ",
            "    ",
            "    ",
            "    ",
        },
        ['A'] = new[]
        {
            " ### ",
            "#   #",
            "#####",
            "#   #",
            "#   #",
        },
        ['B'] = new[]
        {
            "#### ",
            "#   #",
            "#### ",
            "#   #",
            "#### ",
        },
        ['C'] = new[]
        {
            " ####",
            "#    ",
            "#    ",
            "#    ",
            " ####",
        },
        ['D'] = new[]
        {
            "#### ",
            "#   #",
            "#   #",
            "#   #",
            "#### ",
        },
        ['E'] = new[]
        {
            "#####",
            "#    ",
            "#### ",
            "#    ",
            "#####",
        },
        ['F'] = new[]
        {
            "#####",
            "#    ",
            "#### ",
            "#    ",
            "#    ",
        },
        ['G'] = new[]
        {
            " ####",
            "#    ",
            "#  ##",
            "#   #",
            " ####",
        },
        ['H'] = new[]
        {
            "#   #",
            "#   #",
            "#####",
            "#   #",
            "#   #",
        },
        ['I'] = new[]
        {
            "###",
            " # ",
            " # ",
            " # ",
            "###",
        },
        ['J'] = new[]
        {
            "  ###",
            "   # ",
            "   # ",
            "#  # ",
            " ##  ",
        },
        ['K'] = new[]
        {
            "#   #",
            "#  # ",
            "###  ",
            "#  # ",
            "#   #",
        },
        ['L'] = new[]
        {
            "#    ",
            "#    ",
            "#    ",
            "#    ",
            "#####",
        },
        ['M'] = new[]
        {
            "#   #",
            "## ##",
            "# # #",
            "#   #",
            "#   #",
        },
        ['N'] = new[]
        {
            "#   #",
            "##  #",
            "# # #",
            "#  ##",
            "#   #",
        },
        ['O'] = new[]
        {
            " ### ",
            "#   #",
            "#   #",
            "#   #",
            " ### ",
        },
        ['P'] = new[]
        {
            "#### ",
            "#   #",
            "#### ",
            "#    ",
            "#    ",
        },
        ['Q'] = new[]
        {
            " ### ",
            "#   #",
            "# # #",
            "#  # ",
            " ## #",
        },
        ['R'] = new[]
        {
            "#### ",
            "#   #",
            "#### ",
            "#  # ",
            "#   #",
        },
        ['S'] = new[]
        {
            " ####",
            "#    ",
            " ### ",
            "    #",
            "#### ",
        },
        ['T'] = new[]
        {
            "#####",
            "  #  ",
            "  #  ",
            "  #  ",
            "  #  ",
        },
        ['U'] = new[]
        {
            "#   #",
            "#   #",
            "#   #",
            "#   #",
            " ### ",
        },
        ['V'] = new[]
        {
            "#   #",
            "#   #",
            "#   #",
            " # # ",
            "  #  ",
        },
        ['W'] = new[]
        {
            "#   #",
            "#   #",
            "# # #",
            "## ##",
            "#   #",
        },
        ['X'] = new[]
        {
            "#   #",
            " # # ",
            "  #  ",
            " # # ",
            "#   #",
        },
        ['Y'] = new[]
        {
            "#   #",
            " # # ",
            "  #  ",
            "  #  ",
            "  #  ",
        },
        ['Z'] = new[]
        {
            "#####",
            "   # ",
            "  #  ",
            " #   ",
            "#####",
        },
        ['0'] = new[]
        {
            " ### ",
            "#  ##",
            "# # #",
            "##  #",
            " ### ",
        },
        ['1'] = new[]
        {
            " # ",
            "## ",
            " # ",
            " # ",
            "###",
        },
        ['2'] = new[]
        {
            " ### ",
            "#   #",
            "  ## ",
            " #   ",
            "#####",
        },
        ['3'] = new[]
        {
            "#### ",
            "    #",
            " ### ",
            "    #",
            "#### ",
        },
        ['4'] = new[]
        {
            "#  # ",
            "#  # ",
            "#####",
            "   # ",
            "   # ",
        },
        ['5'] = new[]
        {
            "#####",
            "#    ",
            "#### ",
            "    #",
            "#### ",
        },
        ['6'] = new[]
        {
            " ### ",
            "#    ",
            "#### ",
            "#   #",
            " ### ",
        },
        ['7'] = new[]
        {
            "#####",
            "    #",
            "   # ",
            "  #  ",
            "  #  ",
        },
        ['8'] = new[]
        {
            " ### ",
            "#   #",
            " ### ",
            "#   #",
            " ### ",
        },
        ['9'] = new[]
        {
            " ### ",
            "#   #",
            " ####",
            "    #",
            " ### ",
        },
        ['!'] = new[]
        {
            "#",
            "#",
            "#",
            " ",
            "#",
        },
        ['?'] = new[]
        {
            "#### ",
            "    #",
            "  ## ",
            "     ",
            "  #  ",
        },
        ['.'] = new[]
        {
            "  ",
            "  ",
            "  ",
            "  ",
            "##",
        },
        [','] = new[]
        {
            "  ",
            "  ",
            "  ",
            "##",
            " #",
        },
        ['-'] = new[]
        {
            "     ",
            "     ",
            "#####",
            "     ",
            "     ",
        },
        ['_'] = new[]
        {
            "     ",
            "     ",
            "     ",
            "     ",
            "#####",
        },
        [':'] = new[]
        {
            "  ",
            "##",
            "  ",
            "##",
            "  ",
        },
    };

    private static readonly string[] Unknown = new[]
    {
        "?????",
        "?????",
        "?????",
        "?????",
        "?????",
    };

    /// <summary>
    /// Wandelt eine (einzeilige) Eingabe in mehrzeilige ASCII-Art um.
    /// Zeilenumbrüche in der Eingabe werden beibehalten (jede Textzeile ergibt einen ASCII-Art-Block).
    /// </summary>
    public static string Render(string text)
    {
        var output = new System.Text.StringBuilder();
        var textLines = text.Replace("\r\n", "\n").Split('\n');

        for (int t = 0; t < textLines.Length; t++)
        {
            string line = textLines[t].ToUpperInvariant();
            var rows = new string[Height];
            for (int r = 0; r < Height; r++) rows[r] = string.Empty;

            foreach (char c in line)
            {
                string[] glyph = Glyphs.TryGetValue(c, out var g) ? g : Unknown;
                for (int r = 0; r < Height; r++)
                {
                    rows[r] += glyph[r] + " ";
                }
            }

            for (int r = 0; r < Height; r++)
            {
                output.Append(rows[r].TrimEnd());
                output.Append('\n');
            }

            if (t < textLines.Length - 1)
            {
                output.Append('\n');
            }
        }

        return output.ToString().TrimEnd('\n');
    }
}
