using System;
using Antd.Wpf.Tokens;

namespace Antd.Wpf.Theme
{
    /// <summary>
    /// Everything a consumer can change about the theme. Mirrors antd's ConfigProvider
    /// `theme` prop: seed overrides plus an algorithm.
    /// </summary>
    public sealed class ThemeConfig
    {
        public ThemeConfig()
        {
            Seed = new SeedToken();
            Algorithm = ThemeAlgorithm.Default;
        }

        public SeedToken Seed { get; set; }

        public ThemeAlgorithm Algorithm { get; set; }

        public bool IsDark
        {
            get { return (Algorithm & ThemeAlgorithm.Dark) == ThemeAlgorithm.Dark; }
        }

        public ThemeConfig Clone()
        {
            return new ThemeConfig
            {
                Seed = Seed.Clone(),
                Algorithm = Algorithm
            };
        }

        public static ThemeConfig CreateDefault()
        {
            return new ThemeConfig();
        }

        public static ThemeConfig CreateDark()
        {
            return new ThemeConfig { Algorithm = ThemeAlgorithm.Dark };
        }

        public static ThemeConfig Create(Action<SeedToken> configure, ThemeAlgorithm algorithm = ThemeAlgorithm.Default)
        {
            var config = new ThemeConfig { Algorithm = algorithm };
            if (configure != null) configure(config.Seed);
            return config;
        }
    }
}
