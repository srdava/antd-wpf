using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// antd QRCode. The symbol is produced by the in-repo encoder (no third-party dependency):
    /// byte mode, versions 1-10, four error correction levels.
    /// </summary>
    public class QRCode : Control
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Image, "QRCode");
        }
        private bool[,] _matrix;
        private string _matrixValue;
        private QrEcc _matrixEcc = (QrEcc)(-1);

        static QRCode()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(QRCode), new FrameworkPropertyMetadata(typeof(QRCode)));
        }

        public QRCode()
        {
            ThemeLink.Attach(this, UpdateVisualState);
            UpdateVisualState();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(string), typeof(QRCode),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender, OnContentChanged));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ErrorCorrectionProperty = DependencyProperty.Register(
            "ErrorCorrection", typeof(QrEcc), typeof(QRCode),
            new FrameworkPropertyMetadata(QrEcc.Medium,
                FrameworkPropertyMetadataOptions.AffectsRender, OnContentChanged));

        public QrEcc ErrorCorrection
        {
            get { return (QrEcc)GetValue(ErrorCorrectionProperty); }
            set { SetValue(ErrorCorrectionProperty, value); }
        }

        public static readonly DependencyProperty ModuleBrushProperty = DependencyProperty.Register(
            "ModuleBrush", typeof(Brush), typeof(QRCode), new FrameworkPropertyMetadata(Brushes.Black));

        public Brush ModuleBrush
        {
            get { return (Brush)GetValue(ModuleBrushProperty); }
            set { SetValue(ModuleBrushProperty, value); }
        }

        public static readonly DependencyProperty QuietZoneProperty = DependencyProperty.Register(
            "QuietZone", typeof(int), typeof(QRCode),
            new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Margin in modules around the symbol; the spec asks for four.</summary>
        public int QuietZone
        {
            get { return (int)GetValue(QuietZoneProperty); }
            set { SetValue(QuietZoneProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            EnsureMatrix();

            int modules = _matrix == null ? 21 : _matrix.GetLength(0);
            double cell = 4;
            double side = (modules + QuietZone * 2) * cell;

            if (!double.IsInfinity(constraint.Width) && !double.IsNaN(constraint.Width) && constraint.Width > 0)
            {
                cell = Math.Max(1, Math.Floor(constraint.Width / (modules + QuietZone * 2)));
                side = (modules + QuietZone * 2) * cell;
            }

            return new Size(side, side);
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var code = (QRCode)d;
            code._matrix = null;
            code.InvalidateMeasure();
            code.InvalidateVisual();
        }

        private void EnsureMatrix()
        {
            if (_matrix != null && _matrixValue == Value && _matrixEcc == ErrorCorrection) return;

            try
            {
                _matrix = QrEncoder.Encode(Value ?? string.Empty, ErrorCorrection);
            }
            catch (ArgumentException)
            {
                // Longer than a version 10 symbol; draw nothing rather than a wrong code.
                _matrix = null;
            }

            _matrixValue = Value;
            _matrixEcc = ErrorCorrection;
        }

        protected override void OnRender(DrawingContext dc)
        {
            EnsureMatrix();

            var background = Background ?? Brushes.White;
            dc.DrawRectangle(background, null, new Rect(0, 0, ActualWidth, ActualHeight));
            if (_matrix == null) return;

            int modules = _matrix.GetLength(0);
            double cell = Math.Floor(Math.Min(ActualWidth, ActualHeight) / (modules + QuietZone * 2));
            if (cell < 1) cell = 1;

            double side = cell * (modules + QuietZone * 2);
            double originX = (ActualWidth - side) / 2 + cell * QuietZone;
            double originY = (ActualHeight - side) / 2 + cell * QuietZone;

            for (int row = 0; row < modules; row++)
            {
                for (int col = 0; col < modules; col++)
                {
                    if (!_matrix[row, col]) continue;
                    dc.DrawRectangle(ModuleBrush, null,
                        new Rect(originX + col * cell, originY + row * cell, cell, cell));
                }
            }
        }

        public void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            SetCurrentValue(BackgroundProperty, snapshot.Brush("colorBgContainer"));
            SetCurrentValue(ModuleBrushProperty, snapshot.Brush("colorText"));
        }
    }
}
