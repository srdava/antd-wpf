# Antd.Wpf

> Ant Design for WPF · runs on **.NET Framework 4.8** and **.NET 8** · zero third-party dependencies
>
> [中文](README.md) | English
>
> **GitHub (primary)** · https://github.com/srdava/antd-wpf ｜ **Gitee (mirror)** · https://gitee.com/davva/antd.-wpf.-tokens
>
> Issues and pull requests live on GitHub; Gitee is a synced mirror for mainland China (the 1.0.0 release assets currently sit in Gitee Releases).

[![CI](https://github.com/srdava/antd-wpf/actions/workflows/ci.yml/badge.svg)](https://github.com/srdava/antd-wpf/actions/workflows/ci.yml)

[Handbook](docs/manual.en.md) · [Component site](docs/site/index.html) · [Migration](docs/migration.md) · [Capability notes](docs/progress.md) · [Overview](docs/intro.md)

---

## What it is

Ant Design's **token computation ported to C#**, so WPF controls read colours and metrics from the same source of truth. Switching a theme replaces one immutable `ThemeSnapshot`; every brush comes from a frozen cache.

| Item | Value |
|---|---|
| Frameworks | `net48`, `net8.0-windows` |
| Components | **69 of 71** planned (base capability — see the notes below) |
| Design tokens | **536**, value-compared against antd's `theme.getDesignToken()` across 10 configurations |
| Icons | **847** (outlined / filled / two-tone), lazily parsed and cached |
| Assertions | 347 component + 76 feedback + 28 real-page checks, on both TFMs |
| Accessibility | AutomationPeer on all 92 constructible controls, shared focus ring, keyboard paths |

## Install

```bash
dotnet add package Antd.Wpf.Controls --version 1.1.0
```

`Antd.Wpf.Controls` brings `Antd.Wpf.Theme` / `Antd.Wpf.Icons` / `Antd.Wpf.Tokens` along.

```xml
<!-- App.xaml -->
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

```csharp
// App.xaml.cs
ThemeManager.Current.Attach(Application.Current.Resources);
ThemeManager.Current.Apply(ThemeConfig.CreateDefault());

AntdLocale.UseCulture("zh-CN");   // optional: Chinese strings
AntdMotion.UseSystemDefault();    // optional: follow the OS motion setting
```

```xml
<Window xmlns:antd="clr-namespace:Antd.Wpf.Controls;assembly=Antd.Wpf.Controls">
    <StackPanel>
        <antd:Button Content="Primary" Variant="Primary" />
        <antd:Input Width="240" Placeholder="Search" AllowClear="True" />
        <antd:Select Width="240" Filterable="True" IsMultiple="True" MaxTagCount="3" />
    </StackPanel>
</Window>
```

## Documentation

| Document | Contents |
|---|---|
| **[Handbook (English)](docs/manual.en.md)** | Framework usage plus **per-component usage and full property tables for 74 components** (tables are reflected from the assembly, so they cannot go stale) |
| [使用手册（中文）](docs/manual.zh.md) | Chinese mirror of the handbook |
| [Component site](docs/site/index.html) | Single-file HTML with property tables and capability notes |
| [Migration guide](docs/migration.md) | Coming from antd / AntDesign.WPF / 0.x, plus seven pitfalls |
| [Capability notes](docs/progress.md) | Per-component "done / not done" — **read before adopting** |
| [Overview](docs/intro.md) | Positioning, strengths, who should not use it |
| [Visual parity](docs/visual-parity.md) | Measured size/colour comparison with antd's own rendering |

## Theming

```csharp
ThemeManager.Current.ApplyLight();
ThemeManager.Current.ApplyDark();
ThemeManager.Current.UpdateSeed(s => s.ColorPrimary = AntdColor.Parse("#00b96b"));
ThemeManager.Current.Apply(ThemeConfig.Create(null, ThemeAlgorithm.Dark | ThemeAlgorithm.Compact));
```

Reference tokens in XAML (`{DynamicResource Antd.colorPrimary}`); `StaticResource` will not follow a switch.

## Highlights

| Area | What works |
|---|---|
| **Select** | Single / multiple with `+N` tag folding, local search, remote search (`FilterLocally=false`), virtualisation |
| **DatePicker** | Single date, two-month range, `DisabledDate` rules |
| **Table** | Frozen columns, expandable rows, tree rows (`TreeChildrenPath`) |
| **Form** | Sync `Validator` plus `AsyncValidator` / `ValidateAsync()` |
| **Modal** | Declarative and imperative `Modal.Confirm / Info / Warning / Error` |
| **Upload** | Queue, concurrency cap, progress and cancel — transport supplied by the host |
| **Accessibility** | Focus ring; Collapse headers, menu nodes, rating and the upload zone are keyboard operable |
| **Localisation** | `AntdLocale` switches Chinese/English; date formats follow `CultureInfo` |
| **Reduced motion** | `AntdMotion` follows the OS by default; dialogs, dropdowns and carousels stop |

**Not implemented yet**: editable table cells, cross-field form validation, grouped Select options, week/month DatePicker views, RTL, high-contrast themes, true two-colour two-tone icons. See [capability notes](docs/progress.md) for the full list.

## Development

```bash
dotnet build Antd.Wpf.sln                                  # both target frameworks

# Assertions (no test framework involved)
dotnet run --project tests/Antd.Wpf.TokenParity -f net8.0
dotnet run --project tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --shot artifacts
dotnet run --project tests/Antd.Wpf.FeedbackTests -f net8.0-windows
dotnet run --project demo/Antd.Wpf.Integration -f net8.0-windows -- --selftest

# Regenerate the component site and both handbooks
dotnet run --project tools/docs-gen

# Pack and prove the nupkg installs in a real project
dotnet pack src/Antd.Wpf.Controls -c Release -o artifacts/packages
dotnet run --project tools/package-check
```

## Layout

```
src/Antd.Wpf.Tokens     netstandard2.0   Seed → Map → Alias, no WPF dependency
src/Antd.Wpf.Theme      net48;net8       ThemeManager / ThemeSnapshot / brush cache / XAML helpers
src/Antd.Wpf.Icons      net48;net8       847 icons as lazily parsed Geometry
src/Antd.Wpf.Controls   net48;net8       69 components + Generic.xaml
demo/Gallery                             Visual catalogue of every control
demo/Integration                         Real page with 28 self-checks
tests/                                   Three assertion programs, zero dependencies
tools/token-sync                         Node: exports golden tokens from antd
tools/icon-gen                           Node: builds Geometry from @ant-design/icons-svg
tools/docs-gen                           net8: reflects the assembly into docs and handbooks
tools/package-check                      Consumer project: proves the published packages work
```

## Publishing

```bash
dotnet pack src/Antd.Wpf.Controls -c Release -o artifacts/packages   # same for the other three
dotnet nuget push "artifacts/packages/*.nupkg" -k $env:NUGET_API_KEY -s https://api.nuget.org/v3/index.json
```

1.0.0 is live on nuget.org ([Controls](https://www.nuget.org/packages/Antd.Wpf.Controls) · [Theme](https://www.nuget.org/packages/Antd.Wpf.Theme) · [Icons](https://www.nuget.org/packages/Antd.Wpf.Icons) · [Tokens](https://www.nuget.org/packages/Antd.Wpf.Tokens)); release assets with SHA-256 are on [Gitee Releases](https://gitee.com/davva/antd.-wpf.-tokens/releases/tag/v1.0.0).

## License

[MIT](LICENSE). Declared in the package as `<license type="expression">MIT</license>`.
