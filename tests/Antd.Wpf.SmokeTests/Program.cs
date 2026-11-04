using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Antd.Wpf.Controls;
using AntdButton = Antd.Wpf.Controls.Button;
using AntdInput = Antd.Wpf.Controls.Input;
using AntdCalendar = Antd.Wpf.Controls.Calendar;
using AntdAutoComplete = Antd.Wpf.Controls.AutoComplete;
using AntdDatePicker = Antd.Wpf.Controls.DatePicker;
using Antd.Wpf.Icons;
using Antd.Wpf.Gallery;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.SmokeTests
{
    /// <summary>
    /// M0 acceptance harness. Runs headless on both net48 and net8.0-windows:
    ///   1. token parity sample (full parity lives in Antd.Wpf.TokenParity)
    ///   2. controls instantiate, resolve tokens, and react to a theme switch
    ///   3. optional: render the gallery to PNG so the result can be inspected visually
    /// </summary>
    internal static class Program
    {
        private static readonly List<string> Failures = new List<string>();
        private static int _checks;

        [STAThread]
        private static int Main(string[] args)
        {
            string shotDir = null;
            string metricsPath = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--shot" && i + 1 < args.Length) shotDir = args[i + 1];
                if (args[i] == "--metrics" && i + 1 < args.Length) metricsPath = args[i + 1];
            }

            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

            // Framework chrome (scroll bars) lives in a separate dictionary the host merges.
            app.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml")
            });

            ThemeManager.Current.Attach(app.Resources);
            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
            DoEvents();

            if (metricsPath != null)
            {
                try { MetricsProbe.Run(metricsPath); }
                catch (Exception ex) { Console.WriteLine("METRICS FAILED: " + ex); return 2; }
                app.Shutdown();
                return 0;
            }

            Console.WriteLine("Antd.Wpf M0 smoke tests");
            Console.WriteLine("------------------------------");

            CheckArchitecture();
            CheckTokens();
            CheckControls();
            CheckThemeSwitch();
            CheckM1Components();
            CheckM2Components();
            CheckVisualParity();
            CheckAutoCompleteAndCalendar();
            CheckDatePicker();
            CheckM1Tail();
            CheckTree();
            CheckTimePickerAndMentions();

            if (shotDir != null) CaptureGallery(shotDir);

            Console.WriteLine();
            Console.WriteLine("checks: " + _checks + ", failures: " + Failures.Count);
            foreach (string failure in Failures) Console.WriteLine("  FAIL " + failure);
            Console.WriteLine(Failures.Count == 0 ? "RESULT: PASS" : "RESULT: FAIL");

            app.Shutdown();
            return Failures.Count == 0 ? 0 : 1;
        }

        // ---------------------------------------------------------------- architecture

        private static void CheckArchitecture()
        {
            System.Reflection.Assembly tokensAssembly = typeof(AliasToken).Assembly;
            System.Reflection.Assembly themeAssembly = typeof(ThemeManager).Assembly;

            string[] wpfAssemblies = { "WindowsBase", "PresentationCore", "PresentationFramework", "System.Xaml" };
            string[] tokenRefs = tokensAssembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
            string[] themeRefs = themeAssembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();

            Check("Tokens has no WPF dependency",
                !tokenRefs.Any(name => wpfAssemblies.Contains(name)),
                string.Join(", ", tokenRefs));

            Check("Theme does not reference Controls",
                !themeRefs.Contains("Antd.Wpf.Controls"),
                string.Join(", ", themeRefs));

            Check("Controls targets both TFMs",
                new[] { "net48", "net8.0-windows" }.All(tfm =>
                    System.IO.Directory.Exists(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Antd.Wpf.Controls", "bin", "Debug", tfm))),
                "missing build output");
        }

        // ---------------------------------------------------------------- tokens

        private static void CheckTokens()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            Check("token count is 536", t.Values.Count == 536, t.Values.Count.ToString());
            Check("colorPrimary", t.ColorPrimary == AntdColor.Parse("#1677ff"), t.ColorPrimary.ToString());
            Check("colorText light", t.ColorText == AntdColor.Parse("rgba(0,0,0,0.88)"), t.ColorText.ToString());
            Check("fontSize 14", t.FontSize == 14, t.FontSize.ToString());
            Check("lineHeight 22/14", Math.Abs(t.LineHeight - 22.0 / 14.0) < 1e-9, t.LineHeight.ToString("R"));
            Check("controlHeight 32", t.ControlHeight == 32, t.ControlHeight.ToString());
            Check("borderRadius 6", t.BorderRadius == 6, t.BorderRadius.ToString());
            Check("borderRadiusLG 8", t.BorderRadiusLG == 8, t.BorderRadiusLG.ToString());
            Check("colorBorder #d9d9d9", t.ColorBorder == AntdColor.Parse("#d9d9d9"), t.ColorBorder.ToString());
            Check("colorPrimaryBg #e6f4ff", t.ColorPrimaryBg == AntdColor.Parse("#e6f4ff"), t.ColorPrimaryBg.ToString());

            Check("brush cache covers every color token",
                snapshot.Brushes.Count > 200, snapshot.Brushes.Count.ToString());
            Check("brush is frozen", snapshot.Brush("colorPrimary").IsFrozen, "not frozen");
        }

        // ---------------------------------------------------------------- controls

        private static void CheckControls()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            var button = new AntdButton { Content = "Primary", Variant = ButtonVariant.Primary };
            var defaultButton = new AntdButton { Content = "Default" };
            var disabledButton = new AntdButton { Content = "Disabled", IsEnabled = false };
            var input = new AntdInput { Width = 180, Placeholder = "hello" };
            var smallInput = new AntdInput { Width = 120, Size = AntdSize.Small };
            var errorInput = new AntdInput { Width = 180, Status = AntdStatus.Error };

            var host = new Window
            {
                Width = 640,
                Height = 480,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Content = new StackPanel
                {
                    Children = { button, defaultButton, disabledButton, input, smallInput, errorInput }
                }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("primary background = colorPrimary",
                SameBrush(button.Background, snapshot.Brush("colorPrimary")), Describe(button.Background));
            Check("primary foreground = colorTextLightSolid",
                SameBrush(button.Foreground, snapshot.Brush("colorTextLightSolid")), Describe(button.Foreground));
            Check("default background = colorBgContainer",
                SameBrush(defaultButton.Background, snapshot.Brush("colorBgContainer")), Describe(defaultButton.Background));
            Check("default border = colorBorder",
                SameBrush(defaultButton.BorderBrush, snapshot.Brush("colorBorder")), Describe(defaultButton.BorderBrush));
            Check("disabled background = colorBgContainerDisabled",
                SameBrush(disabledButton.Background, snapshot.Brush("colorBgContainerDisabled")),
                Describe(disabledButton.Background));

            Check("button height 32", Math.Abs(button.MinHeight - 32) < 0.01, button.MinHeight.ToString());
            Check("button padding inline 15",
                Math.Abs(button.Padding.Left - 15) < 0.01 && Math.Abs(button.Padding.Right - 15) < 0.01,
                button.Padding.ToString());
            Check("button corner radius 6", Math.Abs(button.CornerRadius.TopLeft - 6) < 0.01,
                button.CornerRadius.TopLeft.ToString());

            button.Size = AntdSize.Small;
            Check("small button height 24", Math.Abs(button.MinHeight - 24) < 0.01, button.MinHeight.ToString());
            Check("small button radius 4", Math.Abs(button.CornerRadius.TopLeft - 4) < 0.01,
                button.CornerRadius.TopLeft.ToString());
            Check("small button padding 7", Math.Abs(button.Padding.Left - 7) < 0.01, button.Padding.ToString());

            button.Size = AntdSize.Large;
            Check("large button height 40", Math.Abs(button.MinHeight - 40) < 0.01, button.MinHeight.ToString());
            Check("large button font 16", Math.Abs(button.FontSize - 16) < 0.01, button.FontSize.ToString());
            button.Size = AntdSize.Middle;

            Check("input background = colorBgContainer",
                SameBrush(input.Background, snapshot.Brush("colorBgContainer")), Describe(input.Background));
            Check("input height 32", Math.Abs(input.MinHeight - 32) < 0.01, input.MinHeight.ToString());
            Check("small input height 24", Math.Abs(smallInput.MinHeight - 24) < 0.01, smallInput.MinHeight.ToString());
            Check("error input border = colorError",
                SameBrush(errorInput.BorderBrush, snapshot.Brush("colorError")), Describe(errorInput.BorderBrush));

            var clearableInput = new AntdInput { Width = 180, AllowClear = true, Text = "value" };
            var host2 = new Window
            {
                Width = 320, Height = 200, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { Children = { clearableInput } }
            };
            host2.Show();
            DoEvents();
            host2.UpdateLayout();
            var clearHost = clearableInput.Template.FindName("PART_ClearButton", clearableInput) as FrameworkElement;
            var ringHost = input.Template.FindName("FocusRing", input) as FrameworkElement;
            var placeholderHost = input.Template.FindName("PlaceholderText", input) as FrameworkElement;

            Check("placeholder shown when empty", input.ShowPlaceholder
                && placeholderHost != null && placeholderHost.Visibility == Visibility.Visible,
                placeholderHost == null ? "part missing" : placeholderHost.Visibility.ToString());
            Check("placeholder hidden when filled",
                !clearableInput.ShowPlaceholder
                && (clearableInput.Template.FindName("PlaceholderText", clearableInput) as FrameworkElement).Visibility == Visibility.Collapsed,
                clearableInput.ShowPlaceholder.ToString());
            Check("clear button visible only when clearable", clearableInput.CanClear
                && clearHost != null && clearHost.Visibility == Visibility.Visible,
                clearHost == null ? "part missing" : clearHost.Visibility.ToString());
            Check("clear button hidden when not clearable",
                !input.CanClear
                && (input.Template.FindName("PART_ClearButton", input) as FrameworkElement).Visibility == Visibility.Collapsed,
                "still visible");
            Check("focus ring hidden while unfocused",
                ringHost != null && ringHost.Visibility == Visibility.Collapsed,
                ringHost == null ? "part missing" : ringHost.Visibility.ToString());
            host2.Close();

            Check("template applied on button", button.Template != null, "template is null");
            Check("template applied on input", input.Template != null, "template is null");
            Check("rendered button has size",
                button.ActualWidth > 0 && button.ActualHeight > 0,
                button.ActualWidth + "x" + button.ActualHeight);

            _testHost = host;
            _testButton = button;
        }

        private static Window _testHost;
        private static AntdButton _testButton;

        // ---------------------------------------------------------------- M1 components

        private static void CheckM1Components()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            var icon = new Icon { Data = AntdIcons.Search, FontSize = 14 };
            Check("icon geometry resolved", AntdIcons.Search != null, "null");
            Check("icon frozen", AntdIcons.Search != null && AntdIcons.Search.IsFrozen, "not frozen");
            Check("icon sizes to 1em of the font", Math.Abs(icon.Width - icon.FontSize) < 0.01, icon.Width.ToString());

            var divider = new Divider { Text = "Hello" };
            Check("divider content from Text", (string)divider.Content == "Hello", "content not set");
            Check("divider line = colorSplit",
                SameBrush(divider.LineBrush, snapshot.Brush("colorSplit")), Describe(divider.LineBrush));

            var tag = new Tag { Content = "ok", Color = AntdTagColor.Success };
            Check("success tag background = colorSuccessBg",
                SameBrush(tag.TagBackground, snapshot.Brush("colorSuccessBg")), Describe(tag.TagBackground));

            var badge = new Badge { Count = 128 };
            Check("badge overflow 99 -> 99+", badge.DisplayText == "99+", badge.DisplayText);
            badge.ShowZero = false;
            badge.Count = 0;
            Check("badge hidden at zero", !badge.IsBadgeVisible, "visible");

            var avatar = new Avatar { Size = 40 };
            Check("avatar circle radius", Math.Abs(avatar.AvatarCornerRadius.TopLeft - 20) < 0.01,
                avatar.AvatarCornerRadius.TopLeft.ToString());
            avatar.Shape = AntdAvatarShape.Square;
            Check("avatar square radius = borderRadiusLG",
                Math.Abs(avatar.AvatarCornerRadius.TopLeft - t.BorderRadiusLG) < 0.01,
                avatar.AvatarCornerRadius.TopLeft.ToString());

            var statistic = new Statistic { Value = 1234.5, Precision = 2 };
            Check("statistic formats value",
                statistic.ValueText == 1234.5.ToString("N2"), statistic.ValueText);

            var progress = new Progress { Type = AntdProgressType.Circle, CircleSize = 88 };
            progress.Measure(new Size(400, 400));
            Check("circle progress uses CircleSize",
                Math.Abs(progress.DesiredSize.Width - 88) < 0.01 && Math.Abs(progress.DesiredSize.Height - 88) < 0.01,
                progress.DesiredSize.ToString());

            var lineProgress = new Progress { Percent = 40 };
            lineProgress.Measure(new Size(200, 100));
            Check("line progress fills available width",
                Math.Abs(lineProgress.DesiredSize.Width - 200) < 0.01, lineProgress.DesiredSize.Width.ToString());

            var skeleton = new Skeleton { Rows = 3, Title = true };
            skeleton.Measure(new Size(200, 500));
            double expected = 4 * 16 + 3 * 16;
            Check("skeleton height matches 4 bars",
                Math.Abs(skeleton.DesiredSize.Height - expected) < 0.01, skeleton.DesiredSize.Height.ToString());

            var space = new Space();
            space.Children.Add(new Border { Width = 10, Height = 10 });
            space.Children.Add(new Border { Width = 10, Height = 10 });
            space.Children.Add(new Border { Width = 10, Height = 10 });
            space.Measure(new Size(500, 100));
            Check("space applies gap between items only",
                Math.Abs(space.DesiredSize.Width - (30 + 2 * 16)) < 0.01, space.DesiredSize.Width.ToString());

            var segmented = new Segmented();
            segmented.ItemsSource = new[] { "A", "B", "C" };
            segmented.SelectedIndex = 1;
            Check("segmented selection", segmented.SelectedItem as string == "B",
                segmented.SelectedItem as string ?? "null");
        }

        // ---------------------------------------------------------------- M2 components

        private static void CheckM2Components()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            var text = new Antd.Wpf.Controls.Text { Text = "hi", Type = TextType.Secondary };
            Check("secondary text uses colorTextSecondary",
                SameBrush(text.Foreground, snapshot.Brush("colorTextSecondary")), Describe(text.Foreground));

            var title = new Antd.Wpf.Controls.Title { Text = "h1", Level = 1 };
            Check("title level 1 uses fontSizeHeading1",
                Math.Abs(title.FontSize - t.FontSizeHeading1) < 0.01, title.FontSize.ToString());

            var paragraph = new Antd.Wpf.Controls.Paragraph { Text = "p" };
            Check("paragraph line height equals fontHeight",
                Math.Abs(paragraph.LineHeight - t.FontHeight) < 0.01, paragraph.LineHeight.ToString());

            var number = new Antd.Wpf.Controls.InputNumber { Value = 3.5, Precision = 2 };
            Check("input number formats with precision", number.Text == "3.50", number.Text);

            var checkbox = new Antd.Wpf.Controls.Checkbox { Content = "x", IsChecked = true };
            Check("checked checkbox is primary",
                SameBrush(checkbox.BoxBrush, snapshot.Brush("colorPrimary")), Describe(checkbox.BoxBrush));

            var radio = new Antd.Wpf.Controls.Radio { Content = "r", IsChecked = true };
            Check("checked radio ring is primary",
                SameBrush(radio.RingBrush, snapshot.Brush("colorPrimary")), Describe(radio.RingBrush));

            var toggle = new Antd.Wpf.Controls.Switch { IsChecked = true };
            Check("switch on uses colorPrimary",
                SameBrush(toggle.TrackBrush, snapshot.Brush("colorPrimary")), Describe(toggle.TrackBrush));
            Check("switch track width 44", Math.Abs(toggle.TrackWidth - 44) < 0.01, toggle.TrackWidth.ToString());

            var rate = new Antd.Wpf.Controls.Rate { Count = 5, StarSize = 20, Gap = 8 };
            rate.Measure(new Size(500, 100));
            Check("rate width = count*star + gaps", Math.Abs(rate.DesiredSize.Width - 132) < 0.01,
                rate.DesiredSize.Width.ToString());

            var slider = new Antd.Wpf.Controls.Slider();
            Check("slider track thickness 4", Math.Abs(slider.TrackThickness - 4) < 0.01,
                slider.TrackThickness.ToString());

            var select = new Antd.Wpf.Controls.Select { AllowClear = true };
            select.ItemsSource = new[] { "a", "b" };
            select.SelectedIndex = 0;
            Check("select reports selection", select.HasSelection && select.CanClearSelection,
                select.HasSelection + "/" + select.CanClearSelection);

            var formItem = new Antd.Wpf.Controls.FormItem
            {
                Label = "Name",
                Required = true,
                Content = new AntdInput { Placeholder = "x" }
            };
            bool valid = formItem.Validate();
            Check("required form item fails when empty", !valid && formItem.Error != null,
                formItem.Error ?? "no error");
            formItem.Content = new AntdInput { Text = "filled" };
            Check("required form item passes when filled", formItem.Validate() && formItem.Error == null,
                formItem.Error ?? "ok");

            var row = new Antd.Wpf.Controls.Row { Gutter = 16 };
            row.Measure(new Size(800, 200));
            Check("row breakpoint at 800 is Md",
                row.CurrentBreakpoint == Antd.Wpf.Controls.Breakpoint.Md, row.CurrentBreakpoint.ToString());

            var col = new Antd.Wpf.Controls.Col { Span = 24, Md = 12 };
            Check("col responsive span resolves", col.ResolveSpan(Antd.Wpf.Controls.Breakpoint.Md) == 12,
                col.ResolveSpan(Antd.Wpf.Controls.Breakpoint.Md).ToString());
            Check("col falls back to Span below breakpoint",
                col.ResolveSpan(Antd.Wpf.Controls.Breakpoint.Sm) == 24,
                col.ResolveSpan(Antd.Wpf.Controls.Breakpoint.Sm).ToString());

            var headerProbe = new Antd.Wpf.Controls.Header();
            Check("header padding is paddingLG", Math.Abs(headerProbe.Padding.Left - t.PaddingLG) < 0.01,
                headerProbe.Padding.ToString() + " paddingLG=" + t.PaddingLG);
            var contentProbe = new Antd.Wpf.Controls.Content();
            Check("content padding is paddingLG", Math.Abs(contentProbe.Padding.Left - t.PaddingLG) < 0.01,
                contentProbe.Padding.ToString());

            var watermark = new Antd.Wpf.Controls.Watermark { Text = "draft" };
            Check("watermark builds a tile brush", watermark.TileBrush != null, "null");
        }

        // ---------------------------------------------------------------- visual parity

        /// <summary>
        /// Guards the numbers that tools/visual-ref measures from real antd. If one of these
        /// drifts the port stops looking like the reference, even when every token still matches.
        /// </summary>
        private static void CheckVisualParity()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;

            var button = new AntdButton { Content = "Primary", Variant = ButtonVariant.Primary };
            Check("button height is controlHeight", Math.Abs(button.MinHeight - t.ControlHeight) < 0.01,
                button.MinHeight.ToString());
            Check("button paddingInline is 15", Math.Abs(button.Padding.Left - 15) < 0.01,
                button.Padding.Left.ToString());

            var small = new AntdButton { Content = "Small", Size = AntdSize.Small };
            Check("small button is 24 tall with radius 4",
                Math.Abs(small.MinHeight - t.ControlHeightSM) < 0.01
                && Math.Abs(small.CornerRadius.TopLeft - t.BorderRadiusSM) < 0.01,
                small.MinHeight + "/" + small.CornerRadius.TopLeft);

            var large = new AntdButton { Content = "Large", Size = AntdSize.Large };
            Check("large button is 40 tall with radius 8",
                Math.Abs(large.MinHeight - t.ControlHeightLG) < 0.01
                && Math.Abs(large.CornerRadius.TopLeft - t.BorderRadiusLG) < 0.01,
                large.MinHeight + "/" + large.CornerRadius.TopLeft);

            var tag = new Tag { Content = "Tag" };
            Check("tag is 22 tall with 7px inline padding",
                Math.Abs(tag.MinHeight - 22) < 0.01 && Math.Abs(tag.Padding.Left - 7) < 0.01,
                tag.MinHeight + "/" + tag.Padding.Left);

            var divider = new Divider { Text = "Section" };
            Check("divider text is 16px at weight 500",
                Math.Abs(divider.FontSize - t.FontSizeLG) < 0.01 && divider.TextFontWeight == FontWeights.Medium,
                divider.FontSize + "/" + divider.TextFontWeight);
            Check("divider line uses colorSplit",
                SameBrush(divider.LineBrush, ThemeManager.Current.Snapshot.Brush("colorSplit")),
                Describe(divider.LineBrush));

            // A stretched Progress must never paint its percentage outside its own bounds; this
            // regressed once when FontSize was mutated from OnRender.
            var host = new Border { Width = 400 };
            var progress = new Progress { Percent = 30 };
            host.Child = progress;
            host.Measure(new Size(400, 100));
            host.Arrange(new Rect(0, 0, 400, 100));
            Check("progress fits its container", progress.ActualWidth <= 400.01,
                progress.ActualWidth.ToString("0.#"));
            // Every bordered field must share one metric source; they drifted once because two
            // templates read their radius from a markup extension and one from a DP.
            var fields = new Control[]
            {
                new AntdInput(),
                new InputNumber(),
                new Select()
            };
            bool sameHeight = fields.All(f => Math.Abs(f.MinHeight - t.ControlHeight) < 0.01);
            bool sameRadius = new[] { (fields[0] as AntdInput).CornerRadius.TopLeft,
                                      (fields[1] as InputNumber).CornerRadius.TopLeft,
                                      (fields[2] as Select).CornerRadius.TopLeft }
                                  .All(r => Math.Abs(r - t.BorderRadius) < 0.01);
            var antdInput = new AntdInput { Width = 220, Placeholder = "x" };
            Check("input padding is 4 by 11",
                Math.Abs(antdInput.Padding.Left - 11) < 0.01
                && Math.Abs(antdInput.Padding.Top - 4) < 0.01
                && Math.Abs(antdInput.Padding.Right - 11) < 0.01
                && Math.Abs(antdInput.Padding.Bottom - 4) < 0.01,
                antdInput.Padding.ToString());

            // The text area must be controlHeight - 2*line - 2*padding = 22, like antd.
            var inputHost = new Window
            {
                Width = 300, Height = 120, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { antdInput } }
            };
            inputHost.Show();
            DoEvents();
            inputHost.UpdateLayout();
            var contentHost = antdInput.Template.FindName("PART_ContentHost", antdInput) as FrameworkElement;
            Check("input text area is 22 tall",
                contentHost != null && Math.Abs(contentHost.ActualHeight - 22) < 0.51,
                contentHost == null ? "missing" : contentHost.ActualHeight.ToString("0.#"));
            inputHost.Close();

            var toggleProbe = new Antd.Wpf.Controls.Switch();
            Check("switch track corner is half the height",
                Math.Abs(toggleProbe.TrackCornerRadius.TopLeft - toggleProbe.TrackHeight / 2) < 0.01,
                toggleProbe.TrackCornerRadius.TopLeft + " of " + toggleProbe.TrackHeight);

            Check("input family shares height and radius", sameHeight && sameRadius,
                string.Join("/", fields.Select(f => f.MinHeight.ToString("0.#"))));

            var clearable = new AntdInput { Text = "x", AllowClear = true, Width = 160 };
            var clearHost = new Window
            {
                Width = 240, Height = 120, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new Border { Child = clearable }
            };
            clearHost.Show();
            DoEvents();
            clearHost.UpdateLayout();
            var clearPart = clearable.Template.FindName("PART_ClearButton", clearable) as FrameworkElement;
            Check("clear icon is a square hit box",
                clearPart != null && Math.Abs(clearPart.ActualWidth - clearPart.ActualHeight) < 0.01,
                clearPart == null ? "missing" : clearPart.ActualWidth + "x" + clearPart.ActualHeight);
            clearHost.Close();

            Check("progress reserves room for the label",
                Math.Abs(progress.FontSize - t.FontSize) < 0.01, progress.FontSize.ToString());
        }

        // ---------------------------------------------------------------- M2 additions

        private static void CheckAutoCompleteAndCalendar()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;

            var complete = new AntdAutoComplete
            {
                ItemsSource = new[] { "WPF", "WinUI 3", "Avalonia", "MAUI" },
                Text = "w"
            };
            var host = new Window
            {
                Width = 400, Height = 200, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new Border { Child = complete }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            var list = complete.Template.FindName("PART_Suggestions", complete) as System.Windows.Controls.ListBox;
            Check("autocomplete filters on text",
                list != null && list.Items.Count == 2, list == null ? "no list" : list.Items.Count.ToString());
            var editor = complete.Template.FindName("PART_EditableTextBox", complete) as System.Windows.Controls.TextBox;
            if (editor != null) { editor.Focus(); DoEvents(); }
            Check("autocomplete opens on focus with matches",
                complete.IsDropDownOpen && complete.HasSuggestions, complete.IsDropDownOpen.ToString());

            complete.Text = "zzz";
            DoEvents();
            Check("autocomplete closes when nothing matches",
                !complete.IsDropDownOpen && !complete.HasSuggestions, complete.IsDropDownOpen.ToString());

            complete.Text = string.Empty;
            DoEvents();
            Check("empty text offers every option",
                list != null && list.Items.Count == 4, list == null ? "no list" : list.Items.Count.ToString());
            host.Close();

            // December 2026 starts on a Tuesday, so the grid leads with November days.
            var calendar = new AntdCalendar { DisplayDate = new DateTime(2026, 12, 15) };
            var calHost = new Window
            {
                Width = 400, Height = 400, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new Border { Child = calendar }
            };
            calHost.Show();
            DoEvents();
            calHost.UpdateLayout();

            Check("calendar renders six weeks", calendar.Days.Count == 42, calendar.Days.Count.ToString());
            Check("calendar starts on the first week",
                calendar.Days[0].Date.DayOfWeek == DayOfWeek.Sunday,
                calendar.Days[0].Date.DayOfWeek.ToString());
            Check("calendar marks leading days as outside",
                calendar.Days[0].IsOutsideMonth, calendar.Days[0].Date.ToString("d"));

            calendar.SelectedDate = new DateTime(2026, 12, 15);
            DoEvents();
            var grid = calendar.Template.FindName("PART_DayGrid", calendar) as System.Windows.Controls.ListBox;
            var picked = grid == null ? null : grid.SelectedItem as CalendarDay;
            Check("calendar selects the requested day",
                picked != null && picked.Date == new DateTime(2026, 12, 15),
                picked == null ? "no selection" : picked.Key);

            calendar.DisplayDate = new DateTime(2027, 1, 1);
            DoEvents();
            Check("calendar rebuilds on month change",
                calendar.Days[15].IsOutsideMonth == false && calendar.Days[15].Date.Month == 1,
                calendar.Days[15].Date.ToString("d"));

            Check("calendar label localised",
                !string.IsNullOrEmpty(calendar.MonthLabel), calendar.MonthLabel);
            calHost.Close();
        }

        // ---------------------------------------------------------------- date picker

        private static void CheckDatePicker()
        {
            var picker = new AntdDatePicker { Format = "yyyy-MM-dd", SelectedDate = new DateTime(2026, 3, 14) };
            var host = new Window
            {
                Width = 420, Height = 300, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { picker } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("date picker formats the selected date", picker.DisplayText == "2026-03-14", picker.DisplayText);
            Check("date picker hides placeholder when set", !picker.ShowPlaceholder, picker.ShowPlaceholder.ToString());
            Check("date picker offers clear when set", picker.CanClear, picker.CanClear.ToString());

            var calendar = picker.Template.FindName("PART_Calendar", picker) as AntdCalendar;
            var popup = picker.Template.FindName("PART_Popup", picker) as System.Windows.Controls.Primitives.Popup;
            Check("date picker panel is the calendar control", calendar != null, calendar == null ? "missing" : "ok");
            Check("date picker forwards its value to the panel",
                calendar != null && calendar.SelectedDate == new DateTime(2026, 3, 14),
                calendar == null ? "no calendar" : calendar.SelectedDate.HasValue.ToString());

            picker.IsDropDownOpen = true;
            DoEvents();
            Check("date picker opens its popup", popup != null && popup.IsOpen, popup == null ? "missing" : popup.IsOpen.ToString());

            // Picking a day in the panel closes the popup and updates the field.
            if (calendar != null)
            {
                var grid = calendar.Template.FindName("PART_DayGrid", calendar) as System.Windows.Controls.ListBox;
                var day = grid == null ? null : grid.Items[10] as Antd.Wpf.Controls.CalendarDay;
                if (grid != null) grid.SelectedItem = day;
                DoEvents();
            }
            Check("picking a day closes the popup", !picker.IsDropDownOpen, picker.IsDropDownOpen.ToString());
            Check("picking a day updates the field",
                picker.SelectedDate.HasValue && picker.DisplayText == picker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                picker.DisplayText);

            picker.SelectedDate = null;
            DoEvents();
            Check("clearing restores the placeholder",
                picker.ShowPlaceholder && picker.DisplayText.Length == 0 && !picker.CanClear,
                picker.DisplayText);

            picker.Format = "MMM d, yyyy";
            picker.SelectedDate = new DateTime(2026, 3, 14);
            DoEvents();
            Check("format applies to the field",
                picker.DisplayText == new DateTime(2026, 3, 14).ToString("MMM d, yyyy", System.Globalization.CultureInfo.CurrentCulture),
                picker.DisplayText);
            host.Close();
        }

        // ---------------------------------------------------------------- M1 tail

        private static void CheckM1Tail()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;

            var splitter = new Splitter { ResizeDirection = GridResizeDirection.Columns };
            Check("splitter bar uses colorSplit",
                SameBrush(splitter.BarBrush, ThemeManager.Current.Snapshot.Brush("colorSplit")),
                Describe(splitter.BarBrush));
            Check("splitter bar is 1px", Math.Abs(splitter.BarThickness - 1) < 0.01,
                splitter.BarThickness.ToString());

            var beam = new BorderBeam { CornerRadius = new CornerRadius(10) };
            var beamHost = new Window
            {
                Width = 260, Height = 140, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new Grid { Children = { beam } }
            };
            beamHost.Show();
            DoEvents();
            beamHost.UpdateLayout();
            Check("border beam paints a base border",
                SameBrush(beam.BaseBrush, ThemeManager.Current.Snapshot.Brush("colorBorderSecondary")),
                Describe(beam.BaseBrush));
            beamHost.Close();

            // Affix must pin itself once the scroller passes OffsetTop.
            var affixTarget = new Border { Height = 40, Background = Brushes.Transparent };
            var affix = new Affix { Content = affixTarget, OffsetTop = 0 };
            var scroller = new ScrollViewer
            {
                Height = 120,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = new StackPanel { Children = { affix, new Border { Height = 400 } } }
            };
            var affixHost = new Window
            {
                Width = 320, Height = 200, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { scroller } }
            };
            affixHost.Show();
            DoEvents();
            affixHost.UpdateLayout();

            Check("affix is not pinned at rest", !affix.IsPinned, affix.IsPinned.ToString());

            scroller.ScrollToVerticalOffset(60);
            DoEvents();
            var translate = affix.RenderTransform as TranslateTransform;
            Check("affix pins after scrolling past the offset",
                affix.IsPinned && translate != null && translate.Y > 1,
                translate == null ? "no transform" : translate.Y.ToString("0.#"));

            affixHost.Close();
        }

        // ---------------------------------------------------------------- tree

        private static void CheckTree()
        {
            var root = new TreeItem("root");
            var a = new TreeItem("a");
            var b = new TreeItem("b");
            a.Children.Add(new TreeItem("a1"));
            a.Children.Add(new TreeItem("a2"));
            root.Children.Add(a);
            root.Children.Add(b);

            Check("tree sets parent links", a.Parent == root && a.Children[0].Parent == a, "parent missing");

            root.IsChecked = true;
            Check("checking a parent checks descendants",
                a.IsChecked == true && a.Children[0].IsChecked == true && b.IsChecked == true,
                a.IsChecked + "/" + a.Children[0].IsChecked);

            root.IsChecked = false;
            Check("unchecking a parent clears descendants",
                a.IsChecked == false && a.Children[1].IsChecked == false, a.IsChecked.ToString());

            a.Children[0].IsChecked = true;
            Check("a single child makes the parent indeterminate",
                a.IsChecked == null && root.IsChecked == null, a.IsChecked + "/" + root.IsChecked);

            a.Children[1].IsChecked = true;
            Check("all children checked makes the parent checked",
                a.IsChecked == true && root.IsChecked == null, a.IsChecked.ToString());

            b.IsChecked = true;
            Check("ancestors roll up to fully checked",
                root.IsChecked == true, root.IsChecked.ToString());

            var tree = new Tree { ItemsSource = new[] { root } };
            var host = new Window
            {
                Width = 360, Height = 320, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { tree } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            int checkedCount = 0;
            foreach (TreeItem item in tree.GetCheckedItems()) checkedCount++;
            Check("tree lists checked nodes", checkedCount == 5, checkedCount.ToString());

            tree.ExpandAll(true);
            DoEvents();
            Check("expand all opens every parent", a.IsExpanded && root.IsExpanded, a.IsExpanded.ToString());

            tree.ExpandAll(false);
            DoEvents();
            Check("collapse all closes every parent", !a.IsExpanded && !root.IsExpanded, a.IsExpanded.ToString());

            var container = tree.ItemContainerGenerator.ContainerFromItem(root) as TreeViewItem;
            Check("tree applies the antd item container",
                container != null && container.Template != null, container == null ? "missing" : "ok");
            host.Close();
        }

        // ---------------------------------------------------------------- time picker / mentions

        private static void CheckTimePickerAndMentions()
        {
            var time = new TimePicker { SelectedTime = new TimeSpan(9, 5, 7) };
            var host = new Window
            {
                Width = 420, Height = 260, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { time } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("time picker formats the value", time.DisplayText == "09:05:07", time.DisplayText);

            time.ShowSeconds = false;
            DoEvents();
            Check("time picker hides seconds", time.DisplayText == "09:05", time.DisplayText);

            var hours = time.Template.FindName("PART_Hours", time) as System.Windows.Controls.ListBox;
            var minutes = time.Template.FindName("PART_Minutes", time) as System.Windows.Controls.ListBox;
            Check("time picker seeds the hour column",
                hours != null && hours.Items.Count == 24, hours == null ? "missing" : hours.Items.Count.ToString());

            time.ShowSeconds = true;
            DoEvents();
            if (hours != null) hours.SelectedIndex = 14;
            if (minutes != null) minutes.SelectedIndex = 45;
            DoEvents();
            Check("picking a column updates the value",
                time.SelectedTime.HasValue && time.SelectedTime.Value.Hours == 14 && time.SelectedTime.Value.Minutes == 45,
                time.SelectedTime.ToString());

            time.SelectedTime = null;
            DoEvents();
            Check("clearing a time restores the placeholder", time.ShowPlaceholder && !time.CanClear, time.DisplayText);
            host.Close();

            var mentions = new Mentions
            {
                ItemsSource = new[] { "Alice Chen", "Bob Li", "Carol Wang" }
            };
            var mHost = new Window
            {
                Width = 420, Height = 200, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { mentions } }
            };
            mHost.Show();
            DoEvents();
            mHost.UpdateLayout();

            var editor = mentions.Template.FindName("PART_Editor", mentions) as System.Windows.Controls.TextBox;
            var list = mentions.Template.FindName("PART_Suggestions", mentions) as System.Windows.Controls.ListBox;
            Check("mentions has an editor", editor != null, editor == null ? "missing" : "ok");

            if (editor != null)
            {
                editor.Text = "hi @ali";
                editor.CaretIndex = editor.Text.Length;
                editor.Focus();
                DoEvents();
            }
            var items = list == null ? null : list.Items;
            Check("mentions filters after the trigger",
                items != null && items.Count == 1, items == null ? "no list" : items.Count.ToString());

            if (editor != null)
            {
                editor.Text = "hi alice";
                editor.CaretIndex = editor.Text.Length;
                DoEvents();
            }
            Check("no trigger means no suggestions",
                !mentions.IsDropDownOpen, mentions.IsDropDownOpen.ToString());
            mHost.Close();
        }

        // ---------------------------------------------------------------- theme switch

        private static void CheckThemeSwitch()
        {
            ThemeSnapshot before = ThemeManager.Current.Snapshot;

            ThemeManager.Current.ApplyDark();
            DoEvents();
            if (_testHost != null) _testHost.UpdateLayout();

            ThemeSnapshot after = ThemeManager.Current.Snapshot;
            Check("dark snapshot installed", after.IsDark, "still light");
            Check("dark background is not the light one",
                !SameBrush(after.Brush("colorBgContainer"), before.Brush("colorBgContainer")),
                "background did not change");
            Check("dark colorBgContainer #141414",
                after.Tokens.ColorBgContainer == AntdColor.Parse("#141414"),
                after.Tokens.ColorBgContainer.ToString());
            Check("dark colorText alpha 0.85",
                Math.Abs(after.Tokens.ColorText.Alpha - 0.85) < 1e-9,
                after.Tokens.ColorText.Alpha.ToString("R"));
            Check("control follows theme switch",
                SameBrush(_testButton.Background, after.Brush("colorPrimary")),
                Describe(_testButton.Background));
            Check("app resources swapped",
                SameBrush((Brush)Application.Current.Resources["Antd.colorBgContainer"], after.Brush("colorBgContainer")),
                "DynamicResource host not updated");

            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
            DoEvents();
            if (_testHost != null) _testHost.UpdateLayout();

            Check("back to light",
                ThemeManager.Current.Snapshot.IsDark == false
                && SameBrush(_testButton.Background, ThemeManager.Current.Snapshot.Brush("colorPrimary")),
                Describe(_testButton.Background));

            if (_testHost != null)
            {
                _testHost.Close();
                _testHost = null;
            }
        }

        // ---------------------------------------------------------------- screenshots

        private static void CaptureGallery(string directory)
        {
            Directory.CreateDirectory(directory);

            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
            var window = new MainWindow { Width = 1100, Height = 2200, WindowStartupLocation = WindowStartupLocation.Manual, Left = -6000, Top = 0 };
            window.Show();
            DoEvents();
            CaptureCard(FindDescendant<Progress>(window), Path.Combine(directory, "zoom-progress.png"));
            CaptureCard(FindDescendant<Badge>(window), Path.Combine(directory, "zoom-display.png"));
            CaptureCard(FindDescendant<FormItem>(window), Path.Combine(directory, "zoom-form.png"));
            var inputPreview = FindDescendant<AntdInput>(window);
            if (inputPreview != null) RenderElement(inputPreview, Path.Combine(directory, "zoom-input-single.png"), 3.0);
            var treePreview = FindDescendant<Antd.Wpf.Controls.Tree>(window);
            if (treePreview != null) RenderElement(treePreview, Path.Combine(directory, "zoom-tree.png"), 2.0);
            CaptureDatePicker(window, directory);
            var switchPreview = FindDescendant<Antd.Wpf.Controls.Switch>(window);
            if (switchPreview != null) RenderElement(switchPreview, Path.Combine(directory, "zoom-switch.png"), 6.0);
            var layoutPreview = FindDescendant<Antd.Wpf.Controls.Layout>(window);
            if (layoutPreview != null) RenderElement(layoutPreview, Path.Combine(directory, "zoom-layout.png"), 1.0);
            Render(window, Path.Combine(directory, "gallery-light.png"));
            Console.WriteLine("captured: gallery-light.png");

            var inputCard = window.FindName("InputCard") as FrameworkElement;
            if (inputCard != null)
            {
                RenderElement(inputCard, Path.Combine(directory, "zoom-inputs.png"), 2.0);
                Console.WriteLine("captured: zoom-inputs.png");
            }

            ThemeManager.Current.ApplyDark();
            DoEvents();
            window.UpdateLayout();
            Render(window, Path.Combine(directory, "gallery-dark.png"));
            Console.WriteLine("captured: gallery-dark.png");

            window.Close();
            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
        }

        private static void RenderElement(FrameworkElement element, string path, double scale)
        {
            element.UpdateLayout();
            double dpiScale = VisualTreeHelper.GetDpi(element).DpiScaleX;
            if (dpiScale <= 0) dpiScale = 1.0;
            scale = scale * dpiScale;

            int width = (int)Math.Ceiling(element.ActualWidth * scale);
            int height = (int)Math.Ceiling(element.ActualHeight * scale);
            if (width <= 0 || height <= 0) return;

            // A dpi-scaled RenderTargetBitmap does not zoom a visual; paint it through a
            // VisualBrush under a ScaleTransform instead.
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.PushTransform(new ScaleTransform(scale, scale));
                context.DrawRectangle(new VisualBrush(element), null,
                    new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                context.Pop();
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = File.Create(path))
            {
                encoder.Save(stream);
            }
        }

        private static void CaptureDatePicker(Window window, string directory)
        {
            var picker = FindDescendant<AntdDatePicker>(window);
            if (picker == null) return;

            RenderElement(picker, Path.Combine(directory, "zoom-datepicker-field.png"), 2.0);

            picker.IsDropDownOpen = true;
            DoEvents();
            DoEvents();

            var popup = picker.Template.FindName("PART_Popup", picker) as System.Windows.Controls.Primitives.Popup;
            var child = popup == null ? null : popup.Child as FrameworkElement;
            if (child != null)
            {
                child.UpdateLayout();
                DoEvents();
                RenderElement(child, Path.Combine(directory, "zoom-datepicker-panel.png"), 2.0);
                Console.WriteLine("captured: zoom-datepicker-panel.png");
            }

            picker.IsDropDownOpen = false;
            DoEvents();
        }

        private static void CaptureCard(DependencyObject anchor, string path)
        {
            if (anchor == null) return;

            DependencyObject node = System.Windows.Media.VisualTreeHelper.GetParent(anchor);
            FrameworkElement card = null;
            while (node != null)
            {
                var border = node as Border;
                if (border != null && border.ActualWidth > 200)
                {
                    card = border;
                    break;
                }
                node = System.Windows.Media.VisualTreeHelper.GetParent(node);
            }

            if (card != null)
            {
                RenderElement(card, path, 2.0);
                Console.WriteLine("captured: " + Path.GetFileName(path));
            }
        }

        private static T FindDescendant<T>(DependencyObject root) where T : DependencyObject
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                var match = child as T;
                if (match != null) return match;
                T nested = FindDescendant<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }

        private static void Render(Window window, string path)
        {
            window.UpdateLayout();
            var root = window.Content as FrameworkElement;
            if (root == null) return;

            // A ScrollViewer clips to the viewport; render its content instead so the whole
            // page is captured rather than the first screenful.
            var scroller = root as ScrollViewer;
            if (scroller != null && scroller.Content is FrameworkElement inner)
            {
                root = inner;
            }

            root.UpdateLayout();

            // Layout happens in device independent units at the monitor's DPI. Rendering a
            // 120 DPI layout into a 96 DPI bitmap squeezes every hairline (a 1px divider lands
            // on 0.8px and washes out), so capture at the real scale.
            double dpiScale = VisualTreeHelper.GetDpi(root).DpiScaleX;
            if (dpiScale <= 0) dpiScale = 1.0;

            int width = (int)Math.Ceiling(root.ActualWidth * dpiScale);
            int height = (int)Math.Ceiling(root.ActualHeight * dpiScale);
            if (width <= 0 || height <= 0) return;

            // The window itself is not part of Content, so composite its background first;
            // otherwise dark theme captures come out on a transparent (white looking) page.
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.PushTransform(new ScaleTransform(dpiScale, dpiScale));
                context.DrawRectangle(window.Background ?? Brushes.White, null,
                    new Rect(0, 0, root.ActualWidth, root.ActualHeight));
                // Viewbox must be absolute, otherwise VisualBrush crops to the content
                // bounds and the capture loses the page margins.
                var brush = new VisualBrush(root)
                {
                    Stretch = Stretch.Fill,
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(0, 0, root.ActualWidth, root.ActualHeight),
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top
                };
                context.DrawRectangle(brush, null, new Rect(0, 0, root.ActualWidth, root.ActualHeight));
                context.Pop();
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = File.Create(path))
            {
                encoder.Save(stream);
            }
        }

        // ---------------------------------------------------------------- helpers

        private static bool SameBrush(Brush actual, Brush expected)
        {
            var a = actual as SolidColorBrush;
            var b = expected as SolidColorBrush;
            if (a == null || b == null) return ReferenceEquals(actual, expected);
            return a.Color == b.Color;
        }

        private static string Describe(Brush brush)
        {
            var solid = brush as SolidColorBrush;
            return solid == null ? "null" : solid.Color.ToString();
        }

        private static void Check(string name, bool condition, string detail)
        {
            _checks++;
            if (!condition)
            {
                Failures.Add(name + " (actual: " + detail + ")");
                Console.WriteLine("  x " + name);
            }
            else
            {
                Console.WriteLine("  . " + name);
            }
        }

        private static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(delegate { frame.Continue = false; }));
            Dispatcher.PushFrame(frame);
        }
    }
}
