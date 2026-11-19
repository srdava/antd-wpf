using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>One stop on a tour: the element to highlight and the copy for the card.</summary>
    public class TourStep
    {
        public FrameworkElement Target { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// antd Tour: a dimmed mask with a hole over the current target and a card that walks through
    /// the steps. Both the mask and the card live in one Popup covering the window.
    /// </summary>
    public class Tour : Control
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Pane, "Tour");
        }
        private Popup _popup;
        private FrameworkElement _mask;
        private Path _spotlight;
        private FrameworkElement _card;

        static Tour()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tour), new FrameworkPropertyMetadata(typeof(Tour)));
        }

        public Tour()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
            Steps = new ObservableCollection<TourStep>();
        }

        public ObservableCollection<TourStep> Steps { get; private set; }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof(bool), typeof(Tour), new FrameworkPropertyMetadata(false, OnOpenChanged));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(
            "Current", typeof(int), typeof(Tour),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnStepChanged));

        public int Current
        {
            get { return (int)GetValue(CurrentProperty); }
            set { SetValue(CurrentProperty, value); }
        }

        private static readonly DependencyPropertyKey CurrentTitleKey = DependencyProperty.RegisterReadOnly(
            "CurrentTitle", typeof(string), typeof(Tour), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty CurrentTitleProperty = CurrentTitleKey.DependencyProperty;

        public string CurrentTitle
        {
            get { return (string)GetValue(CurrentTitleProperty); }
            private set { SetValue(CurrentTitleKey, value); }
        }

        private static readonly DependencyPropertyKey CurrentDescriptionKey = DependencyProperty.RegisterReadOnly(
            "CurrentDescription", typeof(string), typeof(Tour), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty CurrentDescriptionProperty = CurrentDescriptionKey.DependencyProperty;

        public string CurrentDescription
        {
            get { return (string)GetValue(CurrentDescriptionProperty); }
            private set { SetValue(CurrentDescriptionKey, value); }
        }

        private static readonly DependencyPropertyKey PositionTextKey = DependencyProperty.RegisterReadOnly(
            "PositionText", typeof(string), typeof(Tour), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PositionTextProperty = PositionTextKey.DependencyProperty;

        /// <summary>"2 / 5" style counter shown at the bottom of the card.</summary>
        public string PositionText
        {
            get { return (string)GetValue(PositionTextProperty); }
            private set { SetValue(PositionTextKey, value); }
        }

        private static readonly DependencyPropertyKey StepCountKey = DependencyProperty.RegisterReadOnly(
            "StepCount", typeof(int), typeof(Tour), new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty StepCountProperty = StepCountKey.DependencyProperty;

        public int StepCount
        {
            get { return (int)GetValue(StepCountProperty); }
            private set { SetValue(StepCountKey, value); }
        }

        private static readonly DependencyPropertyKey IsLastStepKey = DependencyProperty.RegisterReadOnly(
            "IsLastStep", typeof(bool), typeof(Tour), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsLastStepProperty = IsLastStepKey.DependencyProperty;

        public bool IsLastStep
        {
            get { return (bool)GetValue(IsLastStepProperty); }
            private set { SetValue(IsLastStepKey, value); }
        }

        /// <summary>Raised when the tour is dismissed or finished.</summary>
        public event EventHandler Finished;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _popup = AntdMotion.Configure(GetTemplateChild("PART_Popup") as Popup);
            _mask = GetTemplateChild("PART_Mask") as FrameworkElement;
            _spotlight = GetTemplateChild("PART_Spotlight") as Path;
            _card = GetTemplateChild("PART_Card") as FrameworkElement;

            var next = GetTemplateChild("PART_Next") as Button;
            var prev = GetTemplateChild("PART_Prev") as Button;
            var close = GetTemplateChild("PART_Close") as Button;

            if (next != null) next.Click += delegate { Next(); };
            if (prev != null) prev.Click += delegate { Previous(); };
            if (close != null) close.Click += delegate { Close(); };

            SyncStep();
        }

        public void Next()
        {
            if (Current + 1 >= Steps.Count)
            {
                Close();
                return;
            }

            SetCurrentValue(CurrentProperty, Current + 1);
        }

        public void Previous()
        {
            if (Current <= 0) return;
            SetCurrentValue(CurrentProperty, Current - 1);
        }

        public void Close()
        {
            SetCurrentValue(IsOpenProperty, false);

            EventHandler handler = Finished;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void Open()
        {
            if (Steps.Count == 0) return;
            SetCurrentValue(CurrentProperty, 0);
            SetCurrentValue(IsOpenProperty, true);
        }

        private static void OnOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tour = (Tour)d;
            bool open = (bool)e.NewValue;

            if (tour._popup != null) tour._popup.IsOpen = open;
            if (open) tour.SyncStep();
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Tour)d).SyncStep();
        }

        private void SyncStep()
        {
            SetValue(StepCountKey, Steps.Count);
            SetValue(IsLastStepKey, Current >= Steps.Count - 1);

            if (!IsOpen || Steps.Count == 0) return;

            Window window = Window.GetWindow(this);
            if (window == null) return;

            if (_mask != null)
            {
                _mask.Width = window.ActualWidth;
                _mask.Height = window.ActualHeight;
            }

            if (_popup != null)
            {
                // Absolute placement in screen coordinates: the mask has to cover the window
                // even when the trigger itself has been scrolled out of view.
                Point screen = window.PointToScreen(new Point(0, 0));
                _popup.Placement = PlacementMode.Absolute;
                _popup.HorizontalOffset = screen.X;
                _popup.VerticalOffset = screen.Y;
            }

            int index = Math.Min(Steps.Count - 1, Math.Max(0, Current));
            TourStep step = Steps[index];

            SetValue(CurrentTitleKey, step.Title ?? string.Empty);
            SetValue(CurrentDescriptionKey, step.Description ?? string.Empty);
            SetValue(PositionTextKey, (index + 1) + " / " + Steps.Count);
            Rect hole = new Rect(0, 0, 0, 0);

            if (step.Target != null)
            {
                try
                {
                    Point origin = step.Target.TransformToAncestor(window).Transform(new Point(0, 0));
                    hole = new Rect(origin.X - 4, origin.Y - 4,
                        step.Target.ActualWidth + 8, step.Target.ActualHeight + 8);
                }
                catch (InvalidOperationException)
                {
                    // Target lives in another visual tree; fall back to a centred card.
                }
            }

            PaintSpotlight(window, hole);
            PlaceCard(window, hole);
        }

        /// <summary>Full-window rectangle with the target cut out of it.</summary>
        private void PaintSpotlight(Window window, Rect hole)
        {
            if (_spotlight == null) return;

            var full = new RectangleGeometry(new Rect(0, 0, window.ActualWidth, window.ActualHeight));
            if (hole.Width <= 0 || hole.Height <= 0)
            {
                _spotlight.Data = full;
                return;
            }

            var cutout = new RectangleGeometry(hole, 6, 6);
            _spotlight.Data = new CombinedGeometry(GeometryCombineMode.Exclude, full, cutout);
        }

        private void PlaceCard(Window window, Rect hole)
        {
            if (_card == null) return;

            double cardWidth = _card.ActualWidth > 0 ? _card.ActualWidth : 280;
            double width = window.ActualWidth;
            double height = window.ActualHeight;

            double left = hole.Width > 0 ? hole.Right + 12 : (width - cardWidth) / 2;
            double top = hole.Height > 0 ? hole.Top : (height - 120) / 2;

            if (left + cardWidth > width - 16) left = Math.Max(16, width - cardWidth - 16);
            top = Math.Max(16, Math.Min(top, height - 160));

            Canvas.SetLeft(_card, left);
            Canvas.SetTop(_card, top);
            _card.Width = 280;
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;

            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, Brushes.Transparent);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }
}
