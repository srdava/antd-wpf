using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Theme
{
    /// <summary>
    /// Single entry point for theming. Owns the active <see cref="ThemeSnapshot"/> and a
    /// resource dictionary that exposes every token as <c>Antd.&lt;tokenName&gt;</c> so XAML can
    /// use <c>{DynamicResource Antd.colorPrimary}</c>.
    ///
    /// Two consumption paths on purpose:
    ///  - XAML: DynamicResource, because Setter.Value cannot hold a Binding.
    ///  - Code: Snapshot/Brushes, pre-frozen and allocation free.
    /// </summary>
    public sealed class ThemeManager : INotifyPropertyChanged
    {
        public const string ResourcePrefix = "Antd.";

        private static readonly ThemeManager Instance = new ThemeManager();

        private readonly List<Attachment> _attachments = new List<Attachment>();
        private ThemeConfig _config;
        private ThemeSnapshot _snapshot;
        private ResourceDictionary _resources;

        private ThemeManager()
        {
            _config = ThemeConfig.CreateDefault();
            _snapshot = new ThemeSnapshot(TokenBuilder.Build(_config.Seed, _config.Algorithm), _config.Algorithm);
            _resources = BuildResources(_snapshot);
        }

        public static ThemeManager Current
        {
            get { return Instance; }
        }

        public ThemeConfig Config
        {
            get { return _config; }
        }

        public ThemeSnapshot Snapshot
        {
            get { return _snapshot; }
        }

        public ResourceDictionary Resources
        {
            get { return _resources; }
        }

        /// <summary>Raised after a new snapshot is installed. Controls subscribe to refresh.</summary>
        public event EventHandler ThemeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        // ---------------------------------------------------------------- apply

        public void Apply(ThemeConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");

            _config = config;
            ThemeSnapshot next = new ThemeSnapshot(
                TokenBuilder.Build(config.Seed, config.Algorithm), config.Algorithm);

            _snapshot = next;
            _resources = BuildResources(next);

            foreach (Attachment attachment in _attachments)
            {
                attachment.Replace(_resources);
            }

            OnPropertyChanged("Snapshot");
            EventHandler handler = ThemeChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void ApplyLight()
        {
            ThemeConfig next = _config.Clone();
            next.Algorithm = ThemeAlgorithm.Default;
            Apply(next);
        }

        public void ApplyDark()
        {
            ThemeConfig next = _config.Clone();
            next.Algorithm = ThemeAlgorithm.Dark;
            Apply(next);
        }

        public void ToggleDark()
        {
            if (_config.IsDark) ApplyLight();
            else ApplyDark();
        }

        /// <summary>Changes a single seed value and re-derives everything.</summary>
        public void UpdateSeed(Action<SeedToken> mutate)
        {
            if (mutate == null) throw new ArgumentNullException("mutate");
            ThemeConfig next = _config.Clone();
            mutate(next.Seed);
            Apply(next);
        }

        // ---------------------------------------------------------------- attach

        /// <summary>
        /// Merges the token dictionary into a resource host (Application.Resources or
        /// Window.Resources). Call once per host; theme changes update it automatically.
        /// </summary>
        public void Attach(ResourceDictionary host)
        {
            if (host == null) throw new ArgumentNullException("host");
            var attachment = new Attachment(host, _resources);
            host.MergedDictionaries.Insert(0, _resources);
            _attachments.Add(attachment);
        }

        public void Detach(ResourceDictionary host)
        {
            for (int i = _attachments.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_attachments[i].Host, host))
                {
                    _attachments[i].Dispose();
                    _attachments.RemoveAt(i);
                }
            }
        }

        // ---------------------------------------------------------------- resources

        private static ResourceDictionary BuildResources(ThemeSnapshot snapshot)
        {
            var dictionary = new ResourceDictionary();
            AliasToken tokens = snapshot.Tokens;

            foreach (KeyValuePair<string, object> pair in tokens.Values)
            {
                string key = ResourcePrefix + pair.Key;

                if (pair.Value is AntdColor)
                {
                    dictionary[key] = ColorInterop.ToFrozenBrush((AntdColor)pair.Value);
                }
                else if (pair.Value is double || pair.Value is string || pair.Value is bool)
                {
                    dictionary[key] = pair.Value;
                }
            }

            dictionary[ResourcePrefix + "FontFamily"] = snapshot.FontFamily;
            dictionary[ResourcePrefix + "FontFamilyCode"] = snapshot.FontFamilyCode;
            dictionary[ResourcePrefix + "IsDark"] = snapshot.IsDark;
            return dictionary;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class Attachment
        {
            private ResourceDictionary _current;

            public Attachment(ResourceDictionary host, ResourceDictionary current)
            {
                Host = host;
                _current = current;
            }

            public ResourceDictionary Host { get; private set; }

            public void Replace(ResourceDictionary next)
            {
                int index = Host.MergedDictionaries.IndexOf(_current);
                if (index >= 0) Host.MergedDictionaries.RemoveAt(index);
                if (next != null)
                {
                    int insertAt = index >= 0 ? index : 0;
                    Host.MergedDictionaries.Insert(insertAt, next);
                }
                _current = next;
            }

            public void Dispose()
            {
                if (_current != null)
                {
                    Host.MergedDictionaries.Remove(_current);
                    _current = null;
                }
            }
        }
    }
}
