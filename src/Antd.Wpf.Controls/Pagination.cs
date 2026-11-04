using System;
using System.Windows;
using System.Windows.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd Pagination: previous/next arrows plus numbered pages with an ellipsis when the range
    /// is too long. Pages beyond the first/last two collapse into "…", same rule as antd.
    /// </summary>
    public class Pagination : Control
    {
        private Panel _host;

        static Pagination()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Pagination), new FrameworkPropertyMetadata(typeof(Pagination)));
        }

        public Pagination()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty TotalProperty = DependencyProperty.Register(
            "Total", typeof(int), typeof(Pagination), new FrameworkPropertyMetadata(0, OnRangeChanged));

        public int Total
        {
            get { return (int)GetValue(TotalProperty); }
            set { SetValue(TotalProperty, value); }
        }

        public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
            "PageSize", typeof(int), typeof(Pagination), new FrameworkPropertyMetadata(10, OnRangeChanged));

        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(
            "Current", typeof(int), typeof(Pagination),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRangeChanged, CoerceCurrent));

        /// <summary>One-based page number.</summary>
        public int Current
        {
            get { return (int)GetValue(CurrentProperty); }
            set { SetValue(CurrentProperty, value); }
        }

        public int PageCount
        {
            get
            {
                int size = Math.Max(1, PageSize);
                return Math.Max(1, (Total + size - 1) / size);
            }
        }

        /// <summary>Raised when the user picks a page; <c>NewValue</c> is the one-based page.</summary>
        public event RoutedPropertyChangedEventHandler<int> PageChanged;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _host = GetTemplateChild("PART_Host") as Panel;
            Rebuild();
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pagination = (Pagination)d;
            pagination.CoerceValue(CurrentProperty);
            pagination.Rebuild();
        }

        private static object CoerceCurrent(DependencyObject d, object value)
        {
            var pagination = (Pagination)d;
            int page = (int)value;
            return Math.Min(pagination.PageCount, Math.Max(1, page));
        }

        private void GoTo(int page)
        {
            int target = Math.Min(PageCount, Math.Max(1, page));
            if (target == Current)
            {
                Rebuild();
                return;
            }

            int previous = Current;
            SetCurrentValue(CurrentProperty, target);

            RoutedPropertyChangedEventHandler<int> handler = PageChanged;
            if (handler != null)
            {
                handler(this, new RoutedPropertyChangedEventArgs<int>(previous, target));
            }
        }

        private void Rebuild()
        {
            if (_host == null) return;

            _host.Children.Clear();
            int pages = PageCount;
            int current = Math.Min(pages, Math.Max(1, Current));

            _host.Children.Add(CreateButton("\u2039", current - 1, current <= 1, false));

            int last = 0;
            for (int page = 1; page <= pages; page++)
            {
                bool near = page <= 2 || page > pages - 2 || Math.Abs(page - current) <= 1;
                if (!near)
                {
                    // Collapse the middle into a single ellipsis.
                    if (last != -1)
                    {
                        _host.Children.Add(CreateEllipsis());
                        last = -1;
                    }
                    continue;
                }

                _host.Children.Add(CreateButton(page.ToString(), page, false, page == current));
                last = page;
            }

            _host.Children.Add(CreateButton("\u203A", current + 1, current >= pages, false));
        }

        private PaginationPageButton CreateButton(string text, int page, bool disabled, bool active)
        {
            var button = new PaginationPageButton
            {
                Content = text,
                IsActive = active,
                IsEnabled = !disabled
            };
            button.Click += delegate { GoTo(page); };
            return button;
        }

        private PaginationPageButton CreateEllipsis()
        {
            return new PaginationPageButton { Content = "\u2026", IsEnabled = false, IsEllipsis = true };
        }

        public void UpdateVisualState()
        {
            AliasToken t = ThemeManager.Current.Snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
            SetCurrentValue(BackgroundProperty, System.Windows.Media.Brushes.Transparent);
            SetCurrentValue(PaddingProperty, new Thickness(0));
        }
    }

    /// <summary>Square page button; the active one gets the primary outline antd uses.</summary>
    public class PaginationPageButton : Button
    {
        static PaginationPageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaginationPageButton),
                new FrameworkPropertyMetadata(typeof(PaginationPageButton)));
        }

        public PaginationPageButton()
        {
            Width = 32;
            Height = 32;
            Margin = new Thickness(4, 0, 0, 0);
            Padding = new Thickness(0);
            MinHeight = 0;
            Variant = ButtonVariant.Text;
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(PaginationPageButton), new FrameworkPropertyMetadata(false));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsEllipsisProperty = DependencyProperty.Register(
            "IsEllipsis", typeof(bool), typeof(PaginationPageButton), new FrameworkPropertyMetadata(false));

        public bool IsEllipsis
        {
            get { return (bool)GetValue(IsEllipsisProperty); }
            set { SetValue(IsEllipsisProperty, value); }
        }
    }
}
