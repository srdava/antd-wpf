using System;
using System.Collections.Generic;
using System.Windows.Media;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Theme
{
    /// <summary>
    /// Frozen brush cache keyed by antd token name. Every brush is frozen so WPF keeps
    /// rendering on the render thread; unfrozen brushes are the single biggest cause of
    /// frame drops in token driven themes.
    /// </summary>
    public sealed class BrushSet
    {
        private static readonly SolidColorBrush TransparentBrush = CreateFrozen(Colors.Transparent);

        private readonly Dictionary<string, SolidColorBrush> _brushes =
            new Dictionary<string, SolidColorBrush>(StringComparer.Ordinal);

        internal BrushSet(AliasToken tokens)
        {
            foreach (KeyValuePair<string, object> pair in tokens.Values)
            {
                AntdColor color = pair.Value is AntdColor ? (AntdColor)pair.Value : default(AntdColor);
                if (!(pair.Value is AntdColor)) continue;
                _brushes[pair.Key] = ColorInterop.ToFrozenBrush(color);
            }
        }

        public int Count
        {
            get { return _brushes.Count; }
        }

        public SolidColorBrush this[string tokenName]
        {
            get
            {
                SolidColorBrush brush;
                if (tokenName != null && _brushes.TryGetValue(tokenName, out brush))
                {
                    return brush;
                }
                return TransparentBrush;
            }
        }

        public bool Contains(string tokenName)
        {
            return tokenName != null && _brushes.ContainsKey(tokenName);
        }

        private static SolidColorBrush CreateFrozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
