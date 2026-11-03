# M1 实施记录（进行中）：基础设施 + 原子组件第一批

| 项目 | 内容 |
|---|---|
| 日期 | 2026-11-03 |
| 范围 | compact 算法、图标管线、11 个原子组件 |
| 状态 | 第一批完成，M1 剩余 9 个组件待做 |
| 验证 | 对拍 536×10 组配置 ×2 框架全等；冒烟 63 项断言 ×2 框架全过 |

---

## 1. 本轮交付

### 基础设施

| 项 | 说明 |
|---|---|
| **compact 算法** | 移植 `themes/compact/index.ts` + `genCompactSizeMapToken.ts`。`ThemeAlgorithm` 改为 `[Flags]`，支持 `Dark \| Compact` 组合，与 antd 的 `algorithm: [darkAlgorithm, compactAlgorithm]` 对齐 |
| **图标管线** | `tools/icon-gen/gen-icons.js` 从 `@ant-design/icons-svg` 提取 path，生成 `src/Antd.Wpf.Icons/Icons.g.cs`（57 个 outlined 图标，冻结 `Geometry`）。`Icon` 控件按 1em 定尺、继承 `Foreground`（等价 CSS `currentColor`），支持 `Spin` |

对拍配置从 8 组扩到 **10 组**（新增 `compact`、`darkCompact`），全部 536/536 逐值相等。

### 组件（14 个总计）

| 组件 | 实现要点 |
|---|---|
| `Icon` | `Viewbox` + 896 单位 `Canvas`，几何在生成期归一到 0..896，保证不同图标的视觉重量一致 |
| `Divider` | 水平/垂直、带文字、虚线；实线与虚线用**两个元素切换**（原因见 §3） |
| `Space` | 自研 `Panel`，间距只出现在元素之间，不泄漏到两端 |
| `Tag` | 5 种语义色（Default/Processing/Success/Warning/Error），可关闭并触发 `Closed` 事件 |
| `Badge` | Count/Dot/ShowZero/OverflowCount，`99+` 截断 |
| `Avatar` | 圆形/方形、图片/文字/图标，半径按尺寸推导 |
| `Empty` | 文案 + 图标（暂用 inbox 字形替代 antd 插画） |
| `Spin` | Small/Middle/Large，包裹内容时半透明遮罩 |
| `Progress` | Line + Circle 两种形态，`OnRender` 绘制，支持 Success/Exception 状态 |
| `Statistic` | 标题 + 数值 + 前后缀，支持精度与千分位 |
| `Skeleton` | `OnRender` 绘制，行数/头像/标题可配，`Active` 呼吸动画 |
| `Segmented` | 基于 `ListBox`，选择、键盘导航、`SelectedIndex` 全部复用框架能力 |

Gallery 新增三张卡片演示以上全部组件，亮/暗主题各出一张全页截图。

---

## 2. compact 算法要点

```
baseFontSize   = map.fontSizeSM        (12)
controlHeight  = map.controlHeight - 4  (28)
sizeStep'      = sizeStep - 2           (2)
size           = sizeUnit * sizeStep'   (8)
font scale     = genFontMapToken(12)
```

顺序很关键：compact 必须在 **light/dark 之后、alias 之前** 执行，因为 alias 的 `fontHeight`、
`padding*`、`controlPaddingHorizontal` 都是从这些值派生的。放错位置会导致 `fontHeight` 停留在 22 而不是 20。

---

## 3. 又踩到的 WPF XAML 硬约束（本轮新增 3 条，累计 5 条）

M0 记了 2 条（本地 Visibility 压死触发器、DataTrigger 绑 TemplatedParent bool 失效），本轮又补 3 条，
都已写进代码注释，避免下次重复：

### N1. `TemplateBinding` 不能出现在 `Setter.Value` 里

```xml
<!-- 运行期才炸：ArgumentException "表达式类型不是有效的 Style 值" -->
<Setter TargetName="Line" Property="Stroke" Value="{TemplateBinding LineBrush}" />
```

和 `Binding` 同一限制。解决办法只有两条：把绑定写到元素属性上（触发器只切其它属性），
或者用 `{DynamicResource}`。Divider 的虚线因此改成「实线 Rectangle + 虚线 Rectangle」两套元素切换。

### N2. `ContentControl.HasContent` 已经存在，不要重复注册

自定义 `HasContent` 会触发 CS0108（隐藏基类成员，且本仓库把警告当错误）。
`ContentControl` 的内置 `HasContent` 会自动跟随 `Content`，直接 `Trigger Property="HasContent"` 即可。

### N3. `Control` 派生类要能接子元素，必须标 `[ContentProperty("Content")]`

`Badge` 继承 `Control`（不是 `ContentControl`），XAML 里写子元素会报 MC3028。
加 `[ContentProperty("Content")]` 后 `<antd:Badge Count="5"><TextBlock/></antd:Badge>` 才成立。

### 补充：`Geometry.Parse` 返回的是**已冻结**的几何

图标归一化时不能 `geometry.Transform = ...`（抛「对象处于只读状态」）。
正确做法是 `GeometryGroup { Transform = ..., Children = { parsed } }` 再冻结。

---

## 4. 验证方式的一次修正

本轮一开始根据缩小后的整页截图判断「进度条溢出卡片」，实际测量（`ActualWidth` = 910.4，
卡片内容区 = 958.4 − 48 = 910.4）证明布局精确正确，是低分辨率截图的误读。

**结论**：整页截图只能看宏观结构，判断细节必须用「单卡片 2× 放大」截图或直接打印测量值。
冒烟测试现在会额外输出 `zoom-inputs.png` / `zoom-progress.png` / `zoom-display.png` 三张局部放大图。

---

## 5. M1 剩余

| 组件 | 依赖 |
|---|---|
| `Typography`（Text/Title/Paragraph/Link） | 无，最易 |
| `Flex` / `Grid`（Row/Col，24 栅格 + 断点） | `FlexPanel` |
| `Layout`（Header/Sider/Content/Footer） | `Grid` |
| `Splitter` | `Grid` |
| `FloatButton` / `Affix` / `Watermark` / `BorderBeam` | 无 |

建议顺序：Typography → FlexPanel/Flex → Grid → Layout/Splitter → 其余零散组件。
栅格涉及响应式断点（480/576/768/992/1200/1600/1920），需要 `SizeChanged` 监听与断点缓存，
是本批次之后最复杂的一块。
