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
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--shot" && i + 1 < args.Length) shotDir = args[i + 1];
            }

            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            ThemeManager.Current.Attach(app.Resources);
            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
            DoEvents();

            Console.WriteLine("Antd.Wpf M0 smoke tests");
            Console.WriteLine("------------------------------");

            CheckArchitecture();
            CheckTokens();
            CheckControls();
            CheckThemeSwitch();

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
            var window = new MainWindow { Width = 1080, Height = 860, WindowStartupLocation = WindowStartupLocation.Manual, Left = -4000, Top = 0 };
            window.Show();
            DoEvents();
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

        private static void Render(Window window, string path)
        {
            window.UpdateLayout();
            var root = window.Content as FrameworkElement;
            if (root == null) return;

            root.UpdateLayout();
            int width = (int)Math.Ceiling(root.ActualWidth);
            int height = (int)Math.Ceiling(root.ActualHeight);
            if (width <= 0 || height <= 0) return;

            // The window itself is not part of Content, so composite its background first;
            // otherwise dark theme captures come out on a transparent (white looking) page.
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(window.Background ?? Brushes.White, null, new Rect(0, 0, width, height));
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
