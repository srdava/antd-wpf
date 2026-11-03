using System.Collections.Generic;
using System.Windows.Media;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Theme
{
    /// <summary>
    /// Immutable view of one resolved theme: the token table plus every brush materialised
    /// once and frozen. Swapping the snapshot is the single notification that refreshes
    /// the whole UI.
    /// </summary>
    public sealed class ThemeSnapshot
    {
        internal ThemeSnapshot(AliasToken tokens, ThemeAlgorithm algorithm)
        {
            Tokens = tokens;
            Algorithm = algorithm;
            Brushes = new BrushSet(tokens);
            FontFamily = BuildFontFamily(tokens.FontFamily);
            FontFamilyCode = BuildFontFamily(tokens.FontFamilyCode, "Consolas");
        }

        public AliasToken Tokens { get; private set; }

        public BrushSet Brushes { get; private set; }

        public ThemeAlgorithm Algorithm { get; private set; }

        public bool IsDark
        {
            get { return (Algorithm & ThemeAlgorithm.Dark) == ThemeAlgorithm.Dark; }
        }

        /// <summary>WPF-ready font stack derived from the antd CSS font-family.</summary>
        public FontFamily FontFamily { get; private set; }

        public FontFamily FontFamilyCode { get; private set; }

        public double FontSize
        {
            get { return Tokens.FontSize; }
        }

        public SolidColorBrush Brush(string tokenName)
        {
            return Brushes[tokenName];
        }

        public AntdColor Color(string tokenName)
        {
            return Tokens.Color(tokenName);
        }

        public double Number(string tokenName)
        {
            return Tokens.Number(tokenName);
        }

        // ---------------------------------------------------------------- font helpers

        private static readonly HashSet<string> IgnoredFamilies = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "sans-serif", "serif", "monospace", "cursive", "fantasy", "system-ui"
        };

        /// <summary>
        /// antd declares a browser font stack (-apple-system, BlinkMacSystemFont, ...) that WPF
        /// cannot resolve. We drop the non-Windows entries and prepend the Chinese UI font so
        /// the result matches the intended look on Windows.
        /// </summary>
        internal static FontFamily BuildFontFamily(string cssFontFamily, string firstChoice = "Microsoft YaHei UI")
        {
            var families = new List<string>();
            if (!string.IsNullOrEmpty(firstChoice)) families.Add(firstChoice);

            if (!string.IsNullOrEmpty(cssFontFamily))
            {
                string cleaned = cssFontFamily.Replace("'", string.Empty)
                                              .Replace("\"", string.Empty)
                                              .Replace("\r", " ")
                                              .Replace("\n", " ");

                foreach (string part in cleaned.Split(','))
                {
                    string name = part.Trim();
                    if (name.Length == 0) continue;
                    if (name[0] == '-') continue;
                    if (IgnoredFamilies.Contains(name)) continue;
                    if (name.IndexOf("Emoji", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                    if (name.IndexOf("Symbol", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                    if (families.Contains(name)) continue;
                    families.Add(name);
                }
            }

            if (families.Count == 0) families.Add("Segoe UI");
            return new FontFamily(string.Join(", ", families.ToArray()));
        }
    }
}
