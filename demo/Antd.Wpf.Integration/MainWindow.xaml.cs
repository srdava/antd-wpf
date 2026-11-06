using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Antd.Wpf.Controls;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using AntdButton = Antd.Wpf.Controls.Button;

namespace Antd.Wpf.Integration
{
    /// <summary>
    /// A single realistic screen: a service list with filtering, paging, an editor dialog with
    /// validation, row actions and theme switching. It exists to exercise the library the way an
    /// application would, instead of one control at a time.
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int PageSize = 6;

        private readonly List<ServiceRow> _all = new List<ServiceRow>();
        private readonly List<ServiceRow> _visible = new List<ServiceRow>();
        private ServiceRow _editing;
        private bool _ready;

        public MainWindow()
        {
            InitializeComponent();

            Message.Owner = this;
            Notification.Owner = this;

            StatusFilter.ItemsSource = new[] { "全部状态", "Running", "Stopped", "Error" };
            StatusFilter.SelectedIndex = 0;
            StatusInput.ItemsSource = new[] { "Running", "Stopped", "Error" };

            NameItem.Validator = ValidateName;

            Editor.Ok += OnEditorOk;

            Seed();
            _ready = true;
            ApplyFilter();
        }

        // ---------------------------------------------------------------- data

        private void Seed()
        {
            AddSeed("api-gateway", "registry.local/gateway:2.3.1", "Running", 4, "2026-10-02");
            AddSeed("auth-service", "registry.local/auth:1.8.0", "Running", 3, "2026-10-04");
            AddSeed("billing-worker", "registry.local/billing:0.9.7", "Error", 2, "2026-10-11");
            AddSeed("search-indexer", "registry.local/indexer:3.0.2", "Running", 6, "2026-10-13");
            AddSeed("report-cron", "registry.local/report:1.1.0", "Stopped", 1, "2026-10-18");
            AddSeed("api-edge", "registry.local/edge:2.3.1", "Running", 2, "2026-10-21");
            AddSeed("media-transcoder", "registry.local/media:0.4.3", "Stopped", 1, "2026-10-24");
            AddSeed("audit-sink", "registry.local/audit:1.0.5", "Running", 2, "2026-10-29");
            AddSeed("legacy-bridge", "registry.local/bridge:0.2.0", "Error", 1, "2026-11-01");
        }

        private void AddSeed(string name, string image, string status, int replicas, string created)
        {
            _all.Add(new ServiceRow
            {
                Name = name,
                Image = image,
                Status = status,
                Replicas = replicas,
                CreatedAt = created,
                HealthCheck = status != "Error"
            });
        }

        // ---------------------------------------------------------------- filtering and paging

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (!_ready) return;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = (SearchInput.Text ?? string.Empty).Trim();
            string status = StatusFilter.SelectedItem as string;

            _visible.Clear();
            foreach (ServiceRow row in _all)
            {
                if (query.Length > 0
                    && row.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0
                    && row.Image.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(status) && status != "全部状态" && row.Status != status) continue;
                _visible.Add(row);
            }

            Pager.Current = 1;
            Bind();
        }

        private void Bind()
        {
            Pager.Total = _visible.Count;

            int page = Math.Min(Pager.PageCount, Math.Max(1, Pager.Current));
            int skip = (page - 1) * PageSize;

            ServiceTable.ItemsSource = _visible.Skip(skip).Take(PageSize).ToList();

            Summary.Text = string.Format("共 {0} 条 · 第 {1}/{2} 页", _visible.Count, page, Pager.PageCount);
            UpdateSelectionState();
        }

        internal void GoToPage(int page)
        {
            Pager.Current = page;
            Bind();
        }

        private void OnPageChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            Bind();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            int count = ServiceTable.SelectedItems == null ? 0 : ServiceTable.SelectedItems.Count;
            BatchButton.IsEnabled = count > 0;
            SelectionHint.Text = count == 0 ? "未选择行" : "已选择 " + count + " 行";
        }

        // ---------------------------------------------------------------- editor

        private void OnCreate(object sender, RoutedEventArgs e)
        {
            BeginCreate();
        }

        internal void BeginCreate()
        {
            _editing = null;
            Editor.Title = "新建实例";
            NameInput.Text = string.Empty;
            ImageInput.Text = "registry.local/app:latest";
            ReplicasInput.Value = 2;
            StatusInput.SelectedIndex = 0;
            HealthInput.IsChecked = true;
            NameItem.Error = null;
            Editor.IsOpen = true;
        }

        private void OnEditRow(object sender, RoutedEventArgs e)
        {
            var row = ((FrameworkElement)sender).DataContext as ServiceRow;
            if (row != null) BeginEdit(row);
        }

        internal void BeginEdit(ServiceRow row)
        {
            if (row == null) return;

            _editing = row;
            Editor.Title = "编辑实例";
            NameInput.Text = row.Name;
            ImageInput.Text = row.Image;
            ReplicasInput.Value = row.Replicas;
            StatusInput.SelectedIndex = Math.Max(0, Array.IndexOf(new[] { "Running", "Stopped", "Error" }, row.Status));
            HealthInput.IsChecked = row.HealthCheck;
            NameItem.Error = null;
            Editor.IsOpen = true;
        }

        private void OnEditorOk(object sender, ModalButtonEventArgs e)
        {
            string error = ValidateName(NameInput);
            NameItem.Error = error;
            if (error != null)
            {
                // Keep the dialog open so the user can fix the field.
                e.Handled = true;
                return;
            }

            string name = NameInput.Text.Trim();
            string image = string.IsNullOrWhiteSpace(ImageInput.Text) ? "registry.local/app:latest" : ImageInput.Text.Trim();
            string status = StatusInput.SelectedItem as string ?? "Running";
            int replicas = ReplicasInput.Value.HasValue ? (int)Math.Round(ReplicasInput.Value.Value) : 1;

            if (_editing == null)
            {
                _all.Add(new ServiceRow
                {
                    Name = name,
                    Image = image,
                    Status = status,
                    Replicas = replicas,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    HealthCheck = HealthInput.IsChecked == true
                });
                Message.Success("实例已创建：" + name);
            }
            else
            {
                _editing.Name = name;
                _editing.Image = image;
                _editing.Status = status;
                _editing.Replicas = replicas;
                _editing.HealthCheck = HealthInput.IsChecked == true;
                Message.Success("实例已更新：" + name);
            }

            ApplyFilter();
        }

        /// <summary>Business rule the plain Required flag cannot express.</summary>
        private string ValidateName(object content)
        {
            var input = content as Input;
            string name = (input != null ? input.Text : NameInput.Text) ?? string.Empty;
            name = name.Trim();

            if (name.Length < 2) return "名称至少 2 个字符";
            if (name.Length > 20) return "名称最多 20 个字符";

            foreach (ServiceRow row in _all)
            {
                if (ReferenceEquals(row, _editing)) continue;
                if (string.Equals(row.Name, name, StringComparison.OrdinalIgnoreCase)) return "名称已被占用";
            }

            return null;
        }

        // ---------------------------------------------------------------- row actions

        private void OnDeleteConfirmed(object sender, EventArgs e)
        {
            var popconfirm = sender as Popconfirm;
            var row = popconfirm == null ? null : popconfirm.DataContext as ServiceRow;
            DeleteRow(row);
        }

        internal void DeleteRow(ServiceRow row)
        {
            if (row == null) return;
            _all.Remove(row);
            ApplyFilter();
            Message.Success("已删除：" + row.Name);
        }

        private void OnBatchStop(object sender, RoutedEventArgs e)
        {
            var selected = ServiceTable.SelectedItems == null
                ? new List<ServiceRow>()
                : ServiceTable.SelectedItems.Cast<ServiceRow>().ToList();

            if (selected.Count == 0) return;

            foreach (ServiceRow row in selected) row.Status = "Stopped";
            ServiceTable.ItemsSource = _visible.Skip((Math.Max(1, Pager.Current) - 1) * PageSize).Take(PageSize).ToList();
            Notification.Success("批量操作完成", selected.Count + " 个实例已停止。");
        }

        // ---------------------------------------------------------------- theme

        private void OnToggleTheme(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ToggleDark();
            ThemeButton.Content = ThemeManager.Current.Config.IsDark ? "浅色主题" : "深色主题";
        }

        // ---------------------------------------------------------------- acceptance pass

        /// <summary>
        /// Drives this very window instead of rebuilding the controls in a test, so binding,
        /// validation and paging are exercised through the same code the user runs.
        /// </summary>
        internal int RunSelfTest()
        {
            var failures = new List<string>();
            int checks = 0;

            Action<string, bool, string> check = delegate(string name, bool ok, string detail)
            {
                checks++;
                if (ok)
                {
                    Console.WriteLine("  . " + name);
                    return;
                }
                failures.Add(name + " (" + detail + ")");
                Console.WriteLine("  x " + name + " -> " + detail);
            };

            Pump(150);

            check("seed loads nine rows", _all.Count == 9, _all.Count.ToString());
            check("first page shows six rows", ServiceTable.Items.Count == 6, ServiceTable.Items.Count.ToString());
            check("pager knows the total", Pager.Total == 9 && Pager.PageCount == 2,
                Pager.Total + "/" + Pager.PageCount);
            check("summary line is filled", Summary.Text.Contains("共 9 条"), Summary.Text);

            GoToPage(2);
            check("second page shows the last three rows", ServiceTable.Items.Count == 3,
                ServiceTable.Items.Count.ToString());

            SearchInput.Text = "api";
            DoEvents();
            check("search filters by name", _visible.Count == 2, _visible.Count.ToString());
            check("search resets to the first page", Pager.Current == 1, Pager.Current.ToString());

            SearchInput.Text = string.Empty;
            DoEvents();
            StatusFilter.SelectedIndex = 1;
            DoEvents();
            check("status filter keeps only running rows",
                _visible.Count > 0 && _visible.All(r => r.Status == "Running"), _visible.Count.ToString());

            StatusFilter.SelectedIndex = 0;
            DoEvents();
            check("clearing filters restores every row", _visible.Count == 9, _visible.Count.ToString());

            // ---- create: an empty name must not close the dialog
            BeginCreate();
            Pump(320);
            check("editor opens on the create path", Editor.IsOpen, Editor.IsOpen.ToString());

            NameInput.Text = string.Empty;
            ClickOk();
            Pump(120);
            check("empty name keeps the dialog open", Editor.IsOpen, Editor.IsOpen.ToString());
            check("empty name shows a field error", !string.IsNullOrEmpty(NameItem.Error), NameItem.Error);

            NameInput.Text = "x";
            ClickOk();
            Pump(120);
            check("one-character name is rejected", Editor.IsOpen && NameItem.Error != null, NameItem.Error);

            NameInput.Text = "api-gateway";
            ClickOk();
            Pump(120);
            check("duplicate name is rejected", Editor.IsOpen && NameItem.Error != null, NameItem.Error);

            NameInput.Text = "edge-cache";
            ImageInput.Text = "registry.local/cache:1.0.0";
            ReplicasInput.Value = 99;
            check("replica field clamps to its declared range",
                ReplicasInput.Value.HasValue && Math.Abs(ReplicasInput.Value.Value - 20) < 1e-9,
                ReplicasInput.Value.ToString());

            ClickOk();
            Pump(320);
            check("valid create closes the dialog", !Editor.IsOpen, Editor.IsOpen.ToString());
            check("valid create appends a row", _all.Count == 10, _all.Count.ToString());
            check("created row keeps the clamped replica count",
                _all.Last().Replicas == 20, _all.Last().Replicas.ToString());
            check("create reports through a message",
                ToastHost.ForMessage(this).Count >= 1, ToastHost.ForMessage(this).Count.ToString());

            // ---- edit
            ServiceRow target = _all.First(r => r.Name == "report-cron");
            BeginEdit(target);
            Pump(320);
            check("editor opens filled for edit",
                NameInput.Text == "report-cron" && Editor.Title as string == "编辑实例", NameInput.Text);

            NameInput.Text = "report-cron-v2";
            ClickOk();
            Pump(320);
            check("edit renames the row in place", target.Name == "report-cron-v2", target.Name);
            check("edit does not add a row", _all.Count == 10, _all.Count.ToString());

            // ---- delete
            DeleteRow(target);
            DoEvents();
            check("delete removes the row", _all.Count == 9 && _all.All(r => r.Name != "report-cron-v2"),
                _all.Count.ToString());

            // ---- selection drives the batch action
            ServiceTable.SelectedItem = ServiceTable.Items[0];
            DoEvents();
            check("selecting a row enables the batch button", BatchButton.IsEnabled, BatchButton.IsEnabled.ToString());

            // ---- theme switch while the dialog is open
            BeginCreate();
            Pump(320);
            bool wasDark = ThemeManager.Current.Config.IsDark;
            OnToggleTheme(this, new RoutedEventArgs());
            Pump(120);
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            var editorBackground = Editor.Background as SolidColorBrush;
            check("theme toggle flips the mode", ThemeManager.Current.Config.IsDark != wasDark,
                ThemeManager.Current.Config.IsDark.ToString());
            check("dialog restyles with the theme",
                editorBackground != null && editorBackground.Color == snapshot.Brush("colorBgElevated").Color,
                editorBackground == null ? "no brush" : editorBackground.Color.ToString());
            check("dialog stays open across the theme switch", Editor.IsOpen, Editor.IsOpen.ToString());

            OnToggleTheme(this, new RoutedEventArgs());
            Editor.Close();
            Pump(200);
            check("closing the dialog restores the page", !Editor.IsOpen, Editor.IsOpen.ToString());

            Console.WriteLine();
            Console.WriteLine("checks: " + checks + ", failures: " + failures.Count);
            foreach (string failure in failures) Console.WriteLine("  FAIL " + failure);
            Console.WriteLine(failures.Count == 0 ? "RESULT: PASS" : "RESULT: FAIL");
            return failures.Count;
        }

        internal void CaptureTo(string path)
        {
            Pump(250);

            // The dialog lives in its own popup, so the window's visual tree does not contain it.
            FrameworkElement element = (FrameworkElement)Content;
            if (Editor.IsOpen)
            {
                var surface = Editor.Template.FindName("PART_Surface", Editor) as FrameworkElement;
                if (surface != null) element = surface;
            }
            element.UpdateLayout();

            int width = (int)Math.Ceiling(element.ActualWidth > 0 ? element.ActualWidth : Width);
            int height = (int)Math.Ceiling(element.ActualHeight > 0 ? element.ActualHeight : Height);

            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                context.DrawRectangle(new VisualBrush(element), null, new Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = File.Create(path)) encoder.Save(stream);
        }

        private void ClickOk()
        {
            var ok = Editor.Template.FindName("PART_Ok", Editor) as AntdButton;
            if (ok == null) return;
            ok.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
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

    /// <summary>Row model for the list. Plain INotifyPropertyChanged, as an app would write it.</summary>
    public sealed class ServiceRow : INotifyPropertyChanged
    {
        private string _name;
        private string _image;
        private string _status;
        private int _replicas;
        private bool _healthCheck;

        public string Name
        {
            get { return _name; }
            set { if (_name == value) return; _name = value; Raise("Name"); }
        }

        public string Image
        {
            get { return _image; }
            set { if (_image == value) return; _image = value; Raise("Image"); }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                if (_status == value) return;
                _status = value;
                Raise("Status");
                Raise("StatusText");
                Raise("StatusColor");
            }
        }

        public int Replicas
        {
            get { return _replicas; }
            set { if (_replicas == value) return; _replicas = value; Raise("Replicas"); }
        }

        public bool HealthCheck
        {
            get { return _healthCheck; }
            set { if (_healthCheck == value) return; _healthCheck = value; Raise("HealthCheck"); }
        }

        public string CreatedAt { get; set; }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "Running": return "运行中";
                    case "Stopped": return "已停止";
                    case "Error": return "异常";
                    default: return Status;
                }
            }
        }

        public AntdTagColor StatusColor
        {
            get
            {
                switch (Status)
                {
                    case "Running": return AntdTagColor.Success;
                    case "Stopped": return AntdTagColor.Default;
                    case "Error": return AntdTagColor.Error;
                    default: return AntdTagColor.Default;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Raise(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
    }
}
