using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Antd.Wpf.Controls;
using Antd.Wpf.Theme;

namespace Antd.Wpf.FeedbackTests
{
    /// <summary>
    /// M4 acceptance harness: the feedback layer (Alert, Result, Modal, Drawer, Popconfirm, Message,
    /// Notification). Runs headless on net48 and net8.0-windows and can drop PNGs for visual review.
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
            app.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml")
            });

            ThemeManager.Current.Attach(app.Resources);
            ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
            DoEvents();

            Console.WriteLine("Antd.Wpf M4 feedback tests");
            Console.WriteLine("------------------------------");

            CheckAlert();
            CheckResult();
            CheckModal();
            CheckDrawer();
            CheckPopconfirm();
            CheckMessage();
            CheckNotification();

            if (shotDir != null) Capture(shotDir);

            Console.WriteLine();
            Console.WriteLine("checks: " + _checks + ", failures: " + Failures.Count);
            foreach (string failure in Failures) Console.WriteLine("  FAIL " + failure);
            Console.WriteLine(Failures.Count == 0 ? "RESULT: PASS" : "RESULT: FAIL");

            app.Shutdown();
            return Failures.Count == 0 ? 0 : 1;
        }

        private static void Check(string name, bool ok, string detail)
        {
            _checks++;
            if (ok)
            {
                Console.WriteLine("  . " + name);
                return;
            }
            Failures.Add(name + " (" + detail + ")");
            Console.WriteLine("  x " + name + "  -> " + detail);
        }

        // ---------------------------------------------------------------- Alert

        private static void CheckAlert()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            var info = new Alert { Message = "Informational note", ShowIcon = true };
            var described = new Alert
            {
                Type = AlertType.Success,
                Message = "Upload complete",
                Description = "Three files were stored in the project library.",
                Closable = true
            };
            var banner = new Alert { Type = AlertType.Warning, Banner = true, Message = "Banner alert" };

            Window host = Host(700, 420, new StackPanel
            {
                Width = 620,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20),
                Children = { info, described, banner }
            });

            Check("alert info background = colorInfoBg",
                SameBrush(info.Background, snapshot.Brush("colorInfoBg")), Describe(info.Background));
            Check("alert info border = colorInfoBorder",
                SameBrush(info.BorderBrush, snapshot.Brush("colorInfoBorder")), Describe(info.BorderBrush));
            Check("alert one-line height is 40", Near(info.ActualHeight, 40, 0.6), info.ActualHeight.ToString("F1"));
            Check("alert one-line padding is 8/12",
                info.Padding == new Thickness(12, 8, 12, 8), info.Padding.ToString());
            Check("alert radius is 8", info.CornerRadius.TopLeft == 8, info.CornerRadius.TopLeft.ToString());

            Check("described alert height is 97", Near(described.ActualHeight, 97, 0.6),
                described.ActualHeight.ToString("F1"));
            Check("described alert padding is 20/24",
                described.Padding == new Thickness(24, 20, 24, 20), described.Padding.ToString());
            Check("described alert title is 16", Near(described.MessageFontSize, 16, 0.01),
                described.MessageFontSize.ToString());
            Check("described alert shows the description",
                described.DescriptionVisibility == Visibility.Visible, described.DescriptionVisibility.ToString());
            Check("described alert icon is 24", Near(described.IconSize, 24, 0.01), described.IconSize.ToString());

            Check("banner has no border or radius",
                banner.BorderThickness == new Thickness(0) && banner.CornerRadius.TopLeft == 0,
                banner.BorderThickness + "/" + banner.CornerRadius.TopLeft);
            Check("banner uses colorWarningBg",
                SameBrush(banner.Background, snapshot.Brush("colorWarningBg")), Describe(banner.Background));
            Check("banner height is 38", Near(banner.ActualHeight, 38, 0.6), banner.ActualHeight.ToString("F1"));

            bool closed = false;
            described.Closed += delegate { closed = true; };
            var close = described.Template.FindName("PART_Close", described) as FrameworkElement;
            Check("alert close target exists", close != null, close == null ? "missing" : "ok");
            if (close != null)
            {
                close.RaiseEvent(LeftUp);
                DoEvents();
            }
            Check("alert close hides it and raises Closed",
                closed && described.Visibility == Visibility.Collapsed, closed + "/" + described.Visibility);

            host.Close();
        }

        // ---------------------------------------------------------------- Result

        private static void CheckResult()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            var result = new Result
            {
                Status = ResultStatus.Success,
                Title = "Operation successful",
                SubTitle = "The record was saved and the queue drained.",
                Extra = new Antd.Wpf.Controls.Button { Content = "Back to list", Variant = ButtonVariant.Primary }
            };

            Window host = Host(700, 500, new StackPanel
            {
                Width = 620,
                VerticalAlignment = VerticalAlignment.Top,
                Children = { result }
            });

            Check("result padding is 48/32",
                result.Padding == new Thickness(32, 48, 32, 48), result.Padding.ToString());
            Check("result success icon uses colorSuccess",
                SameBrush(result.IconBrush, snapshot.Brush("colorSuccess")), Describe(result.IconBrush));
            Check("result height is 310", Near(result.ActualHeight, 310, 3), result.ActualHeight.ToString("F1"));

            var title = result.Template.FindName("PART_TitleHost", result) as ContentPresenter;
            Check("result title slot exists", title != null, title == null ? "missing" : "ok");

            result.Status = ResultStatus.NotFound;
            DoEvents();
            Check("404 swaps the glyph for a numeral",
                result.StatusText == "404" && result.StatusTextVisibility == Visibility.Visible
                && result.IconVisibility == Visibility.Collapsed,
                result.StatusText + "/" + result.StatusTextVisibility);

            result.Status = ResultStatus.ServerError;
            DoEvents();
            Check("500 uses the other numeral", result.StatusText == "500", result.StatusText);

            host.Close();
        }

        // ---------------------------------------------------------------- Modal

        private static void CheckModal()
        {
            var modal = new Modal
            {
                Title = "Modal title",
                Content = new TextBlock { Text = "Modal body copy" }
            };

            int closedCount = 0;
            modal.Closed += delegate { closedCount++; };

            var hostContent = new Grid();
            hostContent.Children.Add(modal);
            Window host = Host(800, 600, hostContent);

            var popup = modal.Template.FindName("PART_Popup", modal) as System.Windows.Controls.Primitives.Popup;
            var surface = modal.Template.FindName("PART_Surface", modal) as FrameworkElement;
            var card = modal.Template.FindName("PART_Card", modal) as FrameworkElement;
            var mask = modal.Template.FindName("PART_Mask", modal) as UIElement;

            Check("modal is closed until asked", !modal.IsOpen && (popup == null || !popup.IsOpen), modal.IsOpen.ToString());

            modal.IsOpen = true;
            DoEvents();
            Pump(350);
            host.UpdateLayout();

            Check("modal opens its layer", popup != null && popup.IsOpen, "not open");
            Check("modal layer is anchored to the window",
                popup != null && ReferenceEquals(popup.PlacementTarget, host), "wrong target");
            Check("modal surface covers the client area",
                surface != null && Near(surface.ActualWidth, hostContent.ActualWidth, 0.6)
                && Near(surface.ActualHeight, hostContent.ActualHeight, 0.6),
                surface == null ? "missing" : surface.ActualWidth + "x" + surface.ActualHeight
                    + " vs " + hostContent.ActualWidth + "x" + hostContent.ActualHeight);
            Check("modal card is 520 wide", card != null && Near(card.ActualWidth, 520, 0.6),
                card == null ? "missing" : card.ActualWidth.ToString("F1"));
            Check("modal card sits 100 from the top",
                card != null && Near(card.Margin.Top, 100, 0.01)
                && card.VerticalAlignment == VerticalAlignment.Top,
                card == null ? "missing" : card.Margin.Top.ToString());
            Check("modal traps Tab inside the card",
                card != null && System.Windows.Input.KeyboardNavigation.GetTabNavigation(card)
                    == System.Windows.Input.KeyboardNavigationMode.Cycle,
                card == null ? "missing" : "not cycle");

            bool okRaised = false;
            modal.Ok += delegate { okRaised = true; };
            var ok = modal.Template.FindName("PART_Ok", modal) as Antd.Wpf.Controls.Button;
            Check("modal shows the default footer", ok != null, ok == null ? "missing" : "ok");
            if (ok != null)
            {
                ok.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                DoEvents();
            }
            Check("modal OK flips IsOpen immediately", okRaised && !modal.IsOpen,
                okRaised + "/" + modal.IsOpen);

            // The layer is torn down only after the exit motion, so wait it out before reopening.
            Pump(400);
            Check("modal OK closes and reports",
                !modal.IsOpen && closedCount == 1 && popup != null && !popup.IsOpen,
                okRaised + "/" + modal.IsOpen + "/" + closedCount);

            // Reopening while the exit motion is still running must not leave the dialog closing.
            modal.IsOpen = false;
            DoEvents();
            modal.IsOpen = true;
            DoEvents();
            Pump(500);
            Check("reopening during the exit motion keeps the dialog open", modal.IsOpen,
                modal.IsOpen.ToString());
            modal.MaskClosable = false;
            modal.IsOpen = true;
            DoEvents();
            Pump(350);
            if (mask != null)
            {
                mask.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice, Environment.TickCount,
                    System.Windows.Input.MouseButton.Left)
                { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
                DoEvents();
            }
            Check("mask click is ignored when MaskClosable is false", modal.IsOpen, modal.IsOpen.ToString());

            modal.MaskClosable = true;
            if (mask != null)
            {
                mask.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice, Environment.TickCount,
                    System.Windows.Input.MouseButton.Left)
                { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
                DoEvents();
            }
            Check("mask click closes when MaskClosable is true", !modal.IsOpen, modal.IsOpen.ToString());

            modal.Centered = true;
            modal.IsOpen = true;
            DoEvents();
            Pump(350);
            Check("centered modal ignores the 100px offset",
                card != null && card.VerticalAlignment == VerticalAlignment.Center
                && card.Margin.Top == 0,
                card == null ? "missing" : card.VerticalAlignment + "/" + card.Margin.Top);

            var source = PresentationSource.FromVisual(host);
            if (source != null && surface != null)
            {
                surface.RaiseEvent(new System.Windows.Input.KeyEventArgs(
                    System.Windows.Input.Keyboard.PrimaryDevice, source, 0, System.Windows.Input.Key.Escape)
                { RoutedEvent = UIElement.PreviewKeyDownEvent });
                DoEvents();
            }
            Check("Esc closes the modal", !modal.IsOpen, modal.IsOpen.ToString());

            host.Close();
        }

        // ---------------------------------------------------------------- Drawer

        private static void CheckDrawer()
        {
            var drawer = new Drawer { Title = "Drawer title", Content = new TextBlock { Text = "Drawer body" } };
            var hostContent = new Grid();
            hostContent.Children.Add(drawer);
            Window host = Host(800, 600, hostContent);

            drawer.IsOpen = true;
            DoEvents();
            Pump(350);
            host.UpdateLayout();

            var card = drawer.Template.FindName("PART_Card", drawer) as FrameworkElement;
            Check("drawer defaults to the right edge",
                card != null && card.HorizontalAlignment == HorizontalAlignment.Right,
                card == null ? "missing" : card.HorizontalAlignment.ToString());
            Check("drawer is 378 wide", card != null && Near(card.ActualWidth, 378, 0.6),
                card == null ? "missing" : card.ActualWidth.ToString("F1"));
            Check("drawer fills the height",
                card != null && Near(card.ActualHeight, hostContent.ActualHeight, 0.6),
                card == null ? "missing" : card.ActualHeight + " vs " + hostContent.ActualHeight);

            drawer.Placement = AntdPlacement.Left;
            DoEvents();
            Check("left drawer flips to the left edge",
                card != null && card.HorizontalAlignment == HorizontalAlignment.Left,
                card == null ? "missing" : card.HorizontalAlignment.ToString());

            drawer.Placement = AntdPlacement.Top;
            DoEvents();
            Check("top drawer spans the width and is 378 tall",
                card != null && card.VerticalAlignment == VerticalAlignment.Top
                && Near(card.ActualHeight, 378, 0.6)
                && Near(card.ActualWidth, hostContent.ActualWidth, 0.6),
                card == null ? "missing" : card.ActualWidth + "x" + card.ActualHeight);

            drawer.Placement = AntdPlacement.Bottom;
            DoEvents();
            Check("bottom drawer hugs the bottom edge",
                card != null && card.VerticalAlignment == VerticalAlignment.Bottom,
                card == null ? "missing" : card.VerticalAlignment.ToString());

            Check("drawer footer is hidden without content",
                drawer.FooterVisibility == Visibility.Collapsed, drawer.FooterVisibility.ToString());

            drawer.Footer = new Antd.Wpf.Controls.Button { Content = "Submit", Variant = ButtonVariant.Primary };
            DoEvents();
            Check("drawer footer appears when supplied",
                drawer.FooterVisibility == Visibility.Visible, drawer.FooterVisibility.ToString());

            drawer.Closable = false;
            var close = drawer.Template.FindName("PART_Close", drawer) as FrameworkElement;
            Check("drawer close hides when Closable is false",
                close != null && close.Visibility == Visibility.Collapsed,
                close == null ? "missing" : close.Visibility.ToString());
            drawer.Closable = true;

            drawer.Close();
            DoEvents();
            Check("drawer closes", !drawer.IsOpen, drawer.IsOpen.ToString());
            host.Close();
        }

        // ---------------------------------------------------------------- Popconfirm

        private static void CheckPopconfirm()
        {
            var popconfirm = new Popconfirm
            {
                Title = "Delete this item?",
                Description = "This action cannot be undone."
            };
            popconfirm.Content = new Antd.Wpf.Controls.Button { Content = "Delete" };

            Window host = Host(480, 320, new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                Children = { popconfirm }
            });

            Check("popconfirm starts closed", !popconfirm.IsOpen, popconfirm.IsOpen.ToString());
            Check("popconfirm shows the description row",
                popconfirm.DescriptionVisibility == Visibility.Visible, popconfirm.DescriptionVisibility.ToString());

            bool confirmed = false;
            bool canceled = false;
            popconfirm.Confirmed += delegate { confirmed = true; };
            popconfirm.Canceled += delegate { canceled = true; };

            popconfirm.IsOpen = true;
            DoEvents();

            var ok = popconfirm.Template.FindName("PART_Ok", popconfirm) as Antd.Wpf.Controls.Button;
            var cancel = popconfirm.Template.FindName("PART_Cancel", popconfirm) as Antd.Wpf.Controls.Button;
            Check("popconfirm renders both buttons", ok != null && cancel != null,
                (ok == null ? "no ok" : "ok") + "/" + (cancel == null ? "no cancel" : "cancel"));
            if (ok != null)
            {
                ok.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                DoEvents();
            }
            Check("confirm closes and reports", confirmed && !popconfirm.IsOpen,
                confirmed + "/" + popconfirm.IsOpen);

            popconfirm.IsOpen = true;
            DoEvents();
            if (cancel != null)
            {
                cancel.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                DoEvents();
            }
            Check("cancel closes and reports", canceled && !popconfirm.IsOpen, canceled + "/" + popconfirm.IsOpen);

            popconfirm.Description = null;
            Check("emptying the description hides the row",
                popconfirm.DescriptionVisibility == Visibility.Collapsed,
                popconfirm.DescriptionVisibility.ToString());

            host.Close();
        }

        // ---------------------------------------------------------------- Message

        private static void CheckMessage()
        {
            Message.Owner = null;
            Notification.Owner = null;

            var host = Host(640, 420, new Grid());
            Message.Owner = host;
            Notification.Owner = host;

            ToastHost layer = ToastHost.ForMessage(host);

            var first = Message.Success("Saved successfully");
            Check("message opens its layer", layer.IsLayerOpen && layer.Count == 1,
                layer.IsLayerOpen + "/" + layer.Count);

            Message.Info("Second");
            Message.Error("Third");
            Check("messages stack in one layer", layer.Count == 3, layer.Count.ToString());

            first.Close();
            Check("closing a message removes just that one", layer.Count == 2, layer.Count.ToString());
            first.Close();
            Check("closing twice is a no-op", layer.Count == 2, layer.Count.ToString());

            for (int i = 0; i < 6; i++) Message.Info("flood " + i);
            Check("message stack caps at five", layer.Count == 5, layer.Count.ToString());
            Message.DestroyAll();
            Check("destroy all empties the layer", layer.Count == 0 && !layer.IsLayerOpen,
                layer.Count + "/" + layer.IsLayerOpen);

            var shortLived = Message.Show("brief", MessageType.Info, 0.08);
            Check("timed message is on screen", layer.Count == 1, layer.Count.ToString());
            Pump(400);
            Check("timed message closes itself", layer.Count == 0, layer.Count.ToString());
            shortLived.Close();

            Message.Owner = null;
            host.Close();
        }

        // ---------------------------------------------------------------- Notification

        private static void CheckNotification()
        {
            var host = Host(640, 420, new Grid());
            Notification.Owner = host;

            bool closed = false;
            var config = new NotificationConfig();
            config.Title = "Notification title";
            config.Description = "A short description line.";
            config.Duration = 0;
            config.Placement = NotificationPlacement.BottomLeft;
            config.Closed = delegate { closed = true; };

            ToastHandle notice = Notification.Show(config);
            ToastHost layer = ToastHost.ForNotification(host);

            Check("notification opens its layer", layer.IsLayerOpen && layer.Count == 1,
                layer.IsLayerOpen + "/" + layer.Count);

            Notification.Success("Deploy finished", "Build 42 is live.");
            Check("notifications stack", layer.Count == 2, layer.Count.ToString());

            notice.Close();
            Check("notification close reports back", closed && layer.Count == 1,
                closed + "/" + layer.Count);

            Notification.DestroyAll();
            Check("destroy all empties the notification layer",
                layer.Count == 0 && !layer.IsLayerOpen, layer.Count + "/" + layer.IsLayerOpen);

            Notification.Owner = null;
            host.Close();
        }

        // ---------------------------------------------------------------- shots

        private static void Capture(string dir)
        {
            Directory.CreateDirectory(dir);

            var info = new Alert { Message = "Informational note", ShowIcon = true };
            var described = new Alert
            {
                Type = AlertType.Success,
                Message = "Upload complete",
                Description = "Three files were stored in the project library.",
                Closable = true
            };
            var banner = new Alert { Type = AlertType.Warning, Banner = true, Message = "Banner alert" };
            var error = new Alert { Type = AlertType.Error, Message = "Something went wrong", Closable = true };

            var result = new Result
            {
                Status = ResultStatus.Success,
                Title = "Operation successful",
                SubTitle = "The record was saved and the queue drained.",
                Extra = new Antd.Wpf.Controls.Button { Content = "Back to list", Variant = ButtonVariant.Primary }
            };

            var page = new StackPanel { Width = 620, Margin = new Thickness(24) };
            page.Children.Add(info);
            page.Children.Add(new Border { Height = 12 });
            page.Children.Add(described);
            page.Children.Add(new Border { Height = 12 });
            page.Children.Add(banner);
            page.Children.Add(new Border { Height = 12 });
            page.Children.Add(error);
            page.Children.Add(new Border { Height = 24 });
            page.Children.Add(result);

            Window host = Host(700, 760, page);
            Render(page, Path.Combine(dir, "m4-feedback.png"));
            host.Close();

            var modal = new Modal
            {
                Title = "Modal title",
                Content = new TextBlock { Text = "Modal body copy that explains the decision." }
            };
            var modalHost = new Grid();
            modalHost.Children.Add(modal);
            Window modalWindow = Host(760, 520, modalHost);
            modal.IsOpen = true;
            DoEvents();
            Pump(350);
            modalWindow.UpdateLayout();
            var surface = modal.Template.FindName("PART_Surface", modal) as FrameworkElement;
            if (surface != null) Render(surface, Path.Combine(dir, "m4-modal.png"));
            modalWindow.Close();

            var drawer = new Drawer
            {
                Title = "Drawer title",
                Content = new TextBlock { Text = "Drawer body copy." },
                Footer = new Antd.Wpf.Controls.Button { Content = "Submit", Variant = ButtonVariant.Primary }
            };
            var drawerHost = new Grid();
            drawerHost.Children.Add(drawer);
            Window drawerWindow = Host(760, 520, drawerHost);
            drawer.IsOpen = true;
            DoEvents();
            Pump(350);
            drawerWindow.UpdateLayout();
            var drawerSurface = drawer.Template.FindName("PART_Surface", drawer) as FrameworkElement;
            if (drawerSurface != null) Render(drawerSurface, Path.Combine(dir, "m4-drawer.png"));
            drawerWindow.Close();

            var toastHost = new Grid();
            Window toastWindow = Host(640, 320, toastHost);
            Message.Owner = toastWindow;
            Notification.Owner = toastWindow;
            Message.Success("Saved successfully");
            Message.Info("Three records were exported.");
            Notification.Info("Notification title", "A short description line.");
            DoEvents();
            Render(ToastHost.ForMessage(toastWindow).LayerRoot, Path.Combine(dir, "m4-message.png"), ToastedBackdrop);
            Render(ToastHost.ForNotification(toastWindow).LayerRoot, Path.Combine(dir, "m4-notification.png"), ToastedBackdrop);
            Message.Owner = null;
            Notification.Owner = null;
            toastWindow.Close();
        }

        private static readonly Brush ToastedBackdrop = Brushes.White;

        private static Window Host(double width, double height, object content)
        {
            var host = new Window
            {
                Width = width,
                Height = height,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Content = content
            };
            host.Show();
            DoEvents();
            host.UpdateLayout();
            return host;
        }

        private static void Render(FrameworkElement element, string path)
        {
            Render(element, path, Brushes.White);
        }

        private static void Render(FrameworkElement element, string path, Brush backdrop)
        {
            element.UpdateLayout();
            double width = element.ActualWidth > 0 ? element.ActualWidth : 600;
            double height = element.ActualHeight > 0 ? element.ActualHeight : 400;

            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(backdrop, null, new Rect(0, 0, width, height));
                context.DrawRectangle(new VisualBrush(element), null, new Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height),
                96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = File.Create(path)) encoder.Save(stream);
        }

        // ---------------------------------------------------------------- helpers

        private static readonly System.Windows.Input.MouseButtonEventArgs LeftUp =
            new System.Windows.Input.MouseButtonEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice, Environment.TickCount,
                System.Windows.Input.MouseButton.Left)
            { RoutedEvent = UIElement.MouseLeftButtonUpEvent };

        private static bool Near(double actual, double expected, double tolerance)
        {
            return Math.Abs(actual - expected) <= tolerance;
        }

        private static bool SameBrush(Brush actual, Brush expected)
        {
            var a = actual as SolidColorBrush;
            var b = expected as SolidColorBrush;
            return a != null && b != null && a.Color == b.Color;
        }

        private static string Describe(Brush brush)
        {
            var solid = brush as SolidColorBrush;
            return solid == null ? "null" : solid.Color.ToString();
        }

        private static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(delegate { frame.Continue = false; }));
            Dispatcher.PushFrame(frame);
        }

        private static void Pump(int milliseconds)
        {
            var frame = new DispatcherFrame();
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(milliseconds);
            timer.Tick += delegate
            {
                timer.Stop();
                frame.Continue = false;
            };
            timer.Start();
            Dispatcher.PushFrame(frame);
        }
    }
}
