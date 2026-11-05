using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Antd.Wpf.Tokens;
using Golden = Antd.Wpf.Tests.Generated.GoldenTokens;

namespace Antd.Wpf.TokenParity
{
    /// <summary>
    /// Compares the C# token derivation against antd's own theme.getDesignToken() output.
    /// This is the guard rail described in docs/antd-wpf-implementation-plan.md section 5.4.
    /// </summary>
    internal static class Program
    {
        private static int Main()
        {
            Console.WriteLine("antd golden version: " + Golden.AntdVersion);
            Console.WriteLine();

            var cases = new List<Case>
            {
                new Case("default", () => TokenBuilder.Build(new SeedToken(), ThemeAlgorithm.Default)),
                new Case("dark", () => TokenBuilder.Build(new SeedToken(), ThemeAlgorithm.Dark)),
                new Case("customPrimary", BuildCustom(s => s.ColorPrimary = AntdColor.Parse("#00b96b"), ThemeAlgorithm.Default)),
                new Case("customRadius8", BuildCustom(s => s.BorderRadius = 8, ThemeAlgorithm.Default)),
                new Case("customFontSize16", BuildCustom(s => s.FontSize = 16, ThemeAlgorithm.Default)),
                new Case("customBgBase", BuildCustom(s => s.ColorBgBase = AntdColor.Parse("#123456"), ThemeAlgorithm.Default)),
                new Case("customTextBase", BuildCustom(s => s.ColorTextBase = AntdColor.Parse("#333333"), ThemeAlgorithm.Default)),
                new Case("compact", () => TokenBuilder.Build(new SeedToken(), ThemeAlgorithm.Compact)),
                new Case("darkCompact", () => TokenBuilder.Build(new SeedToken(), ThemeAlgorithm.Dark | ThemeAlgorithm.Compact)),
                new Case("darkPurple", BuildCustom(s => s.ColorPrimary = AntdColor.Parse("#9254de"), ThemeAlgorithm.Dark)),
            };

            int totalFailures = 0;
            var allMissing = new SortedDictionary<string, int>(StringComparer.Ordinal);

            // Missing tokens are failures, not warnings. Keep this list empty: a key only belongs
            // here if antd emits something this port deliberately cannot derive, and that deserves
            // a comment explaining why.
            var allowedMissing = new HashSet<string>(StringComparer.Ordinal);

            foreach (Case testCase in cases)
            {
                Dictionary<string, string> golden = Golden.Configs[testCase.Name];
                AliasToken actual = testCase.Build();

                int matched = 0;
                int compared = 0;
                var failures = new List<string>();

                foreach (KeyValuePair<string, string> pair in golden)
                {
                    object ours;
                    if (!actual.TryGet(pair.Key, out ours))
                    {
                        int count;
                        allMissing.TryGetValue(pair.Key, out count);
                        allMissing[pair.Key] = count + 1;

                        if (!allowedMissing.Contains(pair.Key))
                        {
                            totalFailures++;
                            failures.Add("  " + pair.Key.PadRight(26) + " MISSING  antd emits it, this port does not");
                        }
                        continue;
                    }

                    compared++;
                    string problem = Compare(pair.Value, ours);
                    if (problem == null)
                    {
                        matched++;
                    }
                    else
                    {
                        failures.Add("  " + pair.Key.PadRight(26) + " golden=" + pair.Value + "  actual=" + Format(ours) + "  (" + problem + ")");
                    }
                }

                Console.WriteLine("[" + testCase.Name + "] matched " + matched + "/" + golden.Count
                    + "  (golden keys: " + golden.Count + ", produced: " + actual.Values.Count + ")");

                if (failures.Count > 0)
                {
                    totalFailures += failures.Count;
                    int shown = Math.Min(failures.Count, 25);
                    for (int i = 0; i < shown; i++) Console.WriteLine(failures[i]);
                    if (failures.Count > shown) Console.WriteLine("  ... " + (failures.Count - shown) + " more");
                }
            }

            if (allMissing.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Tokens present in antd but not produced by this port: " + allMissing.Count);
                int shown = 0;
                foreach (string key in allMissing.Keys)
                {
                    if (shown++ >= 40) { Console.WriteLine("  ..."); break; }
                    Console.WriteLine("  - " + key);
                }
            }

            Console.WriteLine();
            Console.WriteLine(totalFailures == 0 ? "RESULT: PASS" : "RESULT: FAIL (" + totalFailures + " mismatches)");
            return totalFailures == 0 ? 0 : 1;
        }

        private sealed class Case
        {
            public Case(string name, Func<AliasToken> build)
            {
                Name = name;
                Build = build;
            }

            public string Name { get; private set; }
            public Func<AliasToken> Build { get; private set; }
        }

        private static Func<AliasToken> BuildCustom(Action<SeedToken> configure, ThemeAlgorithm algorithm)
        {
            return () =>
            {
                var seed = new SeedToken();
                configure(seed);
                return TokenBuilder.Build(seed, algorithm);
            };
        }

        private static string Format(object value)
        {
            if (value is AntdColor) return ((AntdColor)value).ToString();
            if (value is double) return ((double)value).ToString("R", CultureInfo.InvariantCulture);
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static string Compare(string golden, object ours)
        {
            if (ours is AntdColor)
            {
                AntdColor expected;
                if (!AntdColor.TryParse(golden, out expected)) return "golden is not a color";
                AntdColor actual = (AntdColor)ours;
                return actual.Equals(expected) ? null : "color mismatch";
            }

            if (ours is bool)
            {
                string expectedText = Convert.ToString(ours is bool && (bool)ours, CultureInfo.InvariantCulture);
                bool goldenBool;
                if (!bool.TryParse(golden, out goldenBool)) return "golden is not a bool";
                return goldenBool == (bool)ours ? null : "expected " + expectedText;
            }

            if (ours is double)
            {
                double expectedValue;
                if (!double.TryParse(golden, NumberStyles.Float, CultureInfo.InvariantCulture, out expectedValue))
                {
                    return "golden is not numeric";
                }
                double actualValue = (double)ours;
                if (Math.Abs(expectedValue - actualValue) < 0.000001) return null;
                return "expected " + expectedValue.ToString("R", CultureInfo.InvariantCulture);
            }

            string expectedString = Normalize(golden);
            string actualString = Normalize(Convert.ToString(ours, CultureInfo.InvariantCulture));
            return string.Equals(expectedString, actualString, StringComparison.Ordinal) ? null : "string mismatch";
        }

        private static readonly Regex NumberPattern = new Regex(@"\d+\.\d+", RegexOptions.Compiled);
        private static readonly Regex WhitespacePattern = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Collapses whitespace and rounds float noise (antd emits values such as
        /// 0.010000000000000002 because of double arithmetic in JS).
        /// </summary>
        private static string Normalize(string value)
        {
            if (value == null) return string.Empty;
            string compact = WhitespacePattern.Replace(value, " ").Trim();
            compact = NumberPattern.Replace(compact, delegate(Match m)
            {
                double number = double.Parse(m.Value, CultureInfo.InvariantCulture);
                return Math.Round(number, 4).ToString("0.####", CultureInfo.InvariantCulture);
            });
            return compact;
        }
    }
}
