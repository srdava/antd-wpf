using System;
using System.Windows;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// The handful of strings the controls render themselves. Anything a host supplies (button
    /// text, placeholders, table cells) is not here - only text that is baked into a template or a
    /// validation message.
    /// </summary>
    public sealed class AntdStrings
    {
        public AntdStrings()
        {
            RequiredField = "Please fill in this field";
            NoData = "No Data";
            Remove = "Remove";
            AllFiles = "All files (*.*)|*.*";
            Close = "Close";
            Ok = "OK";
            Cancel = "Cancel";
        }

        /// <summary>Validation message for a required <see cref="FormItem"/> that is empty.</summary>
        public string RequiredField { get; set; }

        /// <summary>Placeholder of <see cref="Empty"/>.</summary>
        public string NoData { get; set; }

        /// <summary>Remove action in the <see cref="Upload"/> file list.</summary>
        public string Remove { get; set; }

        /// <summary>Win32 file-dialog filter entry for "any file".</summary>
        public string AllFiles { get; set; }

        /// <summary>Accessible name of close buttons (Alert, Modal, Drawer, toast).</summary>
        public string Close { get; set; }

        /// <summary>Default confirm button of Modal and Popconfirm.</summary>
        public string Ok { get; set; }

        /// <summary>Default dismiss button of Modal and Popconfirm.</summary>
        public string Cancel { get; set; }

        public static AntdStrings CreateEnglish()
        {
            return new AntdStrings();
        }

        public static AntdStrings CreateChinese()
        {
            var strings = new AntdStrings();
            strings.RequiredField = "请填写此项";
            strings.NoData = "暂无数据";
            strings.Remove = "移除";
            strings.AllFiles = "所有文件 (*.*)|*.*";
            strings.Close = "关闭";
            strings.Ok = "确定";
            strings.Cancel = "取消";
            return strings;
        }

        public AntdStrings Clone()
        {
            return new AntdStrings
            {
                RequiredField = RequiredField,
                NoData = NoData,
                Remove = Remove,
                AllFiles = AllFiles,
                Close = Close,
                Ok = Ok,
                Cancel = Cancel
            };
        }
    }

    /// <summary>
    /// Process-wide UI strings. antd does this through ConfigProvider's locale prop; a static
    /// switch is enough here because the whole app shares one language.
    /// </summary>
    public static class AntdLocale
    {
        private static AntdStrings _current = AntdStrings.CreateEnglish();

        public static AntdStrings Current
        {
            get { return _current; }
        }

        /// <summary>Raised after the strings changed so templates can refresh.</summary>
        public static event EventHandler Changed;

        public static void Use(AntdStrings strings)
        {
            if (strings == null) throw new ArgumentNullException("strings");

            _current = strings;

            EventHandler handler = Changed;
            if (handler != null) handler(null, EventArgs.Empty);
        }

        /// <summary>Convenience switch for the two languages this port ships with.</summary>
        public static void UseCulture(string cultureName)
        {
            bool chinese = !string.IsNullOrEmpty(cultureName)
                           && cultureName.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
            Use(chinese ? AntdStrings.CreateChinese() : AntdStrings.CreateEnglish());
        }
    }

    /// <summary>
    /// Keeps a control in sync with <see cref="AntdLocale"/> the same way ThemeLink does with the
    /// theme: subscribe on Loaded, release on Unloaded so no control leaks the static event.
    /// </summary>
    internal static class LocaleLink
    {
        public static void Attach(FrameworkElement element, Action onLocaleChanged)
        {
            if (element == null || onLocaleChanged == null) return;

            EventHandler localeHandler = delegate { onLocaleChanged(); };

            element.Loaded += delegate { AntdLocale.Changed += localeHandler; onLocaleChanged(); };
            element.Unloaded += delegate { AntdLocale.Changed -= localeHandler; };
        }
    }
}
