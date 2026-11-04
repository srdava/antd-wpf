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
using Antd.Wpf.Controls.Internal;
using AntdButton = Antd.Wpf.Controls.Button;
using AntdInput = Antd.Wpf.Controls.Input;
using AntdCalendar = Antd.Wpf.Controls.Calendar;
using AntdAutoComplete = Antd.Wpf.Controls.AutoComplete;
using AntdDatePicker = Antd.Wpf.Controls.DatePicker;
using AntdList = Antd.Wpf.Controls.List;
using AntdMenu = Antd.Wpf.Controls.Menu;
using AntdImage = Antd.Wpf.Controls.Image;
using AntdCard = Antd.Wpf.Controls.Card;
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
            string qrPath = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--shot" && i + 1 < args.Length) shotDir = args[i + 1];
                if (args[i] == "--metrics" && i + 1 < args.Length) metricsPath = args[i + 1];
                if (args[i] == "--qr-dump" && i + 1 < args.Length) qrPath = args[i + 1];
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

            if (qrPath != null)
            {
                DumpQr(qrPath);
                return 0;
            }

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
            CheckUploadAndTransfer();
            CheckColorPicker();
            CheckTable();
            CheckTreeSelectAndCascader();
            CheckDisplayControls();
            CheckOverlays();
            CheckDisplayControls2();
            CheckQrEncoder();
            CheckNavigationAndMedia();

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

            // A WPF Popup freezes at the screen position it opened at, so a dropdown inside a
            // scrolling page drifts away from its field. The panel has to follow it.
            var scrollPicker = new TimePicker { SelectedTime = new TimeSpan(9, 0, 0) };
            var scroller = new ScrollViewer
            {
                Height = 200,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                Content = new StackPanel { Children = { scrollPicker, new Border { Height = 600 } } }
            };
            var scrollHost = new Window
            {
                Width = 400, Height = 260, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = scroller
            };
            scrollHost.Show();
            DoEvents();
            scrollHost.UpdateLayout();

            scrollPicker.IsDropDownOpen = true;
            DoEvents();
            DoEvents();

            var anchoredPopup = scrollPicker.Template.FindName("PART_Popup", scrollPicker)
                as System.Windows.Controls.Primitives.Popup;
            var panel = anchoredPopup == null ? null : anchoredPopup.Child as FrameworkElement;

            Check("time picker panel is anchored to the field",
                anchoredPopup != null && anchoredPopup.PlacementTarget == scrollPicker,
                anchoredPopup == null ? "no popup" : (anchoredPopup.PlacementTarget == null ? "null" : anchoredPopup.PlacementTarget.GetType().Name));

            if (panel != null)
            {
                Point fieldStart = scrollPicker.PointToScreen(new Point(0, 0));
                Point panelStart = panel.PointToScreen(new Point(0, 0));

                scroller.ScrollToVerticalOffset(60);
                DoEvents();
                DoEvents();

                Point fieldScrolled = scrollPicker.PointToScreen(new Point(0, 0));
                Point panelScrolled = panel.PointToScreen(new Point(0, 0));

                double fieldDelta = fieldScrolled.Y - fieldStart.Y;
                double panelDelta = panelScrolled.Y - panelStart.Y;

                Check("time picker panel moves with the field",
                    Math.Abs(fieldDelta) > 1 && Math.Abs(panelDelta - fieldDelta) < 1.5,
                    "field " + fieldDelta.ToString("0.#") + " panel " + panelDelta.ToString("0.#"));
                Check("time picker panel keeps its gap",
                    Math.Abs((panelScrolled.Y - fieldScrolled.Y) - (panelStart.Y - fieldStart.Y)) < 1.5,
                    (panelScrolled.Y - fieldScrolled.Y).ToString("0.#"));
            }
            scrollHost.Close();

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

        // ---------------------------------------------------------------- upload / transfer

        private static void CheckUploadAndTransfer()
        {
            var upload = new Upload();
            Check("upload starts empty", !upload.HasFiles && upload.Files.Count == 0, upload.Files.Count.ToString());

            upload.Files.Add(new Antd.Wpf.Controls.UploadFile { Name = "a.png", Size = 2048 });
            Check("upload reports files", upload.HasFiles, upload.HasFiles.ToString());
            Check("upload formats the size", upload.Files[0].SizeText == "2 KB", upload.Files[0].SizeText);

            upload.RemoveFile(upload.Files[0]);
            Check("upload removes a file", upload.Files.Count == 0 && !upload.HasFiles, upload.Files.Count.ToString());

            var transfer = new Transfer
            {
                ItemsSource = new[]
                {
                    new TransferItem("1", "Alpha"),
                    new TransferItem("2", "Beta"),
                    new TransferItem("3", "Gamma")
                }
            };
            var host = new Window
            {
                Width = 620, Height = 320, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { transfer } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("transfer splits the source list", transfer.SourceItems.Count == 3 && transfer.TargetItems.Count == 0,
                transfer.SourceItems.Count + "/" + transfer.TargetItems.Count);

            bool changed = false;
            transfer.Changed += delegate { changed = true; };
            transfer.MoveToTarget(new[] { transfer.SourceItems[0] });

            Check("transfer moves an item to the target",
                transfer.SourceItems.Count == 2 && transfer.TargetItems.Count == 1 && changed,
                transfer.SourceItems.Count + "/" + transfer.TargetItems.Count);
            Check("transfer updates the counters",
                transfer.SourceCount == "2 items" && transfer.TargetCount == "1 item",
                transfer.SourceCount + " | " + transfer.TargetCount);

            transfer.MoveToSource(new[] { transfer.TargetItems[0] });
            Check("transfer moves an item back",
                transfer.SourceItems.Count == 3 && transfer.TargetItems.Count == 0,
                transfer.SourceItems.Count.ToString());
            host.Close();
        }

        // ---------------------------------------------------------------- colour picker

        private static void CheckColorPicker()
        {
            var picker = new ColorPicker();
            var host = new Window
            {
                Width = 360, Height = 260, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { picker } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("colour picker starts empty", picker.HexText.Length == 0 && !picker.CanClear, picker.HexText);

            picker.SelectedColor = Color.FromRgb(0xFF, 0x00, 0x00);
            DoEvents();
            Check("colour picker prints hex", picker.HexText == "#FF0000", picker.HexText);

            var swatch = picker.SwatchBrush as SolidColorBrush;
            Check("colour picker shows the swatch",
                swatch != null && swatch.Color.R == 0xFF && swatch.Color.G == 0, picker.SwatchBrush.ToString());
            Check("colour picker offers clear", picker.CanClear, picker.CanClear.ToString());

            var hueBrush = picker.HueBrush as SolidColorBrush;
            Check("hue brush follows the selection",
                hueBrush != null && hueBrush.Color.R > 200 && hueBrush.Color.G < 60,
                picker.HueBrush == null ? "null" : hueBrush.Color.ToString());

            var square = picker.Template.FindName("PART_Saturation", picker) as FrameworkElement;
            var popup = picker.Template.FindName("PART_Popup", picker) as System.Windows.Controls.Primitives.Popup;
            Check("colour picker exposes its square", square != null && popup != null,
                square == null ? "missing" : "ok");

            picker.IsDropDownOpen = true;
            DoEvents();
            Check("colour picker opens its panel", popup != null && popup.IsOpen,
                popup == null ? "missing" : popup.IsOpen.ToString());

            picker.SelectedColor = null;
            DoEvents();
            Check("clearing a colour empties the field", picker.HexText.Length == 0, picker.HexText);
            host.Close();
        }

        // ---------------------------------------------------------------- table

        private static void CheckTable()
        {
            var table = new Table();
            table.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            table.Columns.Add(new DataGridTextColumn { Header = "Role", Binding = new System.Windows.Data.Binding("Role"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            var host = new Window
            {
                Width = 600, Height = 320, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { table } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("table starts empty", !table.HasRows, table.HasRows.ToString());

            table.ItemsSource = new[]
            {
                new { Name = "Button", Role = "Atomic" },
                new { Name = "Select", Role = "Data entry" }
            };
            DoEvents();
            host.UpdateLayout();

            Check("table reports rows", table.HasRows && table.Items.Count == 2, table.Items.Count.ToString());
            Check("table keeps its columns", table.Columns.Count == 2, table.Columns.Count.ToString());

            table.SelectedIndex = 1;
            DoEvents();
            Check("table selects a row", table.SelectedIndex == 1, table.SelectedIndex.ToString());

            var row = table.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow;
            Check("table applies the antd row style",
                row != null && Math.Abs(row.MinHeight - 48) < 0.01,
                row == null ? "missing" : row.MinHeight.ToString());

            // Sorting through the view keeps working because the control is a DataGrid.
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(table.ItemsSource);
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Name",
                System.ComponentModel.ListSortDirection.Descending));
            DoEvents();
            Check("table rows follow the view", table.Items.Count == 2, table.Items.Count.ToString());
            host.Close();
        }

        // ---------------------------------------------------------------- tree select / cascader

        private static void CheckTreeSelectAndCascader()
        {
            var parent = new TreeItem("Components");
            parent.Children.Add(new TreeItem("Button"));
            parent.Children.Add(new TreeItem("Input"));

            var treeSelect = new TreeSelect { ItemsSource = new[] { parent } };
            var host = new Window
            {
                Width = 360, Height = 300, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { treeSelect } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("tree select starts with the placeholder",
                treeSelect.ShowPlaceholder && !treeSelect.CanClear, treeSelect.DisplayText);

            treeSelect.SelectedItem = parent.Children[1];
            DoEvents();
            Check("tree select shows the picked node",
                treeSelect.DisplayText == "Input" && treeSelect.CanClear, treeSelect.DisplayText);

            var innerTree = treeSelect.Template.FindName("PART_Tree", treeSelect) as Tree;
            Check("tree select hosts a tree", innerTree != null && innerTree.Items.Count == 1,
                innerTree == null ? "missing" : innerTree.Items.Count.ToString());
            host.Close();

            var root = new CascaderNode("Zhejiang");
            var hangzhou = new CascaderNode("Hangzhou");
            hangzhou.Children.Add(new CascaderNode("Xihu"));
            hangzhou.Children.Add(new CascaderNode("Binjiang"));
            root.Children.Add(hangzhou);

            var cascader = new Cascader { ItemsSource = new[] { root } };
            var cHost = new Window
            {
                Width = 420, Height = 320, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { cascader } }
            };
            cHost.Show();
            DoEvents();
            cHost.UpdateLayout();

            cascader.IsDropDownOpen = true;
            DoEvents();
            var columns = cascader.Template.FindName("PART_Columns", cascader) as System.Windows.Controls.StackPanel;
            Check("cascader opens with one column", columns != null && columns.Children.Count == 1,
                columns == null ? "missing" : columns.Children.Count.ToString());

            var first = columns.Children[0] as System.Windows.Controls.ListBox;
            if (first != null) first.SelectedIndex = 0;
            DoEvents();
            Check("picking a parent opens the next column",
                columns.Children.Count == 2 && cascader.DisplayText == "Zhejiang",
                columns.Children.Count + " | " + cascader.DisplayText);

            var second = columns.Children[1] as System.Windows.Controls.ListBox;
            if (second != null) second.SelectedIndex = 0;
            DoEvents();
            var third = columns.Children.Count > 2 ? columns.Children[2] as System.Windows.Controls.ListBox : null;
            if (third != null) third.SelectedIndex = 1;
            DoEvents();

            Check("cascader joins the path",
                cascader.DisplayText == "Zhejiang / Hangzhou / Binjiang", cascader.DisplayText);
            Check("picking a leaf closes the panel", !cascader.IsDropDownOpen, cascader.IsDropDownOpen.ToString());
            cHost.Close();
        }

        // ---------------------------------------------------------------- display layer

        private static void CheckDisplayControls()
        {
            var tabs = new Tabs { Width = 520 };
            tabs.Items.Add(new TabsItem { Header = "Overview", Content = "one" });
            tabs.Items.Add(new TabsItem { Header = "Settings", Content = "two" });
            tabs.Items.Add(new TabsItem { Header = "History", Content = "three" });

            var host = new Window
            {
                Width = 600, Height = 260, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { tabs } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("tabs select the first item", tabs.SelectedIndex == 0, tabs.SelectedIndex.ToString());

            var firstTab = tabs.ItemContainerGenerator.ContainerFromIndex(0) as TabsItem;
            Check("tab label is 46 tall", firstTab != null && Math.Abs(firstTab.ActualHeight - 46) < 0.6,
                firstTab == null ? "missing" : firstTab.ActualHeight.ToString("0.#"));
            Check("tabs expose an antd template",
                firstTab != null && firstTab.Template != null, firstTab == null ? "no item" : "ok");

            var ink = firstTab == null ? null : firstTab.Template.FindName("Ink", firstTab) as FrameworkElement;
            Check("active tab shows its ink bar",
                ink != null && ink.Visibility == Visibility.Visible, ink == null ? "missing" : ink.Visibility.ToString());

            tabs.SelectedIndex = 1;
            DoEvents();
            Check("selecting a tab swaps the panel",
                tabs.SelectedIndex == 1 && (ink == null || ink.Visibility != Visibility.Visible),
                tabs.SelectedIndex.ToString());

            var card = new AntdCard { Title = "Card title", Content = "body", Width = 300 };
            var cardHost = new Window
            {
                Width = 420, Height = 240, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { card } }
            };
            cardHost.Show();
            DoEvents();
            cardHost.UpdateLayout();

            Check("card grows a head when titled", card.HasHeader, card.HasHeader.ToString());
            var head = card.Template.FindName("Head", card) as FrameworkElement;
            Check("card head is 56 tall", head != null && Math.Abs(head.ActualHeight - 56) < 0.6,
                head == null ? "missing" : head.ActualHeight.ToString("0.#"));
            Check("card radius is borderRadiusLG", Math.Abs(card.CornerRadius.TopLeft - 8) < 0.01,
                card.CornerRadius.TopLeft.ToString());

            card.Title = null;
            DoEvents();
            Check("card drops the head without a title", !card.HasHeader, card.HasHeader.ToString());
            cardHost.Close();

            var collapse = new Collapse { Width = 420 };
            var opened = new CollapseItem { Header = "First", Content = "body one", IsExpanded = true };
            var closed = new CollapseItem { Header = "Second", Content = "body two" };
            collapse.Items.Add(opened);
            collapse.Items.Add(closed);

            var collapseHost = new Window
            {
                Width = 520, Height = 320, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { collapse } }
            };
            collapseHost.Show();
            DoEvents();
            collapseHost.UpdateLayout();

            Check("collapse opens the panel marked expanded", opened.IsExpanded && !closed.IsExpanded,
                opened.IsExpanded + "/" + closed.IsExpanded);
            Check("collapse drops the divider on the last row", closed.IsLast && !opened.IsLast,
                closed.IsLast + "/" + opened.IsLast);

            var header = opened.Template.FindName("PART_Header", opened) as FrameworkElement;
            Check("collapse header hosts the click target", header != null, header == null ? "missing" : "ok");

            var body = opened.Template.FindName("Body", opened) as FrameworkElement;
            Check("expanded panel shows its body",
                body != null && body.Visibility == Visibility.Visible, body == null ? "missing" : body.Visibility.ToString());

            closed.IsExpanded = true;
            DoEvents();
            Check("panels expand independently by default", opened.IsExpanded && closed.IsExpanded,
                opened.IsExpanded + "/" + closed.IsExpanded);

            collapse.Accordion = true;
            closed.IsExpanded = false;
            closed.IsExpanded = true;
            DoEvents();
            Check("accordion closes the others", !opened.IsExpanded && closed.IsExpanded,
                opened.IsExpanded + "/" + closed.IsExpanded);
            collapseHost.Close();

            var list = new AntdList { Width = 420 };
            list.ItemsSource = new[] { "a", "b", "c" };
            var listHost = new Window
            {
                Width = 520, Height = 260, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { list } }
            };
            listHost.Show();
            DoEvents();
            listHost.UpdateLayout();

            Check("list renders every row", list.Items.Count == 3, list.Items.Count.ToString());
            var firstRow = list.ItemContainerGenerator.ContainerFromIndex(0) as ListItem;
            var lastRow = list.ItemContainerGenerator.ContainerFromIndex(2) as ListItem;
            Check("list marks the last row",
                firstRow != null && lastRow != null && !firstRow.IsLast && lastRow.IsLast,
                (firstRow == null ? "?" : firstRow.IsLast.ToString()) + "/" + (lastRow == null ? "?" : lastRow.IsLast.ToString()));
            listHost.Close();
        }

        // ---------------------------------------------------------------- overlays

        private static void CheckOverlays()
        {
            var tooltip = new Tooltip { Text = "Copied" };
            tooltip.Content = new Antd.Wpf.Controls.Button { Content = "Copy" };
            var host = new Window
            {
                Width = 420, Height = 200, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { tooltip } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            var tooltipPopup = tooltip.Template.FindName("PART_Popup", tooltip) as System.Windows.Controls.Primitives.Popup;
            Check("tooltip anchors to its trigger",
                tooltipPopup != null && tooltipPopup.PlacementTarget == tooltip,
                tooltipPopup == null ? "no popup" : "ok");
            Check("tooltip is closed until hovered", !tooltip.IsOpen, tooltip.IsOpen.ToString());

            tooltip.RaiseEvent(new System.Windows.Input.MouseEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice, System.Environment.TickCount)
            { RoutedEvent = UIElement.MouseEnterEvent });
            DoEvents();
            Check("tooltip opens on hover", tooltip.IsOpen && tooltipPopup != null && tooltipPopup.IsOpen,
                tooltip.IsOpen.ToString());

            tooltip.RaiseEvent(new System.Windows.Input.MouseEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice, System.Environment.TickCount)
            { RoutedEvent = UIElement.MouseLeaveEvent });
            DoEvents();
            Check("tooltip closes on leave", !tooltip.IsOpen, tooltip.IsOpen.ToString());

            Check("tooltip defaults to the top",
                tooltip.Placement == AntdPlacement.Top && tooltipPopup != null
                && tooltipPopup.Placement == System.Windows.Controls.Primitives.PlacementMode.Top,
                tooltip.Placement.ToString());
            host.Close();

            var popover = new Popover { Title = "Panel title", Body = "Body copy" };
            popover.Content = new Antd.Wpf.Controls.Button { Content = "Open" };
            var popoverHost = new Window
            {
                Width = 420, Height = 220, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { popover } }
            };
            popoverHost.Show();
            DoEvents();
            popoverHost.UpdateLayout();

            Check("popover shows a head only when titled", popover.HasTitle, popover.HasTitle.ToString());
            popover.Title = null;
            Check("popover drops the head without a title", !popover.HasTitle, popover.HasTitle.ToString());

            ((UIElement)popover.Content).RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice, System.Environment.TickCount,
                System.Windows.Input.MouseButton.Left)
            { RoutedEvent = UIElement.PreviewMouseUpEvent });
            DoEvents();
            Check("popover opens on click", popover.IsOpen, popover.IsOpen.ToString());
            popoverHost.Close();

            var dropdown = new Dropdown { ItemsSource = new[] { "Duplicate", "Delete" } };
            dropdown.Content = new Antd.Wpf.Controls.Button { Content = "Actions" };
            var menuHost = new Window
            {
                Width = 420, Height = 240, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { dropdown } }
            };
            menuHost.Show();
            DoEvents();
            menuHost.UpdateLayout();

            Check("dropdown defaults to click and bottom",
                dropdown.Trigger == AntdTrigger.Click && dropdown.Placement == AntdPlacement.Bottom,
                dropdown.Trigger + "/" + dropdown.Placement);

            dropdown.IsOpen = true;
            DoEvents();
            var menu = dropdown.Template.FindName("PART_Menu", dropdown) as System.Windows.Controls.ListBox;
            Check("dropdown renders its items", menu != null && menu.Items.Count == 2,
                menu == null ? "missing" : menu.Items.Count.ToString());
            Check("dropdown menu is open", dropdown.IsOpen, dropdown.IsOpen.ToString());

            object invoked = null;
            dropdown.ItemInvoked += delegate(object sender, RoutedPropertyChangedEventArgs<object> e)
            {
                invoked = e.NewValue;
            };
            if (menu != null) menu.SelectedIndex = 1;
            DoEvents();
            Check("picking a menu row invokes the item and closes",
                invoked as string == "Delete" && !dropdown.IsOpen,
                (invoked == null ? "none" : invoked.ToString()) + "/" + dropdown.IsOpen);

            Check("menu rows do not stay selected",
                menu != null && menu.SelectedIndex < 0, menu == null ? "?" : menu.SelectedIndex.ToString());
            menuHost.Close();
        }

        // ---------------------------------------------------------------- display layer, second batch

        private static void CheckDisplayControls2()
        {
            var crumb = new Breadcrumb();
            crumb.Items.Add(new BreadcrumbItem { Content = "Home" });
            crumb.Items.Add(new BreadcrumbItem { Content = "Components" });
            crumb.Items.Add(new BreadcrumbItem { Content = "Card" });

            var descriptions = new Descriptions { Column = 3, Width = 600 };
            descriptions.Items.Add(new DescriptionsItem { Label = "Product", Content = "Antd.Wpf" });
            descriptions.Items.Add(new DescriptionsItem { Label = "Theme", Content = "Default" });
            descriptions.Items.Add(new DescriptionsItem { Label = "Version", Content = "0.3" });
            descriptions.Items.Add(new DescriptionsItem { Label = "Notes", Content = "wide", Span = 3 });

            var timeline = new Timeline();
            timeline.Items.Add(new TimelineItem { Content = "one" });
            timeline.Items.Add(new TimelineItem { Content = "two" });

            var steps = new Steps { Current = 1, Width = 480 };
            steps.Items.Add(new StepsItem { Title = "Design" });
            steps.Items.Add(new StepsItem { Title = "Build" });
            steps.Items.Add(new StepsItem { Title = "Ship" });

            var masonry = new Masonry { Columns = 3, Gutter = 12, Width = 360 };
            for (int i = 0; i < 6; i++)
            {
                masonry.Items.Add(new Border { Height = 40 + i * 10 });
            }

            var host = new Window
            {
                Width = 700, Height = 900, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Children = { crumb, descriptions, timeline, steps, masonry }
                }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            var lastCrumb = crumb.ItemContainerGenerator.ContainerFromIndex(2) as BreadcrumbItem;
            var firstCrumb = crumb.ItemContainerGenerator.ContainerFromIndex(0) as BreadcrumbItem;
            Check("breadcrumb marks the current page",
                lastCrumb != null && firstCrumb != null && lastCrumb.IsLast && !firstCrumb.IsLast,
                (lastCrumb == null ? "?" : lastCrumb.IsLast.ToString()));

            var d0 = descriptions.ItemContainerGenerator.ContainerFromIndex(0) as DescriptionsItem;
            var d1 = descriptions.ItemContainerGenerator.ContainerFromIndex(1) as DescriptionsItem;
            var d3 = descriptions.ItemContainerGenerator.ContainerFromIndex(3) as DescriptionsItem;
            Point p0 = d0.TranslatePoint(new Point(0, 0), descriptions);
            Point p1 = d1.TranslatePoint(new Point(0, 0), descriptions);
            Point p3 = d3.TranslatePoint(new Point(0, 0), descriptions);
            Check("descriptions puts one pair per column",
                Math.Abs(p0.Y - p1.Y) < 0.5 && p1.X > p0.X + 100,
                p0.ToString() + " / " + p1.ToString());
            Check("descriptions wraps a full row",
                p3.Y > p0.Y && Math.Abs(p3.X) < 0.5,
                p3.ToString());

            var lastEvent = timeline.ItemContainerGenerator.ContainerFromIndex(1) as TimelineItem;
            Check("timeline drops the rail on the last row",
                lastEvent != null && lastEvent.IsLast, lastEvent == null ? "?" : lastEvent.IsLast.ToString());

            var s0 = steps.ItemContainerGenerator.ContainerFromIndex(0) as StepsItem;
            var s1 = steps.ItemContainerGenerator.ContainerFromIndex(1) as StepsItem;
            var s2 = steps.ItemContainerGenerator.ContainerFromIndex(2) as StepsItem;
            Check("steps derive status from Current",
                s0.Status == StepsStatus.Finish && s1.Status == StepsStatus.Process && s2.Status == StepsStatus.Wait,
                s0.Status + "/" + s1.Status + "/" + s2.Status);
            Check("steps number their icons",
                s0.Number == "1" && s2.Number == "3", s0.Number + "/" + s2.Number);
            Check("steps know their direction", !s0.IsVertical, s0.IsVertical.ToString());

            var m0 = masonry.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
            var m1 = masonry.ItemContainerGenerator.ContainerFromIndex(1) as FrameworkElement;
            var m2 = masonry.ItemContainerGenerator.ContainerFromIndex(2) as FrameworkElement;
            Point mp0 = m0.TranslatePoint(new Point(0, 0), masonry);
            Point mp1 = m1.TranslatePoint(new Point(0, 0), masonry);
            Point mp2 = m2.TranslatePoint(new Point(0, 0), masonry);
            Check("masonry spreads the first items across columns",
                mp0.X < mp1.X && mp1.X < mp2.X && Math.Abs(mp0.Y - mp1.Y) < 0.5,
                mp0.X + "/" + mp1.X + "/" + mp2.X);

            double tallestColumn = Math.Max(Math.Abs(mp0.X - mp1.X), Math.Abs(mp1.X - mp2.X));
            Check("masonry keeps columns the same width",
                Math.Abs((mp1.X - mp0.X) - (mp2.X - mp1.X)) < 0.5, tallestColumn.ToString("0.#"));

            host.Close();
        }

        // ---------------------------------------------------------------- navigation & media

        private static void CheckNavigationAndMedia()
        {
            var pager = new Pagination { Total = 240, PageSize = 10, Width = 520 };
            var host = new Window
            {
                Width = 620, Height = 90, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { pager } }
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();

            Check("pagination counts its pages", pager.PageCount == 24, pager.PageCount.ToString());

            var strip = pager.Template.FindName("PART_Host", pager) as Panel;
            Check("pagination renders arrows and numbers", strip != null && strip.Children.Count > 3,
                strip == null ? "missing" : strip.Children.Count.ToString());

            int previous = pager.Current;
            int raised = 0;
            pager.PageChanged += delegate(object sender, RoutedPropertyChangedEventArgs<int> e) { raised = e.NewValue; };

            var target = strip.Children[strip.Children.Count - 2] as Antd.Wpf.Controls.PaginationPageButton;
            Check("pagination exposes page buttons", target != null, strip.Children.Count.ToString());
            if (target != null) target.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            DoEvents();
            Check("pagination advances on click",
                pager.Current != previous && raised == pager.Current,
                pager.Current + "/" + raised);

            pager.Current = 99;
            DoEvents();
            Check("pagination clamps the page", pager.Current == pager.PageCount, pager.Current.ToString());
            host.Close();

            var carousel = new Carousel { Width = 320, Height = 120 };
            carousel.Items.Add("one");
            carousel.Items.Add("two");
            carousel.Items.Add("three");

            var carouselHost = new Window
            {
                Width = 400, Height = 220, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { carousel } }
            };
            carouselHost.Show();
            DoEvents();
            carouselHost.UpdateLayout();

            Check("carousel selects the first slide", (carousel.SelectedItem as string) == "one",
                carousel.SelectedItem == null ? "null" : carousel.SelectedItem.ToString());

            var dots = carousel.Template.FindName("PART_Dots", carousel) as Panel;
            Check("carousel builds a dot per slide", dots != null && dots.Children.Count == 3,
                dots == null ? "missing" : dots.Children.Count.ToString());

            carousel.SelectedIndex = 2;
            DoEvents();
            Check("carousel follows the index", (carousel.SelectedItem as string) == "three",
                carousel.SelectedItem == null ? "null" : carousel.SelectedItem.ToString());
            carouselHost.Close();

            var menu = new AntdMenu { Width = 200 };
            var design = new MenuNode { Header = "Design", IsExpanded = true };
            var colour = new MenuNode { Header = "Colour" };
            design.Items.Add(colour);
            design.Items.Add(new MenuNode { Header = "Type" });
            menu.Items.Add(design);
            menu.Items.Add(new MenuNode { Header = "About" });

            var menuHost = new Window
            {
                Width = 320, Height = 300, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { menu } }
            };
            menuHost.Show();
            DoEvents();
            menuHost.UpdateLayout();

            Check("menu exposes child nodes", design.HasChildren && design.Items.Count == 2,
                design.Items.Count.ToString());

            menu.Select(colour);
            DoEvents();
            Check("menu tracks the selected leaf",
                ReferenceEquals(menu.SelectedItem, colour) && colour.IsActive,
                menu.SelectedItem == null ? "null" : ((MenuNode)menu.SelectedItem).Header.ToString());
            menuHost.Close();

            var scroller = new ScrollViewer { Height = 100, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var top = new Border { Height = 60 };
            var bottom = new Border { Height = 400 };
            scroller.Content = new StackPanel { Children = { top, bottom } };

            var anchor = new Anchor { ScrollTarget = scroller };
            anchor.Items.Add(new AnchorLink { Content = "Top", Target = top });
            anchor.Items.Add(new AnchorLink { Content = "Bottom", Target = bottom });

            var anchorHost = new Window
            {
                Width = 360, Height = 220, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { anchor, scroller } }
            };
            anchorHost.Show();
            DoEvents();
            anchorHost.UpdateLayout();

            scroller.ScrollToVerticalOffset(80);
            DoEvents();
            Check("anchor highlights the section at the top", anchor.ActiveIndex == 1, anchor.ActiveIndex.ToString());
            anchorHost.Close();

            var image = new AntdImage { Width = 120, Height = 80, Preview = true };
            var imageHost = new Window
            {
                Width = 260, Height = 200, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { image } }
            };
            imageHost.Show();
            DoEvents();
            imageHost.UpdateLayout();

            image.IsPreviewVisible = true;
            DoEvents();
            var popup = image.Template.FindName("PART_Popup", image) as System.Windows.Controls.Primitives.Popup;
            Check("image opens its preview", popup != null && popup.IsOpen,
                popup == null ? "missing" : popup.IsOpen.ToString());
            image.IsPreviewVisible = false;
            DoEvents();
            Check("image closes its preview", popup == null || !popup.IsOpen,
                popup == null ? "missing" : popup.IsOpen.ToString());
            imageHost.Close();

            var tour = new Tour();
            var stepHost = new Border { Height = 40 };
            tour.Steps.Add(new TourStep { Target = stepHost, Title = "First", Description = "one" });
            tour.Steps.Add(new TourStep { Target = stepHost, Title = "Second", Description = "two" });

            var tourHost = new Window
            {
                Width = 400, Height = 300, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
                Content = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Children = { stepHost, tour } }
            };
            tourHost.Show();
            DoEvents();
            tourHost.UpdateLayout();

            tour.Open();
            DoEvents();
            Check("tour opens on the first step",
                tour.IsOpen && tour.Current == 0 && tour.CurrentTitle == "First", tour.CurrentTitle);

            tour.Next();
            DoEvents();
            Check("tour advances", tour.Current == 1 && tour.CurrentTitle == "Second", tour.CurrentTitle);

            bool finished = false;
            tour.Finished += delegate { finished = true; };
            tour.Next();
            DoEvents();
            Check("tour finishes on the last step", !tour.IsOpen && finished, tour.IsOpen.ToString());
            tourHost.Close();
        }

        // ---------------------------------------------------------------- qr encoder

        /// <summary>
        /// Structural checks on the encoder. Full matrix equality against a reference encoder is
        /// not meaningful: the same text can be laid out with a different mask or mode and still
        /// be a valid symbol, so the round trip is verified with a real decoder instead
        /// (tools/qr-ref/verify.py, driven by --qr-dump).
        /// </summary>
        /// <summary>Writes our matrices to disk so tools/qr-ref/verify.py can scan them.</summary>
        private static void DumpQr(string directory)
        {
            Directory.CreateDirectory(directory);

            string[] payloads =
            {
                "https://ant.design",
                "HELLO",
                "Antd.Wpf 二维码",
                "The quick brown fox jumps over the lazy dog. 0123456789"
            };

            for (int i = 0; i < payloads.Length; i++)
            {
                bool[,] matrix = QrEncoder.Encode(payloads[i], (QrEcc)(i % 4));
                var lines = new List<string> { payloads[i] };
                lines.AddRange(ToRows(matrix));
                File.WriteAllLines(Path.Combine(directory, "case" + i + ".txt"), lines);
            }

            Console.WriteLine("qr matrices written to " + directory);
        }

        private static string[] ToRows(bool[,] matrix)
        {
            int size = matrix.GetLength(0);
            var rows = new string[size];
            for (int row = 0; row < size; row++)
            {
                var builder = new System.Text.StringBuilder(size);
                for (int col = 0; col < size; col++) builder.Append(matrix[row, col] ? '#' : '.');
                rows[row] = builder.ToString();
            }
            return rows;
        }

        private static void CheckQrEncoder()
        {
            bool[,] small = QrEncoder.Encode("HELLO", QrEcc.Medium);
            Check("qr uses version 1 for a short payload", small.GetLength(0) == 21, small.GetLength(0).ToString());

            int reserved = CountFunctionModules(small.GetLength(0));
            Check("qr v1 has 208 data modules", small.GetLength(0) * small.GetLength(0) - reserved == 208,
                (small.GetLength(0) * small.GetLength(0) - reserved).ToString());

            Check("qr draws all three finder patterns",
                IsFinder(small, 0, 0) && IsFinder(small, 0, 14) && IsFinder(small, 14, 0), "finder");
            Check("qr keeps the dark module", small[13, 8], small[13, 8].ToString());

            int level;
            int mask;
            Check("qr format bits decode",
                QrFormatOf(small, out level, out mask) && level == 0 && mask >= 0 && mask <= 7,
                "level " + level + " mask " + mask);

            bool[,] medium = QrEncoder.Encode(new string('x', 120), QrEcc.Medium);
            Check("qr grows with the payload", medium.GetLength(0) > 21, medium.GetLength(0).ToString());

            bool[,] high = QrEncoder.Encode("Antd.Wpf 二维码", QrEcc.High);
            Check("qr handles non-ascii payloads", high.GetLength(0) >= 21, high.GetLength(0).ToString());
        }

        private static int CountFunctionModules(int size)
        {
            // Three 8x8 finder+separator blocks, two timing runs of (size - 16), both format
            // copies (15 each) and the dark module.
            return 3 * 64 + 2 * (size - 16) + 30 + 1;
        }

        private static bool IsFinder(bool[,] matrix, int row, int col)
        {
            for (int r = 0; r < 7; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    bool edge = r == 0 || r == 6 || c == 0 || c == 6;
                    bool core = r >= 2 && r <= 4 && c >= 2 && c <= 4;
                    if (matrix[row + r, col + c] != (edge || core)) return false;
                }
            }
            return true;
        }

        private static bool QrFormatOf(bool[,] matrix, out int level, out int mask)
        {
            int size = matrix.GetLength(0);
            int word = 0;

            for (int i = 0; i < 15; i++)
            {
                int row;
                int col;

                if (i < 6) { row = i; col = 8; }
                else if (i == 6) { row = 7; col = 8; }
                else if (i == 7) { row = 8; col = 8; }
                else if (i == 8) { row = 8; col = 7; }
                else { row = 8; col = 14 - i; }

                if (matrix[row, col]) word |= 1 << i;
            }

            // Undo the 0x5412 mask, then split the five data bits.
            int data = (word ^ 0x5412) >> 10;
            level = data >> 3;
            mask = data & 7;
            return size > 0;
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
            CaptureOverlays(window, directory);
            var switchPreview = FindDescendant<Antd.Wpf.Controls.Switch>(window);
            if (switchPreview != null) RenderElement(switchPreview, Path.Combine(directory, "zoom-switch.png"), 6.0);
            var layoutPreview = FindDescendant<Antd.Wpf.Controls.Layout>(window);
            if (layoutPreview != null) RenderElement(layoutPreview, Path.Combine(directory, "zoom-layout.png"), 1.0);
            var pagerShot = window.FindName("DemoPagination") as Pagination;
            if (pagerShot != null) RenderElement(pagerShot, Path.Combine(directory, "zoom-pager.png"), 3.0);
            var qrShot = window.FindName("DemoQr") as QRCode;
            if (qrShot != null) RenderElement(qrShot, Path.Combine(directory, "zoom-qr.png"), 3.0);

            var galleryTabs = window.FindName("DemoTabs") as Tabs;
            if (galleryTabs != null) RenderElement(galleryTabs, Path.Combine(directory, "zoom-tabs.png"), 3.0);

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

        private static void CaptureOverlays(Window window, string directory)
        {
            var popover = FindDescendant<Popover>(window);
            if (popover != null)
            {
                popover.IsOpen = true;
                DoEvents();
                DoEvents();
                var popup = popover.Template.FindName("PART_Popup", popover) as System.Windows.Controls.Primitives.Popup;
                var child = popup == null ? null : popup.Child as FrameworkElement;
                if (child != null)
                {
                    child.UpdateLayout();
                    DoEvents();
                    RenderElement(child, Path.Combine(directory, "zoom-popover.png"), 2.0);
                    Console.WriteLine("captured: zoom-popover.png");
                }
                popover.IsOpen = false;
                DoEvents();
            }

            var dropdown = FindDescendant<Dropdown>(window);
            if (dropdown != null)
            {
                dropdown.IsOpen = true;
                DoEvents();
                DoEvents();
                var popup = dropdown.Template.FindName("PART_Popup", dropdown) as System.Windows.Controls.Primitives.Popup;
                var child = popup == null ? null : popup.Child as FrameworkElement;
                if (child != null)
                {
                    child.UpdateLayout();
                    DoEvents();
                    RenderElement(child, Path.Combine(directory, "zoom-dropdown.png"), 2.0);
                    Console.WriteLine("captured: zoom-dropdown.png");
                }
                dropdown.IsOpen = false;
                DoEvents();
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
