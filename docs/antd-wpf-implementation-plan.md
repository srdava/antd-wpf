# Ant Design → WPF 实现计划（v0.1 草案）

| 项目 | 内容 |
|---|---|
| 文档编号 | ANTD-WPF-PLAN-001 |
| 版本 | v0.1（草案，待评审） |
| 日期 | 2026-11-03 |
| 分析对象 | https://ant.design/ ／ https://github.com/ant-design/ant-design |
| 目标产物 | 遵循 Ant Design 规范的 WPF 控件库，可同时运行于 .NET Framework 4.8 及以上 与 .NET 8 及以上 |
| 分析方法 | REA（Reverse Engineer Anything）：以官方文档与仓库源码为一手证据，先取事实、再做差距判定、最后出计划 |
| 实施状态 | **M0 已完成**（2026-11-03）。双目标工程 + 主题引擎 + Button/Input 落地；536 个 token × 8 组配置与 antd 6.6.8 逐值对拍通过。详见 `docs/m0-implementation-notes.md` |

> 阅读顺序建议：第 1 章看结论，第 2~3 章看边界，第 5 章看核心难点（主题引擎），第 7 章看排期。

---

## 1. REA 分析结论

### 1.1 证据等级标记

本文所有事实按可信度标注，避免把推断当结论：

| 标记 | 含义 |
|---|---|
| **E1** | 官方文档原文或仓库源码原文，可直接引用 |
| **E2** | 二手资料（社区文章、同类项目 README），需交叉验证 |
| **E3** | 本文推断，实施前必须验证 |

### 1.2 关键事实清单

| # | 事实 | 证据 | 等级 |
|---|---|---|---|
| F1 | 当前主线版本为 **antd v6**，v6 起默认使用 **CSS 变量**、不再支持 IE、要求 React ≥ 18 | ant.design/docs/react/migration-v6 | E1 |
| F2 | v6 引入 `zeroRuntime` 模式，可关闭运行时样式生成，改用静态 CSS | customize-theme 文档「Zero Runtime」 | E1 |
| F3 | 组件总览共 **73 项**（含已废弃的 `List`、6.6.0 新增的 `Listy`、6.4.0 的 `BorderBeam`），另有 6 个 Pro 系列组件不属于 antd 本体 | ant.design/components/overview | E1 |
| F4 | 主题体系为三层 Token：**Seed → Map → Alias**，由 `theme.algorithm` 展开 | customize-theme 文档 | E1 |
| F5 | 三种算法：`defaultAlgorithm`、`darkAlgorithm`、`compactAlgorithm`，可组合 | 同上 | E1 |
| F6 | 色板由 `@ant-design/colors` 的 `generate()` 以 **HSV 阶梯**生成 10 阶 | ant-design-colors/src/generate.ts 源码 | E1 |
| F7 | Seed 默认值：`colorPrimary #1677ff`、`borderRadius 6`、`controlHeight 32`、`fontSize 14`、`lineWidth 1`、`sizeUnit/sizeStep 4` | components/theme/themes/seed.ts | E1 |
| F8 | 中性色由 `getAlphaColor(textBase, alpha)` / `getSolidColor(bgBase, alpha)` 生成，浅色主题 `colorText` alpha=0.88，深色 0.85 | themes/default/colors.ts、themes/dark/colors.ts | E1 |
| F9 | 字号序列由 `base * e^(i/5)` 生成并取偶；行高 = `(fontSize + 8) / fontSize` | shared/genFontSizes.ts | E1 |
| F10 | `controlHeightSM = 0.75×`、`controlHeightXS = 0.5×`、`controlHeightLG = 1.25×` | shared/genControlHeight.ts | E1 |
| F11 | 栅格为 **24 栅格**，断点 xs 480 / sm 576 / md 768 / lg 992 / xl 1200 / xxl 1600 / xxxl 1920（v6.3 新增 xxxl） | components/grid | E1 |
| F12 | 动效时长 token：`motionDurationFast 0.1s` / `Mid 0.2s` / `Slow 0.3s`；缓动为 `cubic-bezier(...)` | customize-theme 文档 | E1 |
| F13 | v6 将样式定制 API 统一为 `classNames` / `styles` 语义槽，替代 `bodyStyle`/`overlayClassName` 等散装属性 | migration-v6 | E1 |
| F14 | 社区已有 `AntDesign.WPF`（30+ 控件，**要求 .NET 8.0+**）与若干早期的 `antd-wpf`（2019 年、已停滞） | github 搜索结果 | E2 |
| F15 | 现有 WPF 实现普遍只支持 .NET 8+，**net48 支持是明确的市场空白** | 由 F14 推断 | E3 |

### 1.3 事实对移植的影响判定

| 事实 | 对 WPF 的影响 | 判定 |
|---|---|---|
| F1 CSS 变量 | WPF 无 CSS 级联，需自建「Token 快照 + 绑定」分发机制替代 | **核心难点**，见 §5.3 |
| F2 zeroRuntime | 与 WPF 天然契合：WPF 本就是「编译期模板 + 运行期换主题」，可视为对标能力 | 有利 |
| F3 73 组件 | 全量移植约为 300+ 人日量级，必须分期裁剪 | 决定排期，见 §7 |
| F4/F5 三层 Token | 可直接 1:1 建模为 C# 类；算法可纯函数化、无 UI 依赖 | 有利，可先做 |
| F6 HSV 色板 | 算法可逐行移植，但 **JS 与 C# 的舍入规则不同**（见 §5.2.3），否则 Token 值会差 1 | 必须逐值对拍 |
| F7/F8/F9/F10 数值 | 全部为确定性公式，无平台依赖 | 有利 |
| F11 24 栅格 + 断点 | WPF 无 media query，需监听容器宽度（`SizeChanged`）自行计算断点 | 可控 |
| F12 cubic-bezier | WPF `KeySpline` **就是**三次贝塞尔，控制点语义与 CSS 一致 | 有利，1:1 映射 |
| F13 classNames/styles 语义槽 | WPF 用 `TemplatePart` + 具名依赖属性承接；需先定义语义槽映射规范 | 需新增规范，见 §6.6 |
| F15 net48 空白 | 本项目差异化价值所在 | 立项目标 |

---

## 2. 移植差距分析（Gap Matrix）

WPF 与浏览器是两套渲染模型，逐项判定差距与对策。**差距等级**：A 可直接映射 / B 需适配层 / C 需重建。

| # | Web（antd）能力 | antd 用法 | WPF 对应 | 等级 | 对策 |
|---|---|---|---|---|---|
| G1 | CSS 变量 / 级联 | 全部 token | 资源字典 + 依赖属性 | C | 见 §5.3 主题引擎 |
| G2 | CSS-in-JS 动态样式 | 运行时算样式 | `Style`/`ControlTemplate` 静态 + token 绑定 | B | 模板内只用 token 绑定，不写死数值 |
| G3 | `:hover` / `:focus-visible` / `:disabled` | 状态样式 | `Trigger`（`IsMouseOver`/`IsKeyboardFocusWithin`/`IsEnabled`） | A | 统一状态触发器宏 |
| G4 | Flex 布局 | Row/Col/Space/Flex | `Grid`/`StackPanel`/`DockPanel`/自研 `FlexPanel` | B | 实现轻量 `FlexPanel`（含 `flex-grow/shrink/basis`） |
| G5 | media query | 响应式栅格 | `SizeChanged` + 断点计算 | B | `BreakpointListener` 附加行为 |
| G6 | `position: fixed/sticky` | Affix、固定表头 | `ScrollViewer` 内 `Canvas` 位移 / 双 ScrollViewer | B | 按组件单独实现 |
| G7 | Portal / z-index | Modal、Dropdown、Message | `Popup` / 顶层 `Canvas` 层 / 无边框 `Window` | C | 统一 `OverlayHost`，见 §8.1 |
| G8 | SVG 图标 | `@ant-design/icons` | `Geometry` 资源 | B | 构建期 codegen，见 §6.5 |
| G9 | `line-height` | 全局 22/14=1.5714 | `TextBlock.LineHeight`（**绝对像素**，非倍数） | B | 乘算后写入，见 §6.2 |
| G10 | 多层 `box-shadow` | 卡片/浮层 | `DropShadowEffect` 仅单层 | B | 阴影栈降级策略，见 §6.3 |
| G11 | `rem` / `px` | 全部 | DIP（96 DPI 逻辑像素） | A | 1px = 1 DIP |
| G12 | 文本垂直居中 | flex `align-items:center` | `VerticalAlignment` + `LineHeight` 差异 | B | 控件模板统一用 `Grid` 行居中，不用 `TextBlock` 自带行高居中 |
| G13 | `@media (prefers-reduced-motion)` | 动效降级 | 系统设置读取 | B | 读注册表 `SPI_GETCLIENTAREAANIMATION` |
| G14 | DOM 结构定制 | v6 `classNames`/`styles` | `ControlTemplate` + Part | B | 定义语义槽映射表 |
| G15 | `:root` 全局字号缩放 | `fontSize` 15/16 | 全局 `TextElement.FontSize` 继承 | A | 通过 `ThemeSnapshot` 生效 |

**结论**：A 级 3 项、B 级 10 项、C 级 2 项。C 级两项（主题引擎、浮层系统）是全部风险所在，必须在 M0 阶段先做出可运行的原型，再铺组件。

---

## 3. 技术基线

### 3.1 目标框架

| 目标 | TFM | 说明 |
|---|---|---|
| .NET Framework | `net48` | 覆盖 4.8 与 4.8.1（4.8.1 复用 net48 资产） |
| .NET | `net8.0-windows` | .NET 8 / 9 / 10 均可引用该资产（向下兼容） |

**关键约束（必须写进 CONTRIBUTING）：**

1. **WPF 类库不能 target `netstandard2.0`**。WPF 不在 .NET Standard 契约内，必须用平台 TFM。这是本项目不能采用「一份 netstandard 核心 + 多平台外壳」的原因。
2. **语言版本取交集 `C# 7.3`**。禁用：`record`、`init`、可空引用类型、`文件作用域命名空间`、`target-typed new`、`using` 声明式、模式匹配增强（`is not`）、`switch` 表达式。
3. **API 取交集**。禁用：`Span<T>`、`Math.Clamp`、`System.HashCode`、`string.Contains(char)`、索引/范围、`IAsyncEnumerable`、`System.Text.Json`、`ValueTask`。
   - 自己写 `AntdMath.Clamp`、`AntdHash.Combine` 作为替代。
4. **消费者若运行于 .NET 9/10 WPF**，注意 .NET 9 引入了 Fluent 主题与 `Application.ThemeMode`；本项目所有控件必须给出**显式** `Style`（`TargetType` + `x:Key` 且 `BasedOn` 为空），避免被隐式 Fluent 主题污染。

### 3.2 打包划分

| 程序集 | TFM | 职责 | 依赖 |
|---|---|---|---|
| `Antd.Wpf.Tokens` | **netstandard2.0** | Seed/Map/Alias Token 模型、算法、色板计算。**不含 UI 代码** | 无（不引用任何 WPF 程序集） |
| `Antd.Wpf.Theme` | net48;net8.0-windows | `ThemeManager`、`ThemeSnapshot`、主题扩展、`ThemeScope` | Tokens |
| `Antd.Wpf.Icons` | net48;net8.0-windows | 图标 `Geometry` / `DrawingImage` 资源 + 生成器产出的 `.g.cs` | 无 |
| `Antd.Wpf.Controls` | net48;net8.0-windows | 控件、样式、模板、行为、浮层宿主 | 上三者 |
| `Antd.Wpf.Gallery` | net48;net8.0-windows | 组件演示站（对标 ant.design 文档站） | Controls |
| `Antd.Wpf.Tests` | net48;net8.0 / net48;net8.0-windows | Token 对拍、模板冒烟、截图基准 | 全部 |

> Tokens 实际落地为 `netstandard2.0`（M0 决策 D1）：它是纯计算库，不该绑平台。单一资产同时服务 net48 与 net8+，
> 少一个构建矩阵维度，详见 `docs/m0-implementation-notes.md` §3。

> **命名说明**：社区已有 NuGet `AntDesign.WPF`（F14），为避免包名冲突与用户混淆，本项目使用 `Antd.Wpf.*`。最终名称待评审确认（见 §13 Q1）。

### 3.3 目录结构

```
Antd_WPF_DS/
├─ src/
│  ├─ Antd.Wpf.Tokens/
│  │  ├─ Seed/SeedToken.cs
│  │  ├─ Map/MapToken.cs
│  │  ├─ Alias/AliasToken.cs
│  │  ├─ Algorithms/DefaultAlgorithm.cs
│  │  ├─ Algorithms/DarkAlgorithm.cs
│  │  ├─ Algorithms/CompactAlgorithm.cs
│  │  └─ Color/{Hsv.cs, PaletteGenerator.cs, ColorMixer.cs}
│  ├─ Antd.Wpf.Theme/
│  │  ├─ ThemeManager.cs
│  │  ├─ ThemeSnapshot.cs
│  │  ├─ ThemeConfig.cs
│  │  ├─ ThemeScope.cs
│  │  ├─ Markup/TokenExtension.cs
│  │  └─ Brushes/BrushCache.cs
│  ├─ Antd.Wpf.Icons/
│  ├─ Antd.Wpf.Controls/
│  │  ├─ General/  Layout/  Navigation/
│  │  ├─ DataEntry/  DataDisplay/  Feedback/
│  │  ├─ Internal/{OverlayHost, FlexPanel, ShadowStack, RippleHost}
│  │  └─ Themes/{Generic.xaml, Light.xaml, Dark.xaml}
├─ demo/Antd.Wpf.Gallery/
├─ tests/Antd.Wpf.Tests/
├─ tools/
│  ├─ icon-gen/        (Node: SVG → Geometry 资源)
│  └─ token-sync/      (Node: 从 antd 源码导出 token 快照 JSON)
├─ docs/
└─ Antd.Wpf.sln
```

---

## 4. 架构设计

### 4.1 分层

```
┌──────────────────────────────────────────────────────────┐
│  Gallery / 业务应用                                        │
└───────────────▲──────────────────────────────────────────┘
                │ 使用控件 + ConfigProvider 附加属性
┌───────────────┴──────────────────────────────────────────┐
│  Antd.Wpf.Controls                                        │
│  ┌────────────┬────────────┬────────────┬─────────────┐  │
│  │ 组件 Style │ 行为/交互  │ OverlayHost│ Motion/焦点  │  │
│  └────────────┴────────────┴────────────┴─────────────┘  │
└───────────────▲──────────────────────────────────────────┘
                │ 读取 token（绑定到 ThemeSnapshot）
┌───────────────┴──────────────────────────────────────────┐
│  Antd.Wpf.Theme   ThemeManager / ThemeSnapshot / 作用域    │
└───────────────▲──────────────────────────────────────────┘
                │ 纯计算：seed → map → alias
┌───────────────┴──────────────────────────────────────────┐
│  Antd.Wpf.Tokens  （无 UI 依赖，可单元测试、可离线预计算）  │
└──────────────────────────────────────────────────────────┘
```

**依赖规则（由架构测试强制）：**

- `Tokens` 不得引用任何 `System.Windows.Controls`。
- `Theme` 不得引用 `Controls`。
- `Controls` 不得直接 `new SolidColorBrush(...)` 写死颜色，必须走 token（CI 用 Roslyn 分析器校验）。
- 任何 token 常量不得出现在 XAML 字面量里（除 0/1 等结构性数值）。

### 4.2 与 antd 的概念映射表

| antd | 本项目 | 说明 |
|---|---|---|
| `ConfigProvider` | `Theme.ConfigProvider` 附加属性（挂在 `Application`/`Window`/任意元素） | 支持嵌套，子级继承未覆盖项 |
| `theme.useToken()` | `ThemeManager.Current.Snapshot` | 不可变快照 |
| `theme.getDesignToken(config)` | `ThemeManager.Build(config)` | 纯函数，可离线 |
| `theme.algorithm` | `ThemeConfig.Algorithm`（枚举/委托） | default / dark / compact，可组合 |
| `components.Button.token` | `ThemeConfig.Components["Button"]` | 组件级覆盖表 |
| `App.useApp()` | `AntdApp.Current.Message/Notification/Modal` | 静态服务入口 |
| `classNames` / `styles` | `ControlTemplate` Part 名 + 具名依赖属性 | 见 §6.6 |
| `componentSize` | `ConfigProvider.ComponentSize`（继承附加属性） | small/middle/large |
| `direction="rtl"` | `FlowDirection` + 几何镜像 | P2 阶段 |

---

## 5. 主题引擎（核心难点）

### 5.1 三层 Token 模型

以 `sealed class` 而非 `Dictionary<string, object>` 建模，原因：编译期检查、避免装箱与字符串查找（表格滚动时 token 访问量可达十万次/秒）。

```csharp
// Antd.Wpf.Tokens/Seed/SeedToken.cs
public sealed class SeedToken
{
    public Color ColorPrimary { get; set; }   // #1677ff
    public Color ColorSuccess { get; set; }   // #52c41a
    public Color ColorWarning { get; set; }   // #faad14
    public Color ColorError   { get; set; }   // #ff4d4f
    public Color ColorInfo    { get; set; }   // #1677ff
    public Color? ColorLink   { get; set; }   // 空 = 取 ColorInfo
    public Color? ColorTextBase { get; set; } // 空 = 按算法取 #000 / #fff
    public Color? ColorBgBase   { get; set; } // 空 = 按算法取 #fff / #000

    public double FontSize { get; set; } = 14;
    public double BorderRadius { get; set; } = 6;
    public double LineWidth { get; set; } = 1;
    public double ControlHeight { get; set; } = 32;
    public double SizeUnit { get; set; } = 4;
    public double SizeStep { get; set; } = 4;
    public bool Motion { get; set; } = true;

    // 13 个预设色板（blue/purple/cyan/green/magenta/pink/red/orange/yellow/volcano/geekblue/gold/lime）
    public Dictionary<string, Color> PresetColors { get; set; }
}
```

`MapToken` / `AliasToken` 同构，但含 200+ 只读属性，全部由算法计算得出，构造后 `Freeze()`（自定义不可变语义）。

### 5.2 派生管线（按源码逐行移植）

```
SeedToken
  ├─ PaletteGenerator.Generate(每个语义色)        → palette[1..10]
  ├─ NeutralPalette.Generate(bgBase, textBase)    → colorText/Fill/Bg/Border
  ├─ FontScale.Generate(fontSize)                 → fontSize*/lineHeight*
  ├─ SizeScale.Generate(sizeUnit, sizeStep)       → sizeXXS..sizeXXL
  ├─ ControlHeightScale.Generate(controlHeight)   → controlHeightXS/SM/LG
  └─ CommonMap.Generate(...)                      → boxShadow / motionDuration*
        ↓
     MapToken
        ↓（别名 + 语义槽）
     AliasToken  →  ThemeSnapshot
```

#### 5.2.1 色板生成（对应 `ant-design-colors/src/generate.ts`）

源码级常量（E1，数值必须原样保留）：

```csharp
// Antd.Wpf.Tokens/Color/PaletteGenerator.cs
internal static class PaletteGenerator
{
    private const int    HueStep          = 2;
    private const double SaturationStep   = 0.16;
    private const double SaturationStep2  = 0.05;
    private const double BrightnessStep1  = 0.05;
    private const double BrightnessStep2  = 0.15;
    private const int    LightColorCount  = 5;
    private const int    DarkColorCount   = 4;

    // 暗色主题映射表：{ 源索引, 混合百分比 }
    private static readonly int[,] DarkColorMap =
    {
        {7,15},{6,25},{5,30},{5,45},{5,65},
        {5,85},{4,90},{3,95},{2,97},{1,98}
    };

    /// <summary>返回 10 阶色板，index 0 对应 antd 的 palette[1]。</summary>
    public static Color[] Generate(Color seed, bool dark, Color darkBackground)
    {
        Hsv hsv = Hsv.From(seed);
        var raw = new List<Color>(10);

        for (int i = LightColorCount; i > 0; i--)
            raw.Add(Hsv.ToColor(GetHue(hsv, i, true), GetSaturation(hsv, i, true), GetValue(hsv, i, true)));
        raw.Add(seed);
        for (int i = 1; i <= DarkColorCount; i++)
            raw.Add(Hsv.ToColor(GetHue(hsv, i, false), GetSaturation(hsv, i, false), GetValue(hsv, i, false)));

        if (!dark) return raw.ToArray();

        var result = new Color[10];
        for (int k = 0; k < 10; k++)
            result[k] = ColorMixer.Mix(darkBackground, raw[DarkColorMap[k, 0]], DarkColorMap[k, 1]);
        return result;
    }
}
```

`GetHue` / `GetSaturation` / `GetValue` 三函数需完整移植其边界修正：

- `GetHue`：`60 <= h <= 240` 时浅色向负方向、深色向正方向旋转，否则反向；结果对 360 取模。
- `GetSaturation`：灰色（`h==0 && s==0`）不改饱和；浅色 `s - 0.16*i`；最深一阶 `s + 0.16`；其余 `s + 0.05*i`；上限 1；**浅色第 5 阶若 > 0.1 则强制 0.1**；下限 0.06；最后 `Round2(s)`。
- `GetValue`：浅色 `v + 0.05*i`，深色 `v - 0.15*i`，Clamp 到 [0,1]，`Round2`。

#### 5.2.2 语义色索引映射（对应 `themes/default|dark/colors.ts`）

`generateColorPalettes` 的映射差异是浅色/深色主题观感差异的关键，**必须按表实现**（E1）：

| Token | 浅色取 palette | 深色取 palette |
|---|---|---|
| `colorPrimaryBg` | 1 | 3 |
| `colorPrimaryBgHover` | 2 | 4 |
| `colorPrimaryBorder` | 3 | 5 |
| `colorPrimaryBorderHover` | 4 | 6 |
| `colorPrimaryHover` | 5 | 7 |
| `colorPrimary`（本体） | 6 | 6 |
| `colorPrimaryActive` | 7 | 5 |
| `colorPrimaryTextHover` | 8 | 8（=6） |
| `colorPrimaryText` | 9 | 9（=5） |
| `colorPrimaryTextActive` | 10 | 10（=4） |

（`success` / `warning` / `error` / `info` / `link` 同构，仅 `error` 多一个 `colorErrorBgFilledHover = mix(palette[1], palette[3], 50%)`。）

深色算法另有覆盖：`colorPrimaryBg = colorPrimaryBorder`、`colorPrimaryBgHover = colorPrimaryBorderHover`（源码显式注释，用于修正选中项背景过亮问题）。

#### 5.2.3 中性色 alpha 表（E1，逐值照抄）

| Token | 浅色 | 深色 |
|---|---|---|
| `colorText` | `alpha(textBase, 0.88)` | `0.85` |
| `colorTextSecondary` | `0.65` | `0.65` |
| `colorTextTertiary` | `0.45` | `0.45` |
| `colorTextQuaternary` | `0.25` | `0.25` |
| `colorFill` | `0.15` | `0.18` |
| `colorFillSecondary` | `0.06` | `0.12` |
| `colorFillTertiary` | `0.04` | `0.08` |
| `colorFillQuaternary` | `0.02` | `0.04` |
| `colorBgLayout` | `solid(bgBase, 4)` | `solid(bgBase, 0)` |
| `colorBgContainer` | `solid(bgBase, 0)` | `solid(bgBase, 8)` |
| `colorBgElevated` | `solid(bgBase, 0)` | `solid(bgBase, 12)` |
| `colorBorder` | `solid(bgBase, 15)` | `solid(bgBase, 26)` |
| `colorBorderSecondary` | `solid(bgBase, 6)` | `solid(bgBase, 19)` |
| 默认 `colorBgBase` | `#fff` | `#000` |
| 默认 `colorTextBase` | `#000` | `#fff` |
| `colorBgMask` | `rgba(0,0,0,0.45)` | 同 |

**三处必须警惕的移植陷阱：**

1. **舍入规则不同**。JS `Math.round(x)` 为「四舍五入（.5 向上）」，C# `Math.Round(x)` 默认「银行家舍入（.5 取偶）」。`2.5` 在 JS 得 3、在 C# 得 2。所有 `Math.round` 必须替换为 `AntdMath.RoundJs(x) => Math.Floor(x + 0.5)`。涉及 `getSaturation`/`getValue` 的 `Round2`、`fontHeight`、`fontSize` 取整。**这是最容易造成「颜色差一档、字号差 2px」的地方。**
2. `getAlphaColor(base, alpha)` **不是**简单地给 base 加 alpha，而是「按 alpha 与背景混合后输出带 alpha 的 rgba」；`getSolidColor(base, alpha)` 输出**不透明**的实心色。两者不可混用，否则暗色主题会出现「文字发灰变透明叠加」的错误。
3. 深色主题的 `colorShadow` 默认是 `rgba(255,255,255,0.2)`，与浅色主题的 `#000` 不同——阴影叠层实现需按主题分支。

#### 5.2.4 字号与行高（E1）

```csharp
// 源码：genFontSizes.ts
static double GetLineHeight(double fontSize) => (fontSize + 8) / fontSize;

static FontSizePair[] GetFontSizes(double baseSize)
{
    var pairs = new FontSizePair[10];
    for (int index = 0; index < 10; index++)
    {
        double i = index - 1;
        double raw = baseSize * Math.Exp(i / 5.0);
        double intSize = index > 1 ? Math.Floor(raw) : Math.Ceiling(raw);
        double even = Math.Floor(intSize / 2) * 2;   // 取偶数
        pairs[index] = new FontSizePair(even, GetLineHeight(even));
    }
    pairs[1] = new FontSizePair(baseSize, GetLineHeight(baseSize));
    return pairs;
}
```

`base=14` 时导出：`12/14/16/20/22/24/30/38`，行高因子 `1.6667/1.5714/1.5/1.4/1.3636/1.3333/1.2667/1.2105`。

> WPF 的 `FontSize` 单位是 DIP（= 96dpi 下的 px），与 CSS `px` **数值等价**，无需换算。但 `LineHeight` 在 WPF 是**绝对值**，必须写成 `fontSize * factor`（如 22.0），不能写 1.5714。

#### 5.2.5 尺寸与控件高度（E1）

```
sizeXXS..sizeXXL = sizeUnit * (sizeStep - 3 ... sizeStep + 8)  → 4/8/12/16/16/20/24/32/48
controlHeightSM = controlHeight * 0.75   // 24
controlHeightXS = controlHeight * 0.5    // 16
controlHeightLG = controlHeight * 1.25   // 40
```

### 5.3 主题分发机制（架构决策）

这是全项目最关键的决策，直接决定滚动性能与主题切换体验。

| 方案 | 做法 | 优点 | 缺点 | 结论 |
|---|---|---|---|---|
| A. 全量 `DynamicResource` | 每个 token 一个资源键，样式里 `{DynamicResource Antd.ColorPrimary}` | 实现最简单，切换只需替换字典 | 每次属性读取都要走资源查找树；切换时整树 `InvalidateProperty`；大表格滚动明显掉帧 | 仅用于 `Window` 级背景等少数低频率属性 |
| B. 逐 token 绑定 | 每个 token 一个 `Binding` 到 `ThemeManager` | 属性级精确失效 | 数千个 `BindingExpression`，切换时数千次通知 | 不采用 |
| **C. 不可变快照 + 单点通知（推荐）** | `ThemeManager.Snapshot` 为不可变对象，属性变化只抛一次 `PropertyChanged(nameof(Snapshot))`；样式用 `{antd:Token ColorPrimary}` 扩展，内部生成绑定到 `Snapshot.ColorPrimary` | 切换 = 替换整个快照，一次通知全部更新；快照不可变可预计算、可跨线程；Brush 命中缓存 | 需要自建 MarkupExtension；首次绑定有成本 | **采用** |

实现要点：

```csharp
// Antd.Wpf.Theme/ThemeManager.cs
public sealed class ThemeManager : INotifyPropertyChanged
{
    public static ThemeManager Current { get; } = new ThemeManager();

    private ThemeSnapshot _snapshot = ThemeSnapshot.CreateDefault();

    public ThemeSnapshot Snapshot
    {
        get { return _snapshot; }
        private set
        {
            _snapshot = value;
            OnPropertyChanged("Snapshot");   // 一次通知，全量刷新
        }
    }

    public void Apply(ThemeConfig config)
    {
        // 1) 纯计算，可在后台线程完成
        ThemeSnapshot next = ThemeSnapshot.Build(config);
        // 2) 仅 UI 线程赋值
        if (Dispatcher.CheckAccess()) Snapshot = next;
        else Dispatcher.Invoke(() => Snapshot = next);
    }
}
```

```csharp
// Antd.Wpf.Theme/Markup/TokenExtension.cs  —— 用法：Background="{antd:Token ColorBgContainer}"
[MarkupExtensionReturnType(typeof(object))]
public sealed class TokenExtension : MarkupExtension
{
    public string Path { get; set; }
    public TokenExtension() { }
    public TokenExtension(string path) { Path = path; }

    public override object ProvideValue(IServiceProvider provider)
    {
        var binding = new Binding("Snapshot." + Path)
        {
            Source = ThemeManager.Current,
            Mode = BindingMode.OneWay
        };
        return binding.ProvideValue(provider);
    }
}
```

**Brush 缓存**：`BrushCache` 维护 `Dictionary<int, SolidColorBrush>`（key 为 ARGB），全部 `Freeze()`。WPF 中未冻结的 `Brush` 会导致渲染线程回归 UI 线程，是大表格掉帧的头号原因。

**嵌套主题**：`ThemeScope.Scope` 附加属性提供组件级/子树级覆盖，实现方式为沿逻辑树向上查找最近的 `ThemeScope`，取「父快照 + 本作用域覆盖」生成派生快照并缓存（以覆盖配置的哈希为 key，避免重复计算）。

**性能预算（M0 验收项）：**

| 指标 | 目标 |
|---|---|
| 全量主题切换耗时（1000 控件界面） | < 120 ms |
| Token 派生耗时（冷启动，含 13 个预设色板） | < 15 ms |
| 冻结 Brush 缓存命中率 | > 99% |
| 1000 行 Table 滚动帧率 | ≥ 55 FPS @1080p |

### 5.4 校验策略：与官方对拍

不靠肉眼，靠数据：

1. `tools/token-sync/` 用 Node 调用 `antd` 的 `theme.getDesignToken()`，导出 JSON：
   - 组合覆盖：`default`、`dark`、`compact`、`dark+compact`、`colorPrimary=#00b96b`、`borderRadius=8`、`fontSize=16`
2. `Antd.Wpf.Tests` 读取该 JSON，逐 token 断言 C# 计算结果。
3. 断言精度：颜色 **完全相等**（ARGB 全等）、尺寸 `< 0.01`。
4. CI 中作为必过门禁；每次升级对齐的 antd 版本时重跑。

这是「像素级还原」唯一可工程化的保障手段。

---

## 6. 视觉还原规范

### 6.1 尺寸与 DPI

- 1 CSS px = 1 DIP。不做缩放换算。
- 全库默认 `UseLayoutRounding=True`，`SnapsToDevicePixels=True`。
- **已知差异**：DPI ≠ 100% 时，1 DIP 边框会落在半像素上产生轻微模糊。取舍：接受模糊（保持与 antd 一致的视觉权重），不做设备像素对齐（那会导致 125% 下边框消失）。此为**已知限制**，写入文档。

### 6.2 字体与行高

```xml
<!-- 映射自 seed.fontFamily -->
FontFamily="Microsoft YaHei UI, Segoe UI, Roboto, Helvetica Neue, Arial, Noto Sans, sans-serif"
```

WPF `FontFamily` 支持逗号分隔回退列表，与 CSS font-family 语义接近，可 1:1 映射。

| 项 | antd | WPF 实现 | 差异 |
|---|---|---|---|
| 行高 | 乘数 1.5714 | `LineHeight = 22.0` + `LineStackingStrategy=BlockLineHeight` | CSS 行盒垂直居中算法不同，单行文本基线偏差约 1~2 DIP，需在模板中用 `Grid` 固定行高 + `VerticalAlignment=Center` 兜底 |
| 字重 600 | `fontWeightStrong: 600` | `FontWeights.SemiBold` | 中文字体（雅黑）无 600 字面，WPF 回退到 400 或合成加粗；建议中文环境统一映射为 `Bold(700)`，写入规范 |
| 文本省略 | `text-overflow: ellipsis` | `TextTrimming=CharacterEllipsis` | 一致 |
| 多行省略 | `-webkit-line-clamp` | 无原生支持 | 自研 `ClampedTextBlock`（逐行测量裁剪），P2 |

### 6.3 圆角、边框、阴影

- 圆角：`Border.CornerRadius` 直接映射，但**只支持统一半径**。antd 的 `borderRadiusLG=8 / SM=4 / XS=2` 三档足够，无需逐角。
- 边框：`lineWidth=1`，颜色走 `colorBorder` / `colorBorderSecondary`。
- **阴影是最大视觉差距**。antd 的 `boxShadow` 是 3 层叠加（如 `0 1px 2px rgba(0,0,0,.03), 0 1px 6px -1px rgba(0,0,0,.02), 0 2px 4px rgba(0,0,0,.02)`），WPF `DropShadowEffect` 只支持单层且无负 spread。

  对策：实现 `ShadowStack` 附加行为，用 2~3 个嵌套 `Border`（每层一个 `DropShadowEffect`），按 token 分解为层。并给出**降级等级**：

  | 等级 | 做法 | 适用 |
  |---|---|---|
  | L0 | 单层近似（合并为一层中等模糊） | 列表内高频项（Table 行、Tag） |
  | L1 | 双层 | Card、Popover |
  | L2 | 三层全还原 | Modal、Dropdown 等一次性浮层 |

  性能红线：`DropShadowEffect` 会强制软件渲染路径，**禁止**出现在虚拟化列表项模板内。

### 6.4 动效

`cubic-bezier` → WPF `KeySpline`，控制点语义完全一致（x1,y1,x2,y2），可 1:1 映射；`motionEaseInBack` 的 y 值超界（-0.46 / 1.46）WPF 同样允许。

```csharp
// 用法：new SplineDoubleKeyFrame(value, keyTime) { KeySpline = MotionEaseOutQuint }
public static readonly KeySpline MotionEaseOutQuint = new KeySpline(0.23, 1, 0.32, 1);
public static readonly KeySpline MotionEaseInOut   = new KeySpline(0.645, 0.045, 0.355, 1);
```

时长 token：`0.1s / 0.2s / 0.3s` → `TimeSpan.FromMilliseconds(100/200/300)`。

`motion = false` 或系统关闭动画时，全部 `Duration` 置 0（读取 `SystemParameters.ClientAreaAnimation` 与 `SystemParameters.MenuAnimation`）。

### 6.5 图标

- 源：`@ant-design/icons`（v6，与 antd v6 配套）。SVG 为 1024×1024 viewBox，path 语法与 WPF `StreamGeometry` 迷你语言兼容（含 `A` 圆弧指令，参数语义相同，仅需把 `,` 规范化为空格）。
- 构建期 codegen：`tools/icon-gen/` 扫描 SVG → 生成 `Icons.g.cs`（`Geometry` 静态字段）+ `Icons.xaml`（资源字典，`<Geometry x:Key="Antd.Icon.User"/>`）。
- 首期只入库 **约 120 个高频图标**，其余按需增量；全量约 800+，全量入库会使程序集体积增加约 3~4 MB（E3，需实测）。
- Two-tone 图标需两条 path + 不同不透明度，单独处理为 `DrawingGroup`。
- `Icon` 控件：默认尺寸 = `1em`（即继承 `FontSize`），颜色继承 `Foreground`。

### 6.6 语义槽规范（对标 v6 的 `classNames` / `styles`）

antd v6 用 `classNames`/`styles` 暴露组件内部节点（F13）。WPF 无 DOM，用**约定**承接：

1. 每个复杂控件在 `ControlTemplate` 中为关键节点命名，前缀 `PART_`（WPF 约定，可被 `OnApplyTemplate` 获取）。
2. 同时暴露同名依赖属性（可空），模板内绑定：`Style="{TemplateBinding HeaderStyle}"`。
3. 命名规范：`PART_` + PascalCase 语义名，如 `PART_Header`、`PART_Body`、`PART_Footer`、`PART_Popup`。
4. 对外文档给出「antd 语义槽 ↔ WPF Part/属性」对照表。

示例（Card）：

| antd v6 | 本项目 |
|---|---|
| `styles.header` | `Card.HeaderStyle`（`Style` 类型） |
| `styles.body` | `Card.BodyStyle` |
| `classNames.header` | 模板节点 `PART_Header`（可通过 `Card.Template` 重写） |
| `variant="borderless"` | `Card.Variant` 枚举 |

---

## 7. 组件路线图

73 项官方组件中：`Util` 为内部工具（不对外）、`List` 已废弃（不实现）、6 个 Pro 组件不属于 antd 本体（不实现）。**实际需实现 71 项**，分 4 期。

难度：★ 低 / ★★ 中 / ★★★ 高 / ★★★★ 极高。人日为估算值（假设工程师熟悉 WPF 模板与绑定）。

### 7.1 M1 基础设施 + 原子组件

| 组件 | 分类 | WPF 基类 | 实现要点 | 难度 | 人日 |
|---|---|---|---|---|---|
| ThemeManager / ConfigProvider | 基础 | 附加属性 | 主题引擎，见 §5 | ★★★★ | 15 |
| Icon | 通用 | `Control` | Geometry + 继承字号/前景色 | ★★ | 5 |
| Button | 通用 | `ButtonBase` | 6 类型 × 5 状态 × 3 尺寸 × 图标位置；`Space.Compact` 需相邻圆角裁剪 | ★★★ | 3 |
| Typography | 通用 | `TextBlock` 派生 | Title/Text/Paragraph/Link；可编辑、可复制、省略 | ★★ | 3 |
| Divider | 布局 | `Control` | 水平/垂直、带文字、虚线 | ★ | 0.5 |
| Space | 布局 | `Panel`（自研） | 间距 + 紧凑模式，替代 `Button.Group` | ★ | 1.5 |
| Flex | 布局 | `Panel`（自研 `FlexPanel`） | 完整 flex 语义（grow/shrink/basis/align/justify/wrap） | ★★★ | 4 |
| Grid (Row/Col) | 布局 | `Panel` + 附加属性 | 24 栅格、gutter、offset/push/pull、响应式断点 | ★★★ | 4 |
| Layout | 布局 | `Control` + `ContentControl` | Sider/Header/Footer/Content；Sider 可折叠 | ★★ | 2 |
| Splitter | 布局 | `GridSplitter` 封装 | 面板分割 | ★★ | 2 |
| Tag | 数据展示 | `ContentControl` | 颜色态、可关闭、Checkable | ★ | 1 |
| Badge | 数据展示 | `Control` | 圆点/数字/状态点；溢出 99+ | ★ | 1.5 |
| Avatar | 数据展示 | `ContentControl` | 图片/文字/图标/组叠加 | ★ | 1.5 |
| Empty | 数据展示 | `Control` | 插画（用 Geometry 简化版） | ★ | 0.5 |
| Spin | 反馈 | `ContentControl` | 旋转动画（`RotateTransform` + `DoubleAnimation`） | ★ | 1 |
| Skeleton | 反馈 | `Control` | 骨架屏 + 闪烁动画 | ★ | 1.5 |
| Progress | 反馈 | `Control` | line/circle/dashboard；自绘 `OnRender` | ★★ | 3 |
| Statistic | 数据展示 | `Control` | 数值格式化、倒计时 | ★ | 1.5 |
| Segmented | 数据展示 | `Selector` | 滑块指示器动画 | ★★ | 1.5 |
| Watermark | 其他 | `Adorner` | `DrawingBrush` 平铺 + 旋转 | ★★ | 2 |
| FloatButton | 通用 | `ButtonBase` | 悬浮按钮 + 回顶 | ★ | 1 |
| Affix | 其他 | 附加行为 | 滚动偏移固定 | ★ | 1 |
| BorderBeam | 其他 | `Adorner` | 沿边框流动的光束动画 | ★★ | 1.5 |

**小计 ≈ 56 人日**

### 7.2 M2 数据录入

| 组件 | WPF 基类 | 实现要点 | 难度 | 人日 |
|---|---|---|---|---|
| Input | `TextBox` 模板重写 | prefix/suffix/addon/allowClear/password/count/textarea | ★★★ | 5 |
| InputNumber | `Control` + `TextBox` | 步进、格式化、边界、控件按钮 | ★★ | 4 |
| Checkbox | `ToggleButton` | 组、半选（`IsThreeState`） | ★ | 2 |
| Radio | `RadioButton` | 组、按钮样式 | ★ | 2 |
| Switch | `ToggleButton` | 滑动动画、loading、内嵌文字 | ★★ | 2 |
| Slider | `Slider` 模板重写 | 单值/双值/带输入框/刻度/竖向 | ★★★ | 4 |
| Rate | `Control` | 半星、清除、字符自定义 | ★★ | 2 |
| Select | `Selector` + `Popup` | **单/多选、搜索、标签折叠、虚拟化、分组、远程数据、清除** | ★★★★ | 10 |
| AutoComplete | 继承 Select | 仅需放开自由输入 | ★ | 1 |
| Cascader | `Control` + 多级 Popup | 级联面板、搜索、懒加载 | ★★★ | 6 |
| TreeSelect | Select + Tree | 组合两控件 | ★★★ | 4 |
| Mentions | Input + Popup | 触发字符、候选列表 | ★★ | 2 |
| ColorPicker | `Control` + Popup | 色板/色相条/透明度/预设/取色器 | ★★★ | 6 |
| Upload | `Control` | 拖拽（`AllowDrop`）、文件列表、进度、多选 | ★★ | 4 |
| Form / FormItem | `Panel` + 附加属性 | 布局、校验规则、依赖联动、`INotifyDataErrorInfo` | ★★★ | 6 |
| DatePicker | `Control` + Popup | 日历面板、区间选择、禁用日期、周/月/季/年 | ★★★★ | 10 |
| TimePicker | `Control` + Popup | 时分秒列选择器（滚动吸附） | ★★★ | 4 |
| Calendar | `Control` | 月/年视图、单元格自定义渲染 | ★★★ | 4 |
| Transfer | `Control` | 双列表穿梭、搜索、分页 | ★★ | 4 |

**小计 ≈ 82 人日**

### 7.3 M3 数据展示与导航

| 组件 | WPF 基类 | 实现要点 | 难度 | 人日 |
|---|---|---|---|---|
| **Table** | `Control` + 自研面板 | 见 §8.4。列定义、排序、筛选、固定列、展开行、树形、虚拟化、分页、行选择、可编辑 | ★★★★ | 25 |
| Tree | `TreeView` 模板重写 | 勾选（半选）、拖拽、虚拟化、搜索高亮 | ★★★ | 10 |
| List / Listy | `ItemsControl` | 列表/卡片两种布局、分页、加载更多 | ★★ | 3 |
| Card | `ContentControl` | 头部/额外/封面/操作/网格模式 | ★ | 2 |
| Collapse | `ItemsControl` | 手风琴、展开动画 | ★★ | 2 |
| Descriptions | `Control` | 行列布局、跨列、带边框 | ★★ | 3 |
| Timeline | `ItemsControl` | 节点自定义、模式（left/right/alternate） | ★ | 2 |
| Steps | `ItemsControl` | 横向/纵向、状态机、点状 | ★★ | 2 |
| Tabs | `TabControl` 模板重写 | 卡片/可编辑/居中/额外内容/滚动 | ★★★ | 4 |
| Carousel | `Control` | 自动播放、指示点、淡入/滑动 | ★★ | 3 |
| Image | `Image` + 预览层 | 预览放大/旋转/缩放、加载失败占位 | ★★ | 3 |
| QRCode | `Control` | 需自带 QR 编码实现（无第三方依赖约束） | ★★★ | 3 |
| Tour | `Adorner` + 遮罩 | 引导步骤、目标元素定位 | ★★★ | 3 |
| Tooltip | `Popup` | 12 个方位、延迟、箭头 | ★★ | 3 |
| Popover | 继承 Tooltip | 内容化 + 标题 | ★ | 2 |
| Masonry | `Panel`（自研） | 瀑布流布局算法 | ★★ | 2 |
| Anchor | `Control` | 滚动监听、锚点高亮、滑块 | ★★ | 2 |
| Breadcrumb | `ItemsControl` | 分隔符、下拉菜单 | ★ | 1 |
| Menu | `ItemsControl` | 内联/垂直/水平、子菜单浮层、选中态、折叠 | ★★★ | 6 |
| Pagination | `Control` | 页码省略、快速跳转、页大小切换 | ★★ | 3 |
| Dropdown | `Popup` | 菜单型/自定义、触发方式、箭头 | ★★ | 3 |

**小计 ≈ 89 人日**

### 7.4 M4 反馈与浮层

| 组件 | 实现要点 | 难度 | 人日 |
|---|---|---|---|
| Modal | 遮罩 + 焦点陷阱 + 动画 + 静态方法 + 命令式 API | ★★★ | 5 |
| Drawer | 四方向滑入 + 尺寸 + 嵌套 | ★★★ | 4 |
| Message | 全局队列 + 堆叠 + 自动关闭 | ★★ | 3 |
| Notification | 四角放置 + 堆叠 + 进度 | ★★ | 3 |
| Popconfirm | Tooltip + 确认按钮 | ★ | 1 |
| Alert | 4 类型 × 带描述/可关闭/带操作 | ★ | 1.5 |
| Result | 状态插画 + 操作区 | ★ | 1 |

**小计 ≈ 18.5 人日**

### 7.5 横向工程能力

| 项 | 人日 |
|---|---|
| 动效系统（统一 Duration/缓动 + 减少动效降级） | 5 |
| 无障碍（`AutomationPeer` × 全控件、键盘导航、UIA 名称） | 10 |
| 国际化（`LocalizationProvider` + zh-CN/en-US 资源） | 4 |
| 图标 codegen 工具链 | 5 |
| Token 对拍工具链 + CI | 4 |
| Gallery 文档站（对标 ant.design，含代码高亮与属性表） | 12 |
| 截图基准测试框架 | 6 |
| 打包 / 版本策略 / 发布流水线 | 4 |

**小计 ≈ 50 人日**

### 7.6 汇总

| 阶段 | 人日 |
|---|---|
| M1 基础设施 + 原子 | 56 |
| M2 数据录入 | 82 |
| M3 数据展示与导航 | 89 |
| M4 反馈与浮层 | 18.5 |
| 横向能力 | 50 |
| **小计** | **295.5** |
| 缓冲（技术风险 20% + 联调 10%） | 88.5 |
| **合计** | **≈ 384 人日** |

**工期**：2 名熟悉 WPF 的工程师全职 ≈ **9 个月**；3 人 ≈ **6 个月**。（此为估算，未含需求变更与验收返工。）

**不实现清单（需在文档中明示，避免用户误解）：**
`Masonry`（低优先级，可后置）、`List`（已废弃）、Pro 系列 6 个组件、`Util`（内部）。

---

## 8. 关键子系统

### 8.1 浮层系统（Overlay）

WPF 的浮层有三条路径，各有硬限制：

| 载体 | 限制 | 适用 |
|---|---|---|
| `Popup` | 独立视觉树，**主题附加属性继承断开**；不受父级 `Clip` 影响；支持 `CustomPopupPlacementCallback` 精确摆放 | Tooltip、Dropdown、Select 下拉、DatePicker 面板 |
| `AdornerLayer` | 受 `AdornerDecorator` 边界约束，滚动时需手动重定位 | Tour、Watermark、拖拽提示 |
| 无边框 `Window` | 可跨窗口/跨屏、可超出父窗口边界；但切换成本高、`ShowInTaskbar` 需处理 | Modal（需遮蔽全屏）、Notification 四角 |

**核心陷阱**：`Popup` 内容不在主逻辑树内，`ThemeScope` 附加属性继承会断链，导致弹层永远使用默认主题（暗色主题下弹层是白色的经典 bug）。对策：

- `Popup` 包裹统一走 `AntdPopup` 内部控件，在 `Opened` 时显式将当前 `ThemeSnapshot` 与 `ComponentSize` 复制给 `Child`。
- 或用 `PlacementTarget.Tag` 传递主题引用。

**统一入口** `OverlayHost`：所有浮层注册到同一层，处理 z-index 排序（`zIndexPopupBase = 1000`）、点击外部关闭、Esc 关闭、多浮层叠加互斥。

### 8.2 焦点、键盘与无障碍

- Modal/Drawer：`KeyboardNavigation.TabNavigation="Cycle"` 实现焦点陷阱；`Esc` 关闭；打开时记录并恢复原焦点。
- 全库键盘支持底线：`Tab` 可达、`Space/Enter` 触发、方向键在 Select/Menu/Tree 内导航。
- 每个控件实现 `OnCreateAutomationPeer`，返回带 `AutomationControlType` 与 `Name` 的 Peer。这是 net48 与 net8 完全一致的能力，无兼容问题。
- CI 加 UIA 冒烟测试（用 `FlaUI` 或 `System.Windows.Automation`）验证控件可被读屏识别。

### 8.3 Form 与校验

antd Form 是「虚拟 DOM + 受控字段 + 规则数组」。WPF 无受控组件概念，采用附加属性方案：

```xml
<antd:Form Layout="Vertical" Model="{Binding Vm}">
  <antd:FormItem Label="用户名" Field="UserName">
    <antd:FormItem.Rules>
      <antd:RequiredRule Message="请输入用户名" />
      <antd:MaxLengthRule Max="20" />
    </antd:FormItem.Rules>
    <antd:Input Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}" />
  </antd:FormItem>
</antd:Form>
```

- 校验结果落到 `INotifyDataErrorInfo`（net48 支持），复用 WPF 原生 `Validation.HasError` 触发器描红。
- 支持异步校验（返回 `Task<ValidationResult>`）、依赖联动（`ShouldUpdate` 语义用 `PropertyChanged` 监听实现）。
- `Form.Validate()` / `ResetFields()` / `ScrollToField()` 三个 API 必须提供。

### 8.4 Table（单项最大风险）

antd Table 与 WPF `DataGrid` 语义差异过大（列定义 vs 绑定列、树形数据、固定列），**决定自研**，不继承 `DataGrid`。

技术方案：

1. **结构**：`Table` = 外层 `ScrollViewer`（垂直）+ 内部 `Grid` 三区（左固定 / 主体 / 右固定），三区横向滚动偏移通过 `ScrollChanged` 同步。
2. **行虚拟化**：`VirtualizingStackPanel`（`IsVirtualizing=True`、`VirtualizationMode=Recycling`），`ItemContainerGenerator` 回收。列不虚拟化（antd 也不做）。
3. **列宽**：`TableColumn` 支持 `Width`（固定）/ `Star`（比例）/ `Auto`（内容测量，首屏两遍布局）。实现 `MeasureOverride` 两阶段：先测量 Auto 列，再分配剩余给 Star 列。
4. **固定列**：用 `TranslateTransform` 抵消横向偏移，而非重复渲染。
5. **排序/筛选**：`TableColumn.Sorter` / `Filters`，排序在 `ICollectionView` 层或数据源层执行（由 `Table.SortMode` 决定）。
6. **行选择**：`SelectionMode` + `CheckBox` 列 + 半选表头。
7. **性能红线**：行模板内禁止 `DropShadowEffect`、禁止 `Opacity` 动画、禁止 `DynamicResource`。

**预估风险**：25 人日为理想值，实际可能 35 人日。建议在 M3 开始前先做 3 天的技术验证原型（100 万行虚拟滚动 + 固定列 + 树形）。

### 8.5 日期时间

- 不引入第三方日期库（保持零依赖）。`DateTime` + `CultureInfo` 足够。
- net48 无 `DateOnly`/`TimeOnly`，统一用 `DateTime` + 内部裁剪。
- 区间选择（RangePicker）：`CalendarPanel` 双月并排 + 悬浮预览区间。
- 禁用日期：`Func<DateTime, bool>` 委托 + `[minDate, maxDate]` 双层约束。
- 周起始日跟随 `CultureInfo.DateTimeFormat.FirstDayOfWeek`。

### 8.6 虚拟化与大数据

- `VirtualizingStackPanel` 在 net48 与 net8 行为一致，可直接用。
- 但 WPF 自带 `VirtualizingStackPanel` **只支持单方向虚拟化**；`WrapPanel` 虚拟化需自研 `VirtualizingWrapPanel`（antd 的 Grid 卡片流在 WPF 下需要）。列为 P2。
- 数据层建议提供 `AntdDataSource<T>`：分页 + 排序 + 筛选的抽象，屏蔽 `ICollectionView` 复杂度。

### 8.7 全局反馈

- `Message` / `Notification` 需要「无宿主窗口也可用」——用 `Application.Current.MainWindow` 的顶层 `Canvas` 承载；无窗口时降级为独立 `Window`。
- 队列管理：同一 `key` 的 Message 合并、最大堆叠数（默认 5）、自动关闭倒计时（`DispatcherTimer`）。
- **注意**：antd 的静态方法不受 `ConfigProvider` 影响（官方文档明列为坑）。本项目用 `AntdApp.Current` 单例持有配置，天然规避。

### 8.8 国际化

- `LocalizationProvider.Current` + `ResourceDictionary`（zh-CN / en-US）。
- 文案 key 与 antd 的 `locale` 结构对齐（`Pagination.items_per_page` 等），便于迁移对照。
- 日期/数字格式化走 `CultureInfo`，不硬编码。

---

## 9. 工程质量

### 9.1 多目标 csproj 样例

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net48;net8.0-windows</TargetFrameworks>
    <UseWPF>true</UseWPF>
    <LangVersion>7.3</LangVersion>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <RootNamespace>Antd.Wpf</RootNamespace>
    <AssemblyName>Antd.Wpf.Controls</AssemblyName>
    <GenerateAssemblyInfo>true</GenerateAssemblyInfo>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
  </PropertyGroup>

  <!-- net48 专属 -->
  <PropertyGroup Condition="'$(TargetFramework)' == 'net48'">
    <DefineConstants>$(DefineConstants);NETFRAMEWORK</DefineConstants>
  </PropertyGroup>

  <!-- net8+ 专属 -->
  <PropertyGroup Condition="'$(TargetFramework)' == 'net8.0-windows'">
    <DefineConstants>$(DefineConstants);NET8_0_OR_GREATER</DefineConstants>
    <SupportedOSPlatformVersion>7.0</SupportedOSPlatformVersion>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Antd.Wpf.Tokens\Antd.Wpf.Tokens.csproj" />
    <ProjectReference Include="..\Antd.Wpf.Theme\Antd.Wpf.Theme.csproj" />
    <ProjectReference Include="..\Antd.Wpf.Icons\Antd.Wpf.Icons.csproj" />
  </ItemGroup>

</Project>
```

**已知风险与退路（E3，需在 M0 第一天验证）**：`net48` + `UseWPF` 的 SDK 式工程在 .NET SDK ≥ 6 下可用，但 XAML 编译走传统 `MarkupCompilePass`，偶发与 `net8.0-windows` 的 `Page` 生成冲突。若验证失败，退化为：

```
src/Antd.Wpf.Controls/Antd.Wpf.Controls.net48.csproj      (传统工程)
src/Antd.Wpf.Controls/Antd.Wpf.Controls.net8.csproj       (SDK 式)
src/Antd.Wpf.Controls.Shared/**                           (共享源码，两边 Link 引入)
```

**M0 的第一项交付物就是「双目标 Hello World 控件跑通」，而不是写文档或画架构图。**

### 9.2 条件编译清单

| 场景 | net48 | net8+ |
|---|---|---|
| 舍入 | 统一 `AntdMath.RoundJs` | 同 |
| 哈希 | 自实现 `AntdHash` | 可用 `HashCode`（但为一致性仍用自实现） |
| 字符串 | `IndexOf(string, StringComparison)` | 可额外用 `Contains(char)` |
| 异步 | `Task`（无 `ValueTask`） | `Task` |
| 动画 | `Storyboard`/`KeySpline` | 同 |
| 文本渲染 | `TextFormattingMode.Display`（net48 默认 Ideal） | net8 默认 `Display` |
| 建议 | 全库显式设 `TextOptions.TextFormattingMode="Display"`，保证两平台一致 | 同 |

> `TextFormattingMode` 是 net48 与 net8 WPF 渲染差异的常见来源，不显式指定会导致同一 XAML 在两平台字距不同。

### 9.3 测试策略

| 层次 | 内容 | 工具 |
|---|---|---|
| 单元 | Token 派生对拍官方 JSON（§5.4） | xUnit |
| 单元 | 色板算法边界（灰阶、极值 hue、饱和度钳制） | xUnit + 属性测试 |
| 模板冒烟 | 每个控件在 light/dark/compact × 3 尺寸下实例化不抛异常 | xUnit + `STAThread` |
| 视觉基准 | 每个控件截图与基准图做感知差异（阈值 0.5%） | `ImageSharp` + 自研 diff |
| 交互 | Select 键盘导航、Table 虚拟滚动、Modal 焦点陷阱 | FlaUI |
| 性能 | 1000 行 Table 滚动帧率、主题切换耗时 | `BenchmarkDotNet` + 自研帧率采样 |
| 架构 | 依赖规则（Tokens 不引 Controls 等） | `NetArchTest` |

### 9.4 版本与发布

- 版本号与对齐的 antd 版本绑定：`6.0.x` 表示对齐 antd v6.0，`.x` 为自身修订。
- NuGet 单包多目标（`net48` + `net8.0-windows`），`PackageReference` 自动选择。
- net48 用户若用 `packages.config`：需验证 `.targets` 自动引入 `Generic.xaml`；建议文档要求使用 `PackageReference`。

---

## 10. 里程碑

| 里程碑 | 周期（2 人） | 交付物 | 验收（DoD） |
|---|---|---|---|
| **M0 可行性验证** | 2 周 | 双目标工程骨架；主题引擎原型（含色板/中性色/字号三套派生）；Button + Input 两个控件 | ① `net48` 与 `net8` 均能加载控件并切换亮暗主题；② 20 个关键 token 与官方对拍全等；③ 依赖注入与程序集边界通过架构测试 |
| **M1 原子层** | 5 周 | §7.1 全部 22 项 | ① Gallery 可浏览全部控件；② 截图基准覆盖，差异 < 0.5%；③ 主题切换 < 120ms |
| **M2 录入层** | 7 周 | §7.2 全部 19 项 | ① Form 校验含异步与联动；② Select 支持 1 万项虚拟化搜索不掉帧；③ DatePicker 区间选择可用 |
| **M3 展示层** | 8 周 | §7.3 全部 21 项 | ① Table 100 万行虚拟滚动 ≥ 55 FPS；② 固定列/树形/展开行全通；③ Menu/Dropdown 键盘可达 |
| **M4 反馈层** | 2 周 | §7.4 全部 7 项 | ① Modal 焦点陷阱与 Esc 通过 UIA 测试；② Message 队列合并正确 |
| **M5 横向打磨** | 4 周 | 无障碍、i18n、动效降级、图标全量 | ① 全控件 AutomationPeer 覆盖率 100%；② Narrator 走查通过；③ 减少动效模式全局生效 |
| **M6 稳定发布** | 持续 | 1.0.0 NuGet + 文档站 | ① 连续 4 周无 P0/P1；② 文档站与 ant.design 结构对齐；③ 迁移指南完成 |

总计约 **9 个月 / 2 人**（含 M0 验证期）。

---

## 11. 风险清单

| # | 风险 | 影响 | 概率 | 对策 |
|---|---|---|---|---|
| R1 | net48 与 net8 的 SDK 式多目标 XAML 编译冲突 | 阻塞全部进度 | 中 | M0 首日验证；退路为双 csproj + 共享源码 |
| R2 | Token 数值与官方差一档（舍入/混合算法） | 视觉不还原，返工大 | 高 | 对拍工具链前置到 M0；`RoundJs` 全局替换 |
| R3 | Table 复杂度超预期 | M3 延期 1~2 周 | 高 | 先做 3 天技术验证原型；必要时一期砍掉「树形 + 可编辑」 |
| R4 | 阴影/特效导致渲染性能崩塌 | 体验差 | 中 | 阴影降级等级（§6.3）+ 性能门禁 |
| R5 | DPI 非 100% 时 1px 边框模糊 | 观感差异 | 高 | 已知限制，写入文档；提供 `Border.Width` 可调 token |
| R6 | antd 上游演进（v6 → v7）导致规范漂移 | 持续维护成本 | 中 | 版本号与 antd 对齐；Token 对拍 CI 化，升级成本可量化 |
| R7 | 与社区 `AntDesign.WPF` 命名/包冲突 | 用户混淆 | 中 | 使用 `Antd.Wpf.*` 命名；README 明确区分 |
| R8 | 团队对 WPF 模板/绑定性能不熟 | 质量波动 | 中 | M0 输出《WPF 性能规范》，Code Review 强制 |

---

## 12. 验收标准（整体）

1. **规范一致性**：亮/暗/紧凑三种主题下，20 个抽样组件的截图与 ant.design 官方 demo 做视觉对比，主要元素位置误差 ≤ 2 DIP，颜色 ARGB 全等（允许阴影层数差异）。
2. **Token 一致性**：与 `theme.getDesignToken()` 输出的 200+ token 全量对拍，颜色全等、尺寸误差 < 0.01。
3. **双平台一致性**：同一 Gallery 在 net48 与 net8 下截图差异 < 0.5%。
4. **性能**：1000 行 Table 滚动 ≥ 55 FPS；主题切换 < 120ms；冷启动增加 < 200ms。
5. **可访问性**：全控件可被 UIA 枚举，键盘全流程可达。
6. **可定制性**：业务方在不修改库源码的前提下，可通过 Token 覆盖完成品牌换肤（提供 3 个真实案例）。
7. **文档**：每个组件具备「属性表 + 示例 + 与 antd 差异说明」三件套。

---

## 13. 待决问题（需评审拍板）

| # | 问题 | 选项 | 建议 |
|---|---|---|---|
| Q1 | 包名/命名空间 | `Antd.Wpf.*` / `AntdWpf.*` / 其他 | `Antd.Wpf.*`（与现有 `AntDesign.WPF` 区分） |
| Q2 | 是否对齐 antd **v6** 还是 v5 | v6（CSS 变量、语义槽）/ v5（更成熟） | **v6**。v6 的 CSS 变量思路与 WPF 主题机制同构，且是当前主线 |
| Q3 | 是否允许第三方依赖 | 零依赖 / 允许 `CommunityToolkit.Mvvm` 等 | **零依赖**（控件库应最小化传递依赖）；MVVM 辅助放独立可选包 |
| Q4 | 是否需要 RTL | 需要 / 暂不需要 | P2，先在 Token/布局层预留 `Direction` |
| Q5 | Gallery 是否对外发布 | 仅内部 / 公开发布 | 建议公开发布，作为项目门面（参考 ant.design 文档站形态） |
| Q6 | 一期是否必须覆盖 Table 全功能 | 全量 / 分期 | **分期**：一期覆盖 80% 场景（无树形、无可编辑），二期补齐 |
| Q7 | 团队规模 | 2 人 / 3 人 | 3 人可将工期压到 6 个月，但需 1 人专责 Table |

---

## 附录 A：证据来源索引

| 编号 | 来源 | 用途 |
|---|---|---|
| S1 | https://ant.design/docs/react/introduce | 版本、生态、支持范围 |
| S2 | https://ant.design/docs/react/customize-theme | 三层 Token、算法、API |
| S3 | https://ant.design/docs/react/migration-v6 | v6 变更、语义槽 API |
| S4 | https://ant.design/components/overview | 73 组件清单 |
| S5 | https://ant.design/components/grid | 24 栅格、断点 |
| S6 | ant-design/ant-design `components/theme/themes/seed.ts` | Seed 默认值 |
| S7 | ant-design-colors `src/generate.ts` | 色板 HSV 算法 |
| S8 | ant-design `components/theme/themes/{default,dark}/colors.ts` | 语义索引映射、中性色 alpha |
| S9 | ant-design `components/theme/themes/shared/gen*.ts` | 字号/尺寸/高度派生公式 |
| S10 | ant-design `components/theme/themes/shared/genColorMapToken.ts` | Map Token 索引关系 |

## 附录 B：本文中的推断项（需在 M0 验证）

| 编号 | 推断内容 | 验证方式 |
|---|---|---|
| E3-1 | net48 + UseWPF 的 SDK 式多目标工程可用 | M0 第一天搭建验证 |
| E3-2 | 全量图标入库使程序集增加 3~4 MB | 实测 800 个 SVG 生成的 XAML 体积 |
| E3-3 | Table 25 人日的估算 | 3 天技术验证原型后复核 |
| E3-4 | 2 人 9 个月的工期 | 以 M1 实际速率外推修正 |
