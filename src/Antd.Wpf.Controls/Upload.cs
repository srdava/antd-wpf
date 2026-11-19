using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Antd.Wpf.Controls.Internal;
using Antd.Wpf.Theme;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Controls
{
    public enum AntdUploadStatus
    {
        Ready = 0,
        Uploading = 1,
        Done = 2,
        Error = 3
    }

    /// <summary>One row in an <see cref="Upload"/> list.</summary>
    public class UploadFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public AntdUploadStatus Status { get; set; }
        public double Progress { get; set; }

        /// <summary>Human readable size, the way antd prints it in the list.</summary>
        public string SizeText
        {
            get
            {
                if (Size < 1024) return Size + " B";
                if (Size < 1024 * 1024) return (Size / 1024.0).ToString("0.#") + " KB";
                return (Size / (1024.0 * 1024.0)).ToString("0.#") + " MB";
            }
        }
    }

    /// <summary>
    /// Remove button used inside the upload list. It resolves the row's file from its DataContext
    /// and walks up to the owning Upload, so the template needs no code-behind event handler
    /// (which a generic.xaml dictionary cannot have).
    /// </summary>
    public class UploadRemoveButton : Button
    {
        public UploadRemoveButton()
        {
            Variant = ButtonVariant.Link;
            Size = AntdSize.Small;
            Margin = new Thickness(8, 0, 0, 0);
            Content = AntdLocale.Current.Remove;
            Click += OnClick;
            LocaleLink.Attach(this, ApplyLocale);
        }

        private string _localizedContent;

        /// <summary>Only overwrites the label while the host has not supplied its own.</summary>
        private void ApplyLocale()
        {
            string text = AntdLocale.Current.Remove;
            string current = Content as string;
            if (current == null || current == _localizedContent)
            {
                Content = text;
            }
            _localizedContent = text;
        }
        private void OnClick(object sender, RoutedEventArgs e)
        {
            var file = DataContext as UploadFile;
            if (file == null) return;

            DependencyObject node = this;
            while (node != null)
            {
                var upload = node as Upload;
                if (upload != null) { upload.RemoveFile(file); break; }
                node = System.Windows.Media.VisualTreeHelper.GetParent(node);
            }
            e.Handled = true;
        }
    }

    /// <summary>
    /// antd Upload: a dashed drop zone plus a file list.
    ///
    /// The control deliberately does not perform network IO. It collects files, reports status
    /// and raises <see cref="FileSelected"/>; the host decides how to transfer them and pushes
    /// progress back into the <see cref="UploadFile"/> objects.
    /// </summary>
    public class Upload : Control
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AntdContainerAutomationPeer(this, AutomationControlType.Group, "Upload");
        }
        private const double HeaderHeight = 40;
        private FrameworkElement _dropZone;

        static Upload()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Upload), new FrameworkPropertyMetadata(typeof(Upload)));
        }

        public Upload()
        {
            Files = new ObservableCollection<UploadFile>();
            Files.CollectionChanged += OnFilesChanged;
            ThemeLink.Attach(this, UpdateVisualState);
            AllowDrop = true;
            UpdateVisualState();
        }

        public ObservableCollection<UploadFile> Files { get; private set; }

        /// <summary>Raised with the files the user just picked or dropped.</summary>
        public event EventHandler<UploadFile[]> FileSelected;

        public static readonly DependencyProperty MultipleProperty = DependencyProperty.Register(
            "Multiple", typeof(bool), typeof(Upload), new FrameworkPropertyMetadata(true));

        public bool Multiple
        {
            get { return (bool)GetValue(MultipleProperty); }
            set { SetValue(MultipleProperty, value); }
        }

        public static readonly DependencyProperty AcceptProperty = DependencyProperty.Register(
            "Accept", typeof(string), typeof(Upload), new FrameworkPropertyMetadata(null));

        /// <summary>Filter such as ".png;.jpg". Passed to the open dialog and checked on drop.</summary>
        public string Accept
        {
            get { return (string)GetValue(AcceptProperty); }
            set { SetValue(AcceptProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(
            "ButtonText", typeof(string), typeof(Upload), new FrameworkPropertyMetadata("Upload"));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
            "Hint", typeof(string), typeof(Upload), new FrameworkPropertyMetadata("Click or drag files here"));

        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        private static readonly DependencyPropertyKey IsDraggingOverKey = DependencyProperty.RegisterReadOnly(
            "IsDraggingOver", typeof(bool), typeof(Upload), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsDraggingOverProperty = IsDraggingOverKey.DependencyProperty;

        public bool IsDraggingOver
        {
            get { return (bool)GetValue(IsDraggingOverProperty); }
            private set { SetValue(IsDraggingOverKey, value); }
        }

        private static readonly DependencyPropertyKey HasFilesKey = DependencyProperty.RegisterReadOnly(
            "HasFiles", typeof(bool), typeof(Upload), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasFilesProperty = HasFilesKey.DependencyProperty;

        public bool HasFiles
        {
            get { return (bool)GetValue(HasFilesProperty); }
            private set { SetValue(HasFilesKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_dropZone != null)
            {
                _dropZone.MouseLeftButtonUp -= OnZoneClick;
                _dropZone.DragOver -= OnDragOver;
                _dropZone.Drop -= OnDrop;
                _dropZone.DragLeave -= OnDragLeave;
            }

            _dropZone = GetTemplateChild("PART_DropZone") as FrameworkElement;
            if (_dropZone != null)
            {
                _dropZone.MouseLeftButtonUp += OnZoneClick;

                // The drop zone is a Border: make it reachable and operable from the keyboard.
                _dropZone.Focusable = true;
                System.Windows.Input.KeyboardNavigation.SetIsTabStop(_dropZone, true);
                _dropZone.KeyDown -= OnZoneKeyDown;
                _dropZone.KeyDown += OnZoneKeyDown;
                _dropZone.DragOver += OnDragOver;
                _dropZone.Drop += OnDrop;
                _dropZone.DragLeave += OnDragLeave;
            }
        }

        private void OnFilesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetValue(HasFilesKey, Files.Count > 0);
        }

        private void OnZoneKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Space) return;
            Browse();
            e.Handled = true;
        }

        private void OnZoneClick(object sender, MouseButtonEventArgs e)
        {
            Browse();
            if (e != null) e.Handled = true;
        }

        private void Browse()
        {
            if (!IsEnabled) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = Multiple,
                Filter = string.IsNullOrEmpty(Accept) ? AntdLocale.Current.AllFiles : BuildFilter(Accept)
            };

            if (dialog.ShowDialog() == true)
            {
                AddFiles(dialog.FileNames);
            }
        }

        /// <summary>
        /// Normalises an accept string (".png;.jpg", "png, jpg", "*.png") into ".png" style
        /// extensions. An empty result means "any file": either accept was empty, or it used MIME
        /// wildcards such as "image/*", which a Win32 open dialog cannot express.
        /// </summary>
        internal static string[] ParseAccept(string accept)
        {
            if (string.IsNullOrEmpty(accept)) return new string[0];

            var extensions = new List<string>();
            foreach (string raw in accept.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string part = raw.Trim();
                if (part.Length == 0) continue;
                if (part == "*" || part == "*.*" || part.IndexOf('/') >= 0) return new string[0];

                if (part.StartsWith("*.", StringComparison.Ordinal)) part = part.Substring(1);
                else if (!part.StartsWith(".", StringComparison.Ordinal)) part = "." + part;

                if (!extensions.Contains(part)) extensions.Add(part);
            }
            return extensions.ToArray();
        }

        /// <summary>
        /// Win32 filter string. The display half lists extensions, the pattern half needs the
        /// asterisks: "Supported files (*.png;*.jpg)|*.png;*.jpg". The old version reused the
        /// dotted list on both sides, so the dialog matched nothing.
        /// </summary>
        internal static string BuildFilter(string accept)
        {
            string[] extensions = ParseAccept(accept);
            if (extensions.Length == 0) return AntdLocale.Current.AllFiles;

            var patterns = new StringBuilder();
            foreach (string extension in extensions)
            {
                if (patterns.Length > 0) patterns.Append(';');
                patterns.Append('*').Append(extension);
            }

            string list = patterns.ToString();
            return "Supported files (" + list + ")|" + list + "|All files (*.*)|*.*";
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            bool hasFiles = e.Data.GetDataPresent(DataFormats.FileDrop);
            e.Effects = hasFiles && IsEnabled ? DragDropEffects.Copy : DragDropEffects.None;
            SetValue(IsDraggingOverKey, hasFiles && IsEnabled);
            e.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            SetValue(IsDraggingOverKey, false);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            SetValue(IsDraggingOverKey, false);
            if (!IsEnabled) return;

            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || paths.Length == 0) return;

            AddFiles(Multiple ? paths : new[] { paths[0] });
            e.Handled = true;
        }

        private void AddFiles(string[] paths)
        {
            var added = new System.Collections.Generic.List<UploadFile>();
            foreach (string path in paths)
            {
                if (!Accepts(path)) continue;
                var file = new UploadFile
                {
                    Name = System.IO.Path.GetFileName(path),
                    Path = path,
                    Size = SafeSize(path),
                    Status = AntdUploadStatus.Ready
                };
                Files.Add(file);
                added.Add(file);
            }

            if (added.Count == 0) return;

            EventHandler<UploadFile[]> handler = FileSelected;
            if (handler != null) handler(this, added.ToArray());
        }

        internal bool Accepts(string path)
        {
            string[] extensions = ParseAccept(Accept);
            if (extensions.Length == 0) return true;

            string extension = System.IO.Path.GetExtension(path);
            foreach (string allowed in extensions)
            {
                if (string.Equals(allowed, extension, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static long SafeSize(string path)
        {
            try { return new FileInfo(path).Length; }
            catch (IOException) { return 0; }
            catch (UnauthorizedAccessException) { return 0; }
        }

        public void RemoveFile(UploadFile file)
        {
            if (file != null) Files.Remove(file);
        }

        private void UpdateVisualState()
        {
            ThemeSnapshot snapshot = ThemeManager.Current.Snapshot;
            AliasToken t = snapshot.Tokens;
            SetCurrentValue(FontSizeProperty, t.FontSize);
        }
    }
}
