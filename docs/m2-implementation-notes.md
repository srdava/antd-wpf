# M2 实施记录：M1 收尾 + 数据录入（进行中）

| 项目 | 内容 |
|---|---|
| 日期 | 2026-11-03 |
| 本轮范围 | M1 剩余 12 个类型 + M2 表单控件 10 个 |
| 控件总数 | 14 → **36** |
| 验证 | 对拍 536×10×2 全等；冒烟 **82 项断言** ×2 框架全过 |
| M2 状态 | **未完成**。8 个复杂组件待做（见 §5） |

---

## 1. 本轮新增

### M1 剩余

| 组件 | 说明 |
|---|---|
| `Text` / `Title` / `Paragraph` / `Link` | 均派生自 `TextBlock`，文字渲染、换行、截断保持原生；只有颜色/字号/装饰走 token |
| `Flex` | 自研 Panel，Direction/Justify/Align/Gap。**未实现 flex-grow**，见 §5 |
| `Row` / `Col` | 24 栅格，`Gutter`、`RowGutter`、`Offset`、响应式 `Xs/Sm/Sd/Lg/Xl/Xxl`，断点 6 档 |
| `Layout` / `Sider` / `Header` / `Footer` / `Content` | `Layout` 自动识别 Sider 决定横竖；`Content` 与嵌套 `Layout` 吸收剩余空间 |
| `FloatButton` | 圆/方，`Placement` 映射为对齐 + 外边距（WPF 无 fixed 定位） |
| `Watermark` | `DrawingBrush` 平铺，一个画刷而不是 N 个可视元素 |

### M2 数据录入

| 组件 | 说明 |
|---|---|
| `InputNumber` | 派生 `TextBox`，step/min/max/precision，上下步进按钮 |
| `Checkbox` | 派生 `ToggleButton`，含半选态 |
| `Radio` | 派生 `RadioButton`，`GroupName` 天然分组 |
| `Switch` | 派生 `ToggleButton`，选中/未选中文案，三档尺寸 |
| `Rate` | `OnRender` 画五角星，支持半星、悬停预览 |
| `Slider` | 派生 `Slider`，模板用 `Track` 元件，拖拽/键盘全部复用框架 |
| `Select` | 派生 `ComboBox`，弹出层重绘为 antd 样式，支持清除 |
| `Form` / `FormItem` | 垂直/水平布局、必填标记、错误文案、`Validate()` 汇总 |

---

## 2. 本轮最重要的一条 WPF 教训

### `DefaultStyleKey` 不覆盖，主题字典里的样式永远不会生效

`Sider` / `Header` / `Footer` / `Content` / `Col` 五个类我写了构造函数却没有静态构造函数覆盖
`DefaultStyleKeyProperty`。结果它们一直在用 **`ContentControl` 的默认模板**：

- 自定义模板完全无用（Padding、配色都不生效）
- 表现是「文字贴着控件左上角」，看起来像布局算错了

排查花了很久，因为症状和「布局算法错误」一模一样。**实测坐标**才对上：
Header 文本在 (0,0) 且尺寸等于整个 Header —— 说明压根没走模板。

规则：**任何 `Control` / `ContentControl` 派生类，只要有对应的隐式样式，就必须有**

```csharp
static Xxx()
{
    DefaultStyleKeyProperty.OverrideMetadata(typeof(Xxx),
        new FrameworkPropertyMetadata(typeof(Xxx)));
}
```

可以在 CI 里加一条检查：凡是在 Generic.xaml 里有 `TargetType` 样式、却没有该静态构造的类型，直接报错。

### 附带发现：`dotnet run` 的增量构建可能吃掉 XAML 改动

改了 `Generic.xaml` 后必须 `dotnet build --no-incremental` 才能确保生效，否则会对着旧界面调试。
（本轮就是先改 Padding 无效果，误判为模板绑定问题，实际是旧产物。）

---

## 3. 其他新增约束（累计 7 条）

| # | 约束 | 症状 |
|---|---|---|
| N4 | `DefaultStyleKey` 必须覆盖 | 自定义模板静默失效，见 §2 |
| N5 | `ContentControl.Padding` 不会被 `ContentPresenter` 自动使用 | 必须 `Border Padding="{TemplateBinding Padding}"` 或给 presenter 加 Margin |
| N6 | `GridLength "*"` 不能用于 `Height`/`Width` | 抛 `FormatException: 无法将"*"转换为 Length`，要用两行 Grid |
| N7 | `TextBlock` 子类里 `TextDecorations` 指向实例属性 | `TextDecorations.Underline` 会被解析成属性，需全限定 `System.Windows.TextDecorations` |

加上此前的：Setter.Value 不能放 Binding/TemplateBinding、模板子元素本地 Visibility 压死触发器、
`DataTrigger` 绑 `TemplatedParent` 的 bool 不可靠 —— 共 7 条，都已写进代码注释。

---

## 4. 验证方式的再次修正

本轮两次被「看起来错」的截图带偏，两次都被实测数据推翻：

1. 布局预览看起来错位 → 打印子元素坐标，实际 Header/Sider/Content 位置完全正确。
2. 表单控件看起来居中 → 是 WPF「显式宽度 + Stretch 表现为居中」的既有行为，不是布局 bug。

**结论**：截图只用于看宏观结构与观感，几何问题一律以 `ActualWidth/ActualHeight/TranslatePoint`
的打印值为准。本轮之后，冒烟测试里的截图不再作为判断依据，只作为观感留档。

---

## 5. M2 未完成部分（8 个）

| 组件 | 未做原因 |
|---|---|
| `DatePicker` / `TimePicker` / `Calendar` | 需要自研日历面板与区间选择，工作量约 10+4+4 人日 |
| `Cascader` / `TreeSelect` | 依赖 Tree 控件（M3）与多级浮层 |
| `ColorPicker` | 色板/色相条/取色器，独立子系统 |
| `Upload` | 拖拽、文件列表、进度，依赖 Store 抽象 |
| `Transfer` | 双列表穿梭 + 分页 |
| `Mentions` / `AutoComplete` | 可基于 Select 扩展，但需要可编辑 + 候选浮层 |

### 已知能力缺口（本轮实现里明确限制）

| 组件 | 缺口 |
|---|---|
| `Flex` | 无 `flex-grow` / `flex-shrink` / `Wrap`；宽高比由内容决定 |
| `Select` | 仅单选、无搜索、无虚拟化、无分组 |
| `Form` | 无字段依赖图、无异步校验、无 `initialValues` 绑定 |
| `Row` | 响应式只有 span，没有 `offset/order` 的响应式覆盖 |
| `Rate` | 无自定义字符（只画五角星） |

---

## 6. 下一步建议

1. **先补齐 M2 的输入类**：`AutoComplete`（基于 Select + 可编辑）、`Mentions`、`Upload`。
2. **再攻日期**：`Calendar` → `DatePicker` → `TimePicker`，三者共用日历面板。
3. **`Tree` 优先级应提前**：`TreeSelect`、`Cascader`、`Menu`、`Table` 的树形行都依赖它。
4. 每批仍按「先断言、再模板、最后看观感」的节奏，几何问题只信打印值。
