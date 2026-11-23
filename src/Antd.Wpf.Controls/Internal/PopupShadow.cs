using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace Antd.Wpf.Controls.Internal
{
    /// <summary>
    /// Gives a dropdown panel the antd shadow without letting it drift.
    ///
    /// The shadow needs 16px of room on every side, but WPF's popup placement knows nothing about
    /// that margin, so the panel ends up pushed right and down (or overlapping the field when the
    /// offset is guessed). Rather than hard-code a correction - which is not linear, the shadow
    /// bounds are recomputed every layout - the panel is measured after it opens and the delta is
    /// folded back into the popup offsets, at most three times.
    /// </summary>
    internal static class PopupShadow
    {
        private const double Gutter = 16;
        private const double Gap = 4;

        private static readonly HashSet<Popup> Wired = new HashSet<Popup>();

        public static void Apply(Popup popup, FrameworkElement owner)
        {
            if (popup == null || owner == null) return;

            var panel = popup.Child as FrameworkElement;
            if (panel == null) return;

            if (panel.Margin.Left <= 0) panel.Margin = new Thickness(Gutter);

            // The shadow lives in the theme dictionary, which is not part of any element's resource
            // scope, so look it up through the panel first and fall back to the same numbers.
            var effect = panel.TryFindResource("Antd.Effect.Popup") as Effect;
            if (effect == null) effect = owner.TryFindResource("Antd.Effect.Popup") as Effect;
            if (effect == null)
            {
                effect = new DropShadowEffect
                {
                    BlurRadius = 14,
                    ShadowDepth = 3,
                    Direction = 270,
                    Opacity = 0.16,
                    Color = System.Windows.Media.Colors.Black,
                    RenderingBias = RenderingBias.Quality
                };
            }
            panel.Effect = effect;

            if (!Wired.Add(popup)) return;

            popup.Opened += delegate
            {
                owner.Dispatcher.BeginInvoke(new Action(delegate { Align(popup, panel, owner, 0); }),
                    DispatcherPriority.Loaded);
            };
        }

        private static void Align(Popup popup, FrameworkElement panel, FrameworkElement owner, int pass)
        {
            if (!popup.IsOpen || owner.ActualHeight <= 0) return;

            Point field = owner.PointToScreen(new Point(0, 0));
            Point origin = panel.PointToScreen(new Point(0, 0));

            double dx = origin.X - field.X;
            double dy = origin.Y - (field.Y + owner.ActualHeight);

            if (Math.Abs(dx) < 0.5 && Math.Abs(dy - Gap) < 0.5) return;

            popup.HorizontalOffset -= dx;
            popup.VerticalOffset -= dy - Gap;

            if (pass >= 2) return;

            owner.Dispatcher.BeginInvoke(
                new Action(delegate { Align(popup, panel, owner, pass + 1); }),
                DispatcherPriority.Render);
        }
    }
}
