using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// One switch for every piece of motion the library owns. Defaults to the Windows
    /// "show animations" setting so a user who turned animations off (or has reduced motion on)
    /// does not get zooming dialogs; a host can also pin it for a kiosk or a test.
    /// </summary>
    public static class AntdMotion
    {
        private static bool? _enabled;

        /// <summary>True when dialogs should animate. Defaults to the OS preference.</summary>
        public static bool Enabled
        {
            get { return _enabled.HasValue ? _enabled.Value : SystemParameters.ClientAreaAnimation; }
            set { _enabled = value; }
        }

        /// <summary>
        /// Applies the preference to a popup. Called from each control's template hook so the
        /// fade in dropdowns/tooltips stops when the user asked for less motion.
        /// </summary>
        public static Popup Configure(Popup popup)
        {
            if (popup != null)
            {
                popup.PopupAnimation = Enabled ? PopupAnimation.Fade : PopupAnimation.None;
            }
            return popup;
        }
        /// <summary>Goes back to following the operating system.</summary>
        public static void UseSystemDefault()
        {
            _enabled = null;
        }
    }
}
