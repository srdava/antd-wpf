using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Antd.Wpf.Icons;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>Severity shared by Message and Notification.</summary>
    public enum MessageType
    {
        Info = 0,
        Success = 1,
        Error = 2,
        Warning = 3,
        Loading = 4
    }

    /// <summary>Corner a notification stack grows from.</summary>
    public enum NotificationPlacement
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3
    }

    /// <summary>Handle for a toast that is already on screen. Closing twice is a no-op.</summary>
    public sealed class ToastHandle
    {
        private Action _close;

        internal ToastHandle(Action close)
        {
            _close = close;
        }

        public void Close()
        {
            Action close = _close;
            _close = null;
            if (close != null) close();
        }
    }

    /// <summary>
    /// antd message. Measured: 40 tall, 9/12 padding, 8 radius, a 16px status icon 8 from the copy.
    /// Messages stack at the top centre, eight pixels below the client edge.
    /// </summary>
    public static class Message
    {
        /// <summary>Window the toasts belong to. Defaults to the application's main window.</summary>
        public static Window Owner { get; set; }

        public static ToastHandle Show(string content)
        {
            return Show(content, MessageType.Info, 3);
        }

        public static ToastHandle Show(string content, MessageType type)
        {
            return Show(content, type, 3);
        }

        /// <summary><paramref name="duration"/> of zero or less keeps the toast until it is closed.</summary>
        public static ToastHandle Show(string content, MessageType type, double duration)
        {
            Window window = ToastWindow.Resolve(Owner);
            if (window == null) return new ToastHandle(null);
            return ToastHost.ForMessage(window).AddMessage(content, type, duration);
        }

        public static ToastHandle Info(string content) { return Show(content, MessageType.Info, 3); }
        public static ToastHandle Success(string content) { return Show(content, MessageType.Success, 3); }
        public static ToastHandle Error(string content) { return Show(content, MessageType.Error, 3); }
        public static ToastHandle Warning(string content) { return Show(content, MessageType.Warning, 3); }

        /// <summary>Sticky toast with a spinner, for work that finishes elsewhere.</summary>
        public static ToastHandle Loading(string content) { return Show(content, MessageType.Loading, 0); }

        public static void DestroyAll()
        {
            Window window = ToastWindow.Resolve(Owner);
            if (window == null) return;
            ToastHost.ForMessage(window).Clear();
        }
    }

    /// <summary>
    /// antd notification. Measured: 384 wide, 20/24 padding, 8 radius, 16/24 title, 14/22 body, a
    /// 24px status icon inset 12 from the copy and a 22px close target in the top-right corner.
    /// </summary>
    public static class Notification
    {
        public static Window Owner { get; set; }

        public static ToastHandle Show(NotificationConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");

            Window window = ToastWindow.Resolve(Owner);
            if (window == null) return new ToastHandle(null);
            return ToastHost.ForNotification(window).AddNotification(config);
        }

        /// <summary>Shorthand for a titled notice with a body line.</summary>
        public static ToastHandle Show(string title, string description, MessageType type)
        {
            var config = new NotificationConfig();
            config.Title = title;
            config.Description = description;
            config.Type = type;
            return Show(config);
        }

        public static ToastHandle Success(string title, string description)
        {
            return Show(title, description, MessageType.Success);
        }

        public static ToastHandle Info(string title, string description)
        {
            return Show(title, description, MessageType.Info);
        }

        public static ToastHandle Warning(string title, string description)
        {
            return Show(title, description, MessageType.Warning);
        }

        public static ToastHandle Error(string title, string description)
        {
            return Show(title, description, MessageType.Error);
        }

        public static void DestroyAll()
        {
            Window window = ToastWindow.Resolve(Owner);
            if (window == null) return;
            ToastHost.ForNotification(window).Clear();
        }
    }

    /// <summary>Options for <see cref="Notification.Show(NotificationConfig)"/>.</summary>
    public class NotificationConfig
    {
        public NotificationConfig()
        {
            Type = MessageType.Info;
            Duration = 4.5;
            Placement = NotificationPlacement.TopRight;
            ShowIcon = true;
            Closable = true;
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public MessageType Type { get; set; }

        /// <summary>Seconds on screen; zero or less keeps the notice pinned.</summary>
        public double Duration { get; set; }

        public NotificationPlacement Placement { get; set; }

        public bool ShowIcon { get; set; }

        public bool Closable { get; set; }

        /// <summary>Raised when the notice leaves the screen, however that happened.</summary>
        public Action Closed { get; set; }
    }

    /// <summary>Finds a window to hang a toast layer on.</summary>
    internal static class ToastWindow
    {
        public static Window Resolve(Window preferred)
        {
            if (preferred != null && preferred.IsVisible) return preferred;

            Application app = Application.Current;
            if (app == null) return null;

            if (app.MainWindow != null && app.MainWindow.IsVisible) return app.MainWindow;

            foreach (Window window in app.Windows)
            {
                if (window.IsVisible) return window;
            }
            return null;
        }
    }

    /// <summary>
    /// One transparent layer pinned over a window, holding either the message stack or the four
    /// notification corners. Layers are cached per window and dropped when the window closes.
    /// </summary>
    internal sealed class ToastHost
    {
        private const int MaxMessages = 5;

        private static readonly Dictionary<Window, ToastHost> MessageHosts = new Dictionary<Window, ToastHost>();
        private static readonly Dictionary<Window, ToastHost> NotificationHosts = new Dictionary<Window, ToastHost>();

        private readonly Window _window;
        private readonly Popup _popup;
        private readonly Grid _root;
        private readonly StackPanel _messageStack;
        private readonly Panel[] _corners;
        private readonly List<ToastEntry> _entries = new List<ToastEntry>();

        private ToastHost(Window window, bool messageLayer)
        {
            _window = window;
            _root = new Grid { Background = Brushes.Transparent };

            if (messageLayer)
            {
                _messageStack = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                _root.Children.Add(_messageStack);
            }
            else
            {
                _corners = new Panel[4];
                for (int i = 0; i < 4; i++)
                {
                    var panel = new StackPanel { Margin = new Thickness(24) };
                    switch (i)
                    {
                        case 0:
                            panel.HorizontalAlignment = HorizontalAlignment.Left;
                            panel.VerticalAlignment = VerticalAlignment.Top;
                            break;
                        case 1:
                            panel.HorizontalAlignment = HorizontalAlignment.Right;
                            panel.VerticalAlignment = VerticalAlignment.Top;
                            break;
                        case 2:
                            panel.HorizontalAlignment = HorizontalAlignment.Left;
                            panel.VerticalAlignment = VerticalAlignment.Bottom;
                            break;
                        default:
                            panel.HorizontalAlignment = HorizontalAlignment.Right;
                            panel.VerticalAlignment = VerticalAlignment.Bottom;
                            break;
                    }

                    _corners[i] = panel;
                    _root.Children.Add(panel);
                }
            }

            _popup = new Popup
            {
                PlacementTarget = window,
                Placement = PlacementMode.Relative,
                HorizontalOffset = 0,
                VerticalOffset = 0,
                StaysOpen = true,
                AllowsTransparency = true,
                Focusable = false,
                Child = _root
            };

            window.SizeChanged += OnWindowSizeChanged;
            window.Closed += OnWindowClosed;
            SyncSize();
        }

        public static ToastHost ForMessage(Window window)
        {
            ToastHost host;
            if (!MessageHosts.TryGetValue(window, out host))
            {
                host = new ToastHost(window, true);
                MessageHosts[window] = host;
            }
            return host;
        }

        public static ToastHost ForNotification(Window window)
        {
            ToastHost host;
            if (!NotificationHosts.TryGetValue(window, out host))
            {
                host = new ToastHost(window, false);
                NotificationHosts[window] = host;
            }
            return host;
        }

        internal int Count
        {
            get { return _entries.Count; }
        }

        internal bool IsLayerOpen
        {
            get { return _popup.IsOpen; }
        }

        /// <summary>The full-window surface, exposed so the test harness can render it to a bitmap.</summary>
        internal FrameworkElement LayerRoot
        {
            get { return _root; }
        }

        public ToastHandle AddMessage(string content, MessageType type, double duration)
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            FrameworkElement item = BuildMessage(content, type, snapshot);

            if (_messageStack.Children.Count >= MaxMessages)
            {
                RemoveEntry(_entries[0]);
            }

            item.Margin = new Thickness(0, 0, 0, 8);
            _messageStack.Children.Add(item);
            Show();

            ToastEntry entry = Track(item, duration, delegate { });
            return new ToastHandle(delegate { RemoveEntry(entry); });
        }

        public ToastHandle AddNotification(NotificationConfig config)
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;

            ToastEntry entry = null;
            FrameworkElement item = BuildNotification(config, snapshot, delegate { RemoveEntry(entry); });

            Panel panel = _corners[(int)config.Placement];
            bool bottom = config.Placement == NotificationPlacement.BottomLeft
                          || config.Placement == NotificationPlacement.BottomRight;
            item.Margin = bottom ? new Thickness(0, 16, 0, 0) : new Thickness(0, 0, 0, 16);
            panel.Children.Add(item);
            Show();

            entry = Track(item, config.Duration, config.Closed);
            return new ToastHandle(delegate { RemoveEntry(entry); });
        }

        public void Clear()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                RemoveEntry(_entries[i]);
            }
        }

        private ToastEntry Track(FrameworkElement item, double duration, Action closed)
        {
            var entry = new ToastEntry { Element = item, Closed = closed };
            if (duration > 0)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(duration) };
                timer.Tick += delegate
                {
                    timer.Stop();
                    RemoveEntry(entry);
                };
                timer.Start();
                entry.Timer = timer;
            }
            _entries.Add(entry);
            return entry;
        }

        private void RemoveEntry(ToastEntry entry)
        {
            if (entry == null || entry.Removed) return;
            entry.Removed = true;

            if (entry.Timer != null) entry.Timer.Stop();

            Panel parent = entry.Element.Parent as Panel;
            if (parent != null) parent.Children.Remove(entry.Element);

            _entries.Remove(entry);

            Action closed = entry.Closed;
            entry.Closed = null;
            if (closed != null) closed();

            if (_entries.Count == 0) Hide();
        }

        private void Show()
        {
            if (!_popup.IsOpen) _popup.IsOpen = true;
        }

        private void Hide()
        {
            _popup.IsOpen = false;
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SyncSize();
        }

        private void SyncSize()
        {
            FrameworkElement content = _window.Content as FrameworkElement;
            double w = content != null && content.ActualWidth > 0 ? content.ActualWidth : _window.ActualWidth;
            double h = content != null && content.ActualHeight > 0 ? content.ActualHeight : _window.ActualHeight;
            if (w <= 0) w = _window.Width;
            if (h <= 0) h = _window.Height;
            if (double.IsNaN(w) || w <= 0) w = 640;
            if (double.IsNaN(h) || h <= 0) h = 480;
            _root.Width = w;
            _root.Height = h;
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _window.SizeChanged -= OnWindowSizeChanged;
            _window.Closed -= OnWindowClosed;
            _popup.IsOpen = false;
            MessageHosts.Remove(_window);
            NotificationHosts.Remove(_window);
            _entries.Clear();
        }

        // ------------------------------------------------------------ item visuals

        private static FrameworkElement BuildMessage(string content, MessageType type, ThemeSnapshot snapshot)
        {
            AliasToken t = snapshot.Tokens;

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var icon = new Icon
            {
                Data = IconFor(type),
                Size = 16,
                Foreground = ColorFor(type, snapshot),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var text = new TextBlock
            {
                Text = content,
                FontFamily = snapshot.FontFamily,
                FontSize = t.FontSize,
                LineHeight = t.FontSize * t.LineHeight,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                Foreground = snapshot.Brush("colorText"),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            row.Children.Add(icon);
            row.Children.Add(text);

            return new Border
            {
                Background = snapshot.Brush("colorBgElevated"),
                CornerRadius = new CornerRadius(t.BorderRadiusLG),
                Padding = new Thickness(12, 9, 12, 9),
                Child = row,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        private static FrameworkElement BuildNotification(NotificationConfig config, ThemeSnapshot snapshot, Action close)
        {
            AliasToken t = snapshot.Tokens;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            if (config.ShowIcon)
            {
                var icon = new Icon
                {
                    Data = IconFor(config.Type),
                    Size = 24,
                    Foreground = ColorFor(config.Type, snapshot),
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 0, 12, 0)
                };
                Grid.SetColumn(icon, 0);
                grid.Children.Add(icon);
            }

            var copy = new StackPanel();
            copy.Children.Add(new TextBlock
            {
                Text = config.Title,
                FontFamily = snapshot.FontFamily,
                FontSize = t.FontSizeLG,
                LineHeight = 24,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                Foreground = snapshot.Brush("colorTextHeading"),
                TextWrapping = TextWrapping.Wrap
            });

            if (!string.IsNullOrEmpty(config.Description))
            {
                copy.Children.Add(new TextBlock
                {
                    Text = config.Description,
                    FontFamily = snapshot.FontFamily,
                    FontSize = t.FontSize,
                    LineHeight = t.FontSize * t.LineHeight,
                    LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                    Foreground = snapshot.Brush("colorText"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 8, 0, 0)
                });
            }

            Grid.SetColumn(copy, 1);
            grid.Children.Add(copy);

            var notice = new Border
            {
                Background = snapshot.Brush("colorBgElevated"),
                CornerRadius = new CornerRadius(t.BorderRadiusLG),
                Padding = new Thickness(24, 20, 24, 20),
                Width = 384,
                Child = grid
            };

            if (config.Closable)
            {
                Border button = BuildCloseButton(snapshot);
                Grid.SetColumn(button, 2);
                grid.Children.Add(button);
                button.MouseLeftButtonUp += delegate
                {
                    if (close != null) close();
                };
            }

            return notice;
        }

        private static Border BuildCloseButton(ThemeSnapshot snapshot)
        {
            var icon = new Icon
            {
                Data = AntdIcons.Close,
                Size = 12,
                Foreground = snapshot.Brush("colorIcon"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var button = new Border
            {
                Width = 22,
                Height = 22,
                CornerRadius = new CornerRadius(4),
                Background = Brushes.Transparent,
                Child = icon,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(8, 0, 0, 0)
            };

            button.MouseEnter += delegate { button.Background = snapshot.Brush("colorFillTertiary"); };
            button.MouseLeave += delegate { button.Background = Brushes.Transparent; };
            return button;
        }

        private static Geometry IconFor(MessageType type)
        {
            switch (type)
            {
                case MessageType.Success: return AntdIcons.CheckCircle;
                case MessageType.Error: return AntdIcons.CloseCircle;
                case MessageType.Warning: return AntdIcons.ExclamationCircle;
                case MessageType.Loading: return AntdIcons.Loading;
                default: return AntdIcons.InfoCircle;
            }
        }

        private static Brush ColorFor(MessageType type, ThemeSnapshot snapshot)
        {
            switch (type)
            {
                case MessageType.Success: return snapshot.Brush("colorSuccess");
                case MessageType.Error: return snapshot.Brush("colorError");
                case MessageType.Warning: return snapshot.Brush("colorWarning");
                case MessageType.Loading: return snapshot.Brush("colorPrimary");
                default: return snapshot.Brush("colorInfo");
            }
        }

        private sealed class ToastEntry
        {
            public FrameworkElement Element;
            public DispatcherTimer Timer;
            public Action Closed;
            public bool Removed;
        }
    }
}
