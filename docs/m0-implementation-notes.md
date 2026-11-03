# M0 实施记录：可行性验证

| 项目 | 内容 |
|---|---|
| 版本 | v1.0 |
| 日期 | 2026-11-03 |
| 对应计划 | `docs/antd-wpf-implementation-plan.md` §10 里程碑 M0 |
| 结论 | **通过**。双目标工程、主题引擎、Button/Input 全部可用，token 与 antd 6.6.8 逐值一致 |

---

## 1. 里程碑验收对照

计划中 M0 的三条 DoD 与实测结果：

| # | 验收标准 | 结果 | 证据 |
|---|---|---|---|
| ① | `net48` 与 `net8` 均能加载控件并切换亮暗主题 | **通过** | `tests/Antd.Wpf.SmokeTests` 在 net48 与 net8.0-windows 各 47 项断言全过 |
| ① | 20 个关键 token 与官方对拍全等 | **超额通过** | **536 个 token × 8 组配置 × 2 个框架，全部逐值相等**（计划只要求 20 个） |
| ③ | 依赖注入与程序集边界通过架构测试 | **通过** | Tokens 无 WPF 引用；Theme 不引用 Controls；Controls 双 TFM 产物存在 |

补充：视觉验证不止于断言。冒烟测试会把 Gallery 渲染成 PNG（`artifacts/gallery-light.png`、`gallery-dark.png`、
`zoom-inputs.png`），人工复核排版与主题切换效果。

---

## 2. 交付物

```
Antd.Wpf.sln
src/Antd.Wpf.Tokens/          netstandard2.0   17 个文件，纯 C#，无 WPF 依赖
src/Antd.Wpf.Theme/           net48;net8.0-windows
src/Antd.Wpf.Controls/        net48;net8.0-windows
demo/Antd.Wpf.Gallery/        net48;net8.0-windows
tests/Antd.Wpf.TokenParity/   net48;net8.0      536×8 token 对拍
tests/Antd.Wpf.SmokeTests/    net48;net8.0-windows  47 项断言 + 截图
tools/token-sync/             Node，从 antd 导出 golden token
docs/m0-implementation-notes.md
```

### 已实现能力

- **Token 派生全链路**：Seed → Map → Alias，593 行派生逻辑，覆盖色板 / 中性色 / 字号 / 行高 / 尺寸 /
  控件高度 / 圆角 / 动效时长 / 阴影 / 断点，共 536 个 token。
- **主题引擎**：`ThemeManager` 单例 + 不可变 `ThemeSnapshot` + 冻结 `BrushSet` +
  `Antd.<tokenName>` 资源字典 + `{antd:Token}` / `{antd:Brush}` / `{antd:FontFamily}` 三个 XAML 扩展。
- **控件**：`Button`（5 变体 × 危险态 × 3 尺寸 × 3 形状 × loading × block）、
  `Input`（尺寸 / 状态 / 前后缀 / 占位符 / 清除按钮 / 聚焦环）。
- **主题切换**：亮 / 暗一键切换，应用级资源与全部控件同步更新，界面无闪烁。

---

## 3. 与计划的偏差（需要评审确认）

| # | 计划 | 实际 | 原因 |
|---|---|---|---|
| D1 | Tokens 目标 `net48;net8.0-windows` | 改为 **`netstandard2.0`** | Tokens 是纯计算库，不应绑平台。单一资产同时服务 net48 / net8 / 未来 net9，少一个构建矩阵维度。 |
| D2 | 主题分发「快照 + 单点通知」 | **快照 + DynamicResource 双通道** | WPF 的 `Setter.Value` 不允许 Binding（运行时抛错），变体样式无法用绑定表达。因此：XAML 用 DynamicResource，代码用 Snapshot，两者同源同值。 |
| D3 | 控件状态用 XAML 触发器 | **部分状态在 C# 计算** | 5 变体 × hover/pressed/disabled 用触发器需 ~60 个 Trigger，且颜色来自 token 无法写进 Setter。改为解析成 token 名称后取冻结画刷，一个 switch 解决。 |
| D4 | `Antd.Wpf.Tests` 用 xUnit | **零依赖可执行断言程序** | 保持仓库零第三方包，CI 只需 `dotnet run` 即可判定退出码。 |

---

## 4. 实施中发现并解决的坑（可直接指导 M1）

这 6 个问题都不是「写错代码」，而是 WPF 与 antd 的语义差异，都会在后续组件中反复出现。

### K1. HSB 与 HSL 不能混用（影响：所有语义色）

antd 的色板由 `generate()` 用 **HSV** 构造：

```ts
new FastColor({ h: getHue(...), s: getSaturation(...), v: getValue(...) })
```

而 `darken` / `lighten` 用 **HSL** 构造。第一版把两者都写成 HSL，结果浅色全部发白、深色全灰。
必须提供 `FromHsv` 与 `FromHsl` 两条独立路径。

### K2. `darken` 用 HSV 饱和度当 HSL 饱和度（影响：自定义背景色）

`FastColor#darken` 内部 `s = getSaturation()`（HSV 饱和度），却传给 HSL 重建。
这是上游的一处口径不一致，但它决定了 `getSolidColor('#123456', 15)` 的实际输出。
照抄才能对齐；"修正"它反而会让 token 对不上。

### K3. 浅色 `darken`、深色 `lighten`（影响：所有背景/边框）

`getSolidColor` 在浅色主题是 `darken`，在深色主题是 `lighten`。
若统一用 `darken`，深色主题的 `colorBgContainer` 会算成 `#000000` 而不是 `#141414`。

### K4. `Math.round` 与 `Math.Round`（影响：颜色档位与字号）

JS 四舍五入，C# 默认银行家舍入。`2.5` 一个得 3 一个得 2。
全库统一走 `AntdMath.RoundJs(x) => Math.Floor(x + 0.5)`。

### K5. `getAlphaColor` 有两个同名不同义的实现（影响：分隔线、聚焦轮廓）

- `themes/*/colorAlgorithm.ts#getAlphaColor(base, alpha)` = 直接设 alpha。
- `theme/util/getAlphaColor.ts#getAlphaColor(front, bg)` = 反解形如 `rgba(5,5,5,0.06)` 的等价半透明色。

第二个还必须**先四舍五入再做范围判断**，否则浮点噪声（`255.00000000000003`）会让循环多走一步，
`controlOutline` 就从 `0.1` 变成 `0.11`。代码里分别命名为 `SetAlpha` 与 `AlphaBlend` 以防混用。

### K6. 模板子元素的本地 `Visibility` 会压死触发器（影响：所有显隐逻辑）

```xml
<TextBlock x:Name="Placeholder" Visibility="Collapsed" />   <!-- 本地值优先级最高 -->
<Trigger Property="ShowPlaceholder" Value="True">
  <Setter TargetName="Placeholder" Property="Visibility" Value="Visible"/>  <!-- 永远不生效 -->
</Trigger>
```

正确做法：**不要**在模板子元素上写本地 `Visibility`，改为成对的 `True→Visible` / `False→Collapsed` 触发器
（默认值由依赖属性在构造时写入，不依赖元素默认）。

### K7. `DataTrigger` 绑定 `TemplatedParent` 的布尔属性不可靠

`<DataTrigger Binding="{Binding CanClear, RelativeSource={RelativeSource TemplatedParent}}" Value="False">`
在本项目环境中对 `bool` 从不匹配（`Value="True"` 同样不匹配），导致清除按钮与聚焦环恒显示。
控件属性本身就是依赖属性时，直接用 `<Trigger Property="CanClear" Value="False">` 既可靠又省一次绑定。

---

## 5. 验证方法（可复现）

```bash
# 1) 全量构建（双 TFM，0 警告 0 错误）
dotnet build Antd.Wpf.sln

# 2) token 对拍：536 × 8 组配置
dotnet run --project tests/Antd.Wpf.TokenParity -f net8.0
dotnet run --project tests/Antd.Wpf.TokenParity -f net48

# 3) 控件 + 主题 + 架构断言（47 项），并输出截图
dotnet run --project tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --shot artifacts
dotnet run --project tests/Antd.Wpf.SmokeTests -f net48

# 4) 人工看效果
dotnet run --project demo/Antd.Wpf.Gallery -f net8.0-windows
```

对拍的 8 组配置：`default`、`dark`、`customPrimary(#00b96b)`、`customRadius8`、`customFontSize16`、
`customBgBase(#123456)`、`customTextBase(#333333)`、`darkPurple(#9254de)`。
后四组专门用来暴露自定义种子下的派生差异。

---

## 6. 已知限制（M0 范围内不修）

| # | 限制 | 计划 |
|---|---|---|
| L1 | `compactAlgorithm` 未移植，传入会抛 `NotSupportedException` | M1 |
| L2 | Button 图标只支持前置，`IconPlacement.End` 未生效 | M1 |
| L3 | 阴影 token 只有字符串形式，尚未转成 WPF 可用的分层特效 | M1（见计划 §6.3 阴影降级等级） |
| L4 | 1px 边框在非 100% DPI 下会有半像素模糊 | 设计取舍，文档化 |
| L5 | 中文字重 600 回退到 400/700，未做显式映射 | M1 |
| L6 | 无 RTL、无高对比度主题、无 `AutomationPeer` | M1 / M5 |
| L7 | 主题切换时控件订阅依赖 `Loaded`，离屏未加载的控件需手动调用 `UpdateVisualState()` | 可接受 |

---

## 7. 下一步（M1 原子层）

按计划 §7.1，M1 需交付 22 个组件。基于 M0 的经验，建议顺序：

1. **先补基础设施**（1 周）：compact 算法、阴影分层、图标 Geometry 生成器、`AntdSize` 全局继承语义。
2. **再做容器类**（1 周）：`Space`、`Flex`、`Grid`、`Divider`、`Layout`——它们会暴露 Flex 布局适配问题。
3. **然后做展示类**（1.5 周）：`Tag`、`Badge`、`Avatar`、`Divider`、`Empty`、`Progress`、`Statistic`。
4. **最后做交互类**（2 周）：`Typography`、`Skeleton`、`Spin`、`Segmented`、`Watermark`、`Affix`、`FloatButton`。

每个组件沿用 M0 的流程：**先写 token 断言 → 再写模板 → 用截图复核**，避免"看起来对但数值错"。
