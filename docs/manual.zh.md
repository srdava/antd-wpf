# Antd.Wpf 使用手册

> Ant Design 规范的 WPF 实现 · 1.1.0 · **中文** · [English](manual.en.md)

本手册的参数表由 `tools/docs-gen` 从编译后的程序集生成，不会与代码脱节。

## 基础

Ant Design 的 WPF 实现，同时支持 .NET Framework 4.8 与 .NET 8，零第三方依赖。
控件的颜色与尺寸全部取自与 antd 同一套设计 token，换主题是替换一份不可变快照，而不是改样式表。

| 项 | 取值 |
|---|---|
| 支持框架 | `net48`、`net8.0-windows` |
| 包 | `Antd.Wpf.Controls`（自动带 Theme / Icons / Tokens） |
| 设计 token | 536 个，与 antd `theme.getDesignToken()` 逐值对拍 |
| 图标 | 847 个（outlined / filled / twotone，twotone 合并为单色） |
| 无障碍 | 每个可构造控件都有 AutomationPeer；统一焦点环；键盘可达 |

## 快速开始

```bash
dotnet add package Antd.Wpf.Controls --version 1.1.0
```

合并资源字典，让控件模板能取到 token：

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

启动时初始化一次主题：

```csharp
ThemeManager.Current.Attach(Application.Current.Resources);
ThemeManager.Current.Apply(ThemeConfig.CreateDefault());

AntdLocale.UseCulture("zh-CN");   // optional
AntdMotion.UseSystemDefault();    // optional
```

在窗口上加 `xmlns:antd="clr-namespace:Antd.Wpf.Controls;assembly=Antd.Wpf.Controls"`。

## 主题

```csharp
ThemeManager.Current.ApplyLight();
ThemeManager.Current.ApplyDark();
ThemeManager.Current.UpdateSeed(s => s.ColorPrimary = AntdColor.Parse("#00b96b"));
ThemeManager.Current.Apply(ThemeConfig.Create(null, ThemeAlgorithm.Dark | ThemeAlgorithm.Compact));
```

XAML 里引用 token（`{DynamicResource Antd.colorPrimary}`），用 `StaticResource` 不会跟随切换；代码里读 `ThemeManager.Current.Snapshot.Tokens`。

## 本地化与无障碍

- `AntdLocale.UseCulture("zh-CN")` 切中文文案；`AntdStrings.CreateEnglish()` 切英文。
- `AntdMotion.Enabled = false` 关闭弹窗、下拉、轮播动效。
- 键盘：Tab 可达所有控件；焦点环来自 `Antd.FocusVisual`。

## 组件索引

**通用**

| 组件 | 用途 |
|---|---|
| `Button` | 主/默认/虚线/文本/链接五种变体，含危险态、图标与加载态。 |
| `Icon` | 用 Geometry 绘制，颜色跟随 Foreground；847 个图标可用 AntdIcons.ByName 取。 |
| `Text` | Typography 的正文，支持次要/成功/警告/危险语义色。 |
| `Title` | Typography 的标题，1–5 级。 |
| `Paragraph` | 多行正文，可带省略与复制。 |
| `Link` | 带下划线与悬停态的文本链接。 |
| `Divider` | 水平/垂直分割，可带文字。 |
| `Space` | 统一间距排列子元素，边缘不会多出一份。 |
| `Flex` | 一维排布，支持对齐、换行与间隔。 |
| `Watermark` | 在内容上叠加倾斜文字或图片水印。 |
| `BorderBeam` | 沿边框流动的高亮描边，用于强调卡片。 |

**数据录入**

| 组件 | 用途 |
|---|---|
| `Input` | 单行输入，带清除按钮与三种尺寸。 |
| `AutoComplete` | 输入时给出候选，上下键选择。 |
| `Cascader` | 多级联动选择，逐列展开。 |
| `Checkbox` | 支持半选（IsThreeState）与禁用。 |
| `ColorPicker` | 预设色板 + 常用颜色，弹出面板选择。 |
| `DatePicker` | 单日期选择，面板与输入框联动。 |
| `Form` | 标签布局、必填校验与错误提示；支持横向布局与标签列宽。 |
| `InputNumber` | 带步进按钮，外部赋值也会收敛到声明范围。 |
| `Mentions` | 输入 @ 触发候选。 |
| `Radio` | 同组互斥选择。 |
| `Rate` | 星级评分，支持半星；方向键可调。 |
| `Select` | 单选下拉，支持清除与占位。 |
| `Slider` | 连续或分段取值。 |
| `Switch` | 两种状态的即时切换。 |
| `TimePicker` | 时分秒选择面板。 |
| `Transfer` | 两栏列表之间移动条目。 |
| `TreeSelect` | 下拉里的树形选择。 |
| `Upload` | 拖拽/点选收集文件并列出，传输由宿主完成；拖拽区键盘可操作。 |
| `Segmented` | 多选项的紧凑切换。 |

**布局**

| 组件 | 用途 |
|---|---|
| `Layout` | 页面骨架，配合 Sider / Header / Footer / Content 使用。 |
| `Sider` | 可折叠侧边栏，折叠后保留图标区。 |
| `Header` | 顶部条，默认高度与内边距按 token。 |
| `Footer` | 底部条。 |
| `Content` | 主内容区，带 token 内边距。 |
| `Row` | 24 栅格的行容器，配合 Col 使用。 |
| `Col` | 栅格列，支持 span / offset。 |
| `Masonry` | 按列高度自动分配的多列布局。 |
| `Splitter` | 可拖拽调整两栏尺寸。 |
| `Affix` | 滚动到阈值后把内容固定在顶部。 |

**导航**

| 组件 | 用途 |
|---|---|
| `Anchor` | 页面内跳转，滚动时高亮当前段落。 |
| `Breadcrumb` | 层级路径，自动插入分隔符。 |
| `Dropdown` | 点击/悬停触发，菜单项复用 MenuNode 数据。 |
| `Menu` | 垂直或水平菜单，支持多级与选中态；方向键可操作。 |
| `Pagination` | 页码、上一页/下一页与总数，超出时折叠省略号。 |
| `Steps` | 流程进度，支持当前步与状态色。 |
| `Tabs` | 横向切换内容区，支持卡片式与居中。 |
| `Tree` | 层级数据展示与选择，键盘可导航。 |

**数据展示**

| 组件 | 用途 |
|---|---|
| `Avatar` | 图片、图标或文字头像，支持组。 |
| `Badge` | 红点或数字角标，可独立展示。 |
| `Calendar` | 月视图，日期格可键盘导航。 |
| `Card` | 内容容器，带标题、额外操作与操作区。 |
| `Carousel` | 轮播内容，可自动播放（减动效时停）。 |
| `Collapse` | 手风琴式展开；头部可聚焦，空格/回车切换。 |
| `Descriptions` | 键值对信息展示，支持多列。 |
| `Empty` | 占位插图与说明文案。 |
| `Image` | 带预览的图片。 |
| `List` | 纵向条目，支持标题、描述与操作。 |
| `QRCode` | 生成二维码图形，可设颜色与纠错级别。 |
| `Statistic` | 大字号数值 + 前缀/后缀。 |
| `Table` | 基于 DataGrid 重绘，保留排序、列宽与虚拟化。 |
| `Tag` | 状态色标签，可关闭。 |
| `Timeline` | 纵向事件节点。 |
| `Tooltip` | 悬停/聚焦时的简短说明，已带阴影。 |
| `Tour` | 分步高亮目标元素。 |

**反馈**

| 组件 | 用途 |
|---|---|
| `Alert` | 四种语义状态，可带标题、描述与关闭。 |
| `Drawer` | 从四边滑出，入退场带方向滑动。 |
| `Modal` | 遮罩 + 卡片，焦点循环、Esc 关闭、退场动效。 |
| `Popconfirm` | 行内二次确认，面板带阴影。 |
| `Popover` | 悬停/点击浮现的富内容面板。 |
| `Progress` | 线形与环形进度。 |
| `Result` | 操作结果的整页反馈，四种状态。 |
| `Skeleton` | 加载占位，支持头像与段落形态。 |
| `Spin` | 旋转指示器，可包裹内容。 |

## 组件详解

### 按钮 `Button`

主/默认/虚线/文本/链接五种变体，含危险态、图标与加载态。

```xml
<antd:Button Content="确定" Variant="Primary" Click="OnOk" />
<antd:Button Content="取消" />
<antd:Button Content="删除" Danger="True" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Block` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `Danger` | Boolean | false |
| `HasIcon` | Boolean | false |
| `Icon` | Object | null |
| `IconMargin` | Thickness | 0,0,0,0 |
| `IconPlacement` | ButtonIconPlacement | ButtonIconPlacement |
| `IsDashed` | Boolean | false |
| `Loading` | Boolean | false |
| `OutlineThickness` | Double | 1 |
| `Shape` | ButtonShape | ButtonShape |
| `Size` | AntdSize | AntdSize |
| `Variant` | ButtonVariant | ButtonVariant |

### 图标 `Icon`

用 Geometry 绘制，颜色跟随 Foreground；847 个图标可用 AntdIcons.ByName 取。

```xml
<antd:Icon />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Data` | Geometry | null |
| `Size` | Double | NaN |
| `Spin` | Boolean | false |

### 输入框 `Input`

单行输入，带清除按钮与三种尺寸。

```xml
<antd:Input Width="240" Placeholder="请输入" AllowClear="True"
           Text="{Binding Name, Mode=TwoWay}" />

<!-- 前后缀 / clear -->
<antd:Input Prefix="https://" Suffix=".com" AllowClear="True" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AllowClear` | Boolean | false |
| `CanClear` | Boolean | false |
| `ContentHeight` | Double | 22 |
| `CornerRadius` | CornerRadius | CornerRadius |
| `HasPrefix` | Boolean | false |
| `HasSuffix` | Boolean | false |
| `HostHeight` | Double | 30 |
| `IsFocusedState` | Boolean | false |
| `Placeholder` | String | "" |
| `PlaceholderBrush` | Brush | theme token |
| `Prefix` | Object | null |
| `ShowPlaceholder` | Boolean | false |
| `Size` | AntdSize | AntdSize |
| `Status` | AntdStatus | AntdStatus |
| `Suffix` | Object | null |

### 文本 `Text`

Typography 的正文，支持次要/成功/警告/危险语义色。

```xml
<antd:Text />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Code` | Boolean | false |
| `Disabled` | Boolean | false |
| `Italic` | Boolean | false |
| `Strong` | Boolean | false |
| `Type` | TextType | TextType |
| `Underline` | Boolean | false |

### 标题 `Title`

Typography 的标题，1–5 级。

```xml
<antd:Title />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Level` | Int32 | 1 |

### 段落 `Paragraph`

多行正文，可带省略与复制。

```xml
<antd:Paragraph />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Type` | TextType | TextType |

### 链接 `Link`

带下划线与悬停态的文本链接。

```xml
<antd:Link />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Disabled` | Boolean | false |
| `Underline` | Boolean | false |

### 分割线 `Divider`

水平/垂直分割，可带文字。

```xml
<antd:Divider />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Dashed` | Boolean | false |
| `LineBrush` | Brush | theme token |
| `Orientation` | DividerOrientation | DividerOrientation |
| `PlainText` | Boolean | false |
| `Text` | String | null |
| `TextBrush` | Brush | theme token |
| `TextFontWeight` | FontWeight | FontWeight |
| `TextLineHeight` | Double | 22 |

### 间距 `Space`

统一间距排列子元素，边缘不会多出一份。

```xml
<antd:Space />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Orientation` | Orientation | Orientation |
| `Size` | AntdSize | AntdSize |
| `Spacing` | Double | NaN |

### 弹性布局 `Flex`

一维排布，支持对齐、换行与间隔。

```xml
<antd:Flex />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Align` | FlexAlign | FlexAlign |
| `Direction` | FlexDirection | FlexDirection |
| `Gap` | Double | NaN |
| `Justify` | FlexJustify | FlexJustify |
| `Wrap` | Boolean | false |

### 水印 `Watermark`

在内容上叠加倾斜文字或图片水印。

```xml
<antd:Watermark />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Gap` | Double | 80 |
| `Rotate` | Double | -22 |
| `Text` | String | "Ant Design" |
| `TileBrush` | Brush | null |
| `WatermarkFontSize` | Double | NaN |

### 边框光效 `BorderBeam`

沿边框流动的高亮描边，用于强调卡片。

```xml
<antd:BorderBeam />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `BaseBrush` | Brush | theme token |
| `BeamBrush` | Brush | null |
| `BeamThickness` | Double | 2 |
| `CornerRadius` | CornerRadius | CornerRadius |
| `Duration` | TimeSpan | TimeSpan |

### 布局容器 `Layout`

页面骨架，配合 Sider / Header / Footer / Content 使用。

```xml
<antd:Layout />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Orientation` | Orientation | Orientation |

### 侧边栏 `Sider`

可折叠侧边栏，折叠后保留图标区。

```xml
<antd:Sider />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Collapsed` | Boolean | false |
| `CollapsedWidth` | Double | 80 |
| `SiderWidth` | Double | 200 |

### 页头 `Header`

顶部条，默认高度与内边距按 token。

```xml
<antd:Header />
```

### 页脚 `Footer`

底部条。

```xml
<antd:Footer />
```

### 内容区 `Content`

主内容区，带 token 内边距。

```xml
<antd:Content />
```

### 行 `Row`

24 栅格的行容器，配合 Col 使用。

```xml
<antd:Row />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Align` | GridAlign | GridAlign |
| `CurrentBreakpoint` | Breakpoint | Breakpoint |
| `Gutter` | Double | 0 |
| `RowGutter` | Double | 0 |

### 列 `Col`

栅格列，支持 span / offset。

```xml
<antd:Col />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Lg` | Int32 | -1 |
| `Md` | Int32 | -1 |
| `Offset` | Int32 | 0 |
| `Sm` | Int32 | -1 |
| `Span` | Int32 | 24 |
| `Xl` | Int32 | -1 |
| `Xs` | Int32 | -1 |
| `Xxl` | Int32 | -1 |

### 瀑布流 `Masonry`

按列高度自动分配的多列布局。

```xml
<antd:Masonry />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Columns` | Int32 | 3 |
| `Gutter` | Double | 16 |

### 分隔面板 `Splitter`

可拖拽调整两栏尺寸。

```xml
<antd:Splitter />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `BarBrush` | Brush | theme token |
| `BarThickness` | Double | 1 |

### 固钉 `Affix`

滚动到阈值后把内容固定在顶部。

```xml
<antd:Affix />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `IsPinned` | Boolean | false |
| `OffsetTop` | Double | 0 |

### 锚点 `Anchor`

页面内跳转，滚动时高亮当前段落。

```xml
<antd:Anchor />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `ActiveIndex` | Int32 | -1 |
| `ScrollTarget` | ScrollViewer | null |

### 面包屑 `Breadcrumb`

层级路径，自动插入分隔符。

```xml
<antd:Breadcrumb />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Separator` | String | "/" |

### 下拉菜单 `Dropdown`

点击/悬停触发，菜单项复用 MenuNode 数据。

```xml
<antd:Dropdown />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `ItemTemplate` | DataTemplate | null |
| `ItemsSource` | IEnumerable | null |

事件：`ItemInvoked`

### 导航菜单 `Menu`

垂直或水平菜单，支持多级与选中态；方向键可操作。

```xml
<antd:Menu SelectedIndex="{Binding MenuIndex, Mode=TwoWay}">
  <antd:MenuNode Header="服务">
    <antd:MenuNode Header="实例列表" />
  </antd:MenuNode>
  <antd:MenuNode Header="设置" />
</antd:Menu>
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Mode` | MenuMode | MenuMode |
| `SelectedItem` | MenuNode | null |

事件：`ItemSelected`

### 分页 `Pagination`

页码、上一页/下一页与总数，超出时折叠省略号。

```xml
<antd:Pagination Total="{Binding Total}" PageSize="10"
               CurrentPage="{Binding Page, Mode=TwoWay}" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Current` | Int32 | 1 |
| `PageSize` | Int32 | 10 |
| `Total` | Int32 | 0 |

事件：`PageChanged`

### 步骤条 `Steps`

流程进度，支持当前步与状态色。

```xml
<antd:Steps Current="{Binding Step}">
  <antd:StepsItem Title="填写" />
  <antd:StepsItem Title="校验" />
  <antd:StepsItem Title="完成" />
</antd:Steps>
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Current` | Int32 | 0 |
| `Direction` | Orientation | Orientation |

### 标签页 `Tabs`

横向切换内容区，支持卡片式与居中。

```xml
<antd:Tabs SelectedIndex="{Binding TabIndex, Mode=TwoWay}">
  <antd:TabsItem Header="概览"><TextBlock Text="…" /></antd:TabsItem>
  <antd:TabsItem Header="日志"><TextBlock Text="…" /></antd:TabsItem>
</antd:Tabs>
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `ItemGutter` | Double | 32 |

### 树形控件 `Tree`

层级数据展示与选择，键盘可导航。

```xml
<antd:Tree ItemsSource="{Binding Nodes}" DisplayMemberPath="Name" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Checkable` | Boolean | false |
| `ShowIcon` | Boolean | false |

### 自动完成 `AutoComplete`

输入时给出候选，上下键选择。

```xml
<antd:AutoComplete />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `CornerRadius` | CornerRadius | CornerRadius |
| `FilterMode` | AntdFilterMode | AntdFilterMode |
| `HasSuggestions` | Boolean | false |
| `IsDropDownOpen` | Boolean | false |
| `IsFocusedState` | Boolean | false |
| `ItemsSource` | IEnumerable | null |
| `MaxDropDownHeight` | Double | 256 |
| `Placeholder` | String | "" |
| `ShowPlaceholder` | Boolean | true |
| `Text` | String | "" |

### 级联选择 `Cascader`

多级联动选择，逐列展开。

```xml
<antd:Cascader />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AllowClear` | Boolean | true |
| `CanClear` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `DisplayText` | String | "" |
| `IsDropDownOpen` | Boolean | false |
| `ItemsSource` | IEnumerable | null |
| `Placeholder` | String | "Select" |
| `Separator` | String | " / " |
| `ShowPlaceholder` | Boolean | true |

### 复选框 `Checkbox`

支持半选（IsThreeState）与禁用。

```xml
<antd:Checkbox />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `BoxBorderBrush` | Brush | theme token |
| `BoxBrush` | Brush | theme token |
| `BoxSize` | Double | 16 |
| `CheckBrush` | Brush | theme token |
| `Size` | AntdSize | AntdSize |

### 颜色选择 `ColorPicker`

预设色板 + 常用颜色，弹出面板选择。

```xml
<antd:ColorPicker />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AllowClear` | Boolean | true |
| `CanClear` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `HexText` | String | "" |
| `HueBrush` | Brush | theme token |
| `HueOffset` | Double | 0 |
| `IsDropDownOpen` | Boolean | false |
| `MarkerX` | Double | 0 |
| `MarkerY` | Double | 0 |
| `Presets` | IEnumerable`1 | null |
| `SelectedColor` | Nullable`1 | null |
| `SwatchBrush` | Brush | theme token |

### 日期选择 `DatePicker`

单日期选择，面板与输入框联动。

```xml
<antd:DatePicker Width="220" AllowClear="True"
                SelectedDate="{Binding Date, Mode=TwoWay}" />

<!-- 区间 -->
<antd:DatePicker Width="280" IsRange="True"
                RangeStart="{Binding From, Mode=TwoWay}"
                RangeEnd="{Binding To, Mode=TwoWay}" />

<!-- 代码里加禁用规则 -->
picker.DisabledDate = d => d < DateTime.Today;
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AllowClear` | Boolean | true |
| `CanClear` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `DisabledDate` | Func`2 | null |
| `DisplayText` | String | "" |
| `Format` | String | "yyyy-MM-dd" |
| `IsDropDownOpen` | Boolean | false |
| `IsRange` | Boolean | false |
| `Placeholder` | String | "Select date" |
| `RangeEnd` | Nullable`1 | null |
| `RangeStart` | Nullable`1 | null |
| `SelectedDate` | Nullable`1 | null |
| `ShowPlaceholder` | Boolean | true |

### 表单 `Form`

标签布局、必填校验与错误提示；支持横向布局与标签列宽。

```xml
<antd:Form LabelWidth="96">
  <antd:FormItem Label="名称" Required="True">
    <antd:Input Text="{Binding Name, Mode=TwoWay}" />
  </antd:FormItem>
  <antd:FormItem Label="编码" Required="True" Width="120" />
</antd:Form>

if (!await form.ValidateAsync()) return;   // 含异步校验
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `ItemSpacing` | Double | NaN |
| `LabelWidth` | Double | 96 |
| `Layout` | FormLayout | FormLayout |

### 数字输入框 `InputNumber`

带步进按钮，外部赋值也会收敛到声明范围。

```xml
<antd:InputNumber />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `CornerRadius` | CornerRadius | CornerRadius |
| `HostHeight` | Double | 30 |
| `Maximum` | Double | 179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 |
| `Minimum` | Double | -179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 |
| `Precision` | Int32 | -1 |
| `Size` | AntdSize | AntdSize |
| `Step` | Double | 1 |
| `StepDownEnabled` | Boolean | true |
| `StepUpEnabled` | Boolean | true |
| `Value` | Nullable`1 | null |

### @提及 `Mentions`

输入 @ 触发候选。

```xml
<antd:Mentions />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `CornerRadius` | CornerRadius | CornerRadius |
| `IsDropDownOpen` | Boolean | false |
| `ItemsSource` | IEnumerable | null |
| `Placeholder` | String | "Type @ to mention" |
| `ShowPlaceholder` | Boolean | true |
| `Text` | String | "" |
| `TriggerChar` | Char | Char |

### 单选框 `Radio`

同组互斥选择。

```xml
<antd:Radio />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `DotBrush` | Brush | theme token |
| `RingBrush` | Brush | theme token |
| `RingSize` | Double | 16 |

### 评分 `Rate`

星级评分，支持半星；方向键可调。

```xml
<antd:Rate />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AllowHalf` | Boolean | false |
| `Count` | Int32 | 5 |
| `Gap` | Double | 8 |
| `IsReadOnly` | Boolean | false |
| `StarSize` | Double | 20 |
| `Value` | Double | 0 |

### 选择器 `Select`

单选下拉，支持清除与占位。

```xml
<antd:Select Width="240" Placeholder="选择状态"
             ItemsSource="{Binding States}"
             SelectedItem="{Binding State, Mode=TwoWay}" />

<!-- 可搜索 + 多选（标签折叠） -->
<antd:Select Width="320" IsMultiple="True" MaxTagCount="3"
             Filterable="True" ItemsSource="{Binding Tags}" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AllowClear` | Boolean | false |
| `ArrowBrush` | Brush | theme token |
| `CanClearSelection` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `FilterLocally` | Boolean | true |
| `Filterable` | Boolean | false |
| `HasSelection` | Boolean | false |
| `IsMultiple` | Boolean | false |
| `MaxTagCount` | Int32 | 0 |
| `Placeholder` | String | "Please select" |
| `PlaceholderBrush` | Brush | theme token |
| `SelectedItems` | ObservableCollection`1 | null |
| `Size` | AntdSize | AntdSize |
| `Status` | AntdStatus | AntdStatus |

事件：`SearchTextChanged`

### 滑动输入条 `Slider`

连续或分段取值。

```xml
<antd:Slider />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Size` | AntdSize | AntdSize |
| `ThumbSize` | Double | 14 |
| `TrackThickness` | Double | 4 |

### 开关 `Switch`

两种状态的即时切换。

```xml
<antd:Switch IsChecked="{Binding Enabled, Mode=TwoWay}" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `ActiveText` | String | null |
| `CheckedText` | String | null |
| `Size` | AntdSize | AntdSize |
| `ThumbBrush` | Brush | theme token |
| `ThumbSize` | Double | 18 |
| `TrackBrush` | Brush | theme token |
| `TrackCornerRadius` | CornerRadius | CornerRadius |
| `TrackHeight` | Double | 22 |
| `TrackWidth` | Double | 44 |
| `UncheckedText` | String | null |

### 时间选择 `TimePicker`

时分秒选择面板。

```xml
<antd:TimePicker />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AllowClear` | Boolean | true |
| `CanClear` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `DisplayText` | String | "" |
| `Format` | String | "HH:mm:ss" |
| `Hours` | IEnumerable`1 | null |
| `IsDropDownOpen` | Boolean | false |
| `Placeholder` | String | "Select time" |
| `SelectedTime` | Nullable`1 | null |
| `ShowPlaceholder` | Boolean | true |
| `ShowSeconds` | Boolean | true |
| `Sixty` | IEnumerable`1 | null |

### 穿梭框 `Transfer`

两栏列表之间移动条目。

```xml
<antd:Transfer />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `ItemsSource` | IEnumerable | null |
| `SourceCount` | String | "0 items" |
| `SourceTitle` | String | "Source" |
| `TargetCount` | String | "0 items" |
| `TargetTitle` | String | "Target" |

事件：`Changed`

### 树选择 `TreeSelect`

下拉里的树形选择。

```xml
<antd:TreeSelect />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AllowClear` | Boolean | true |
| `CanClear` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `DisplayText` | String | "" |
| `IsDropDownOpen` | Boolean | false |
| `ItemsSource` | IEnumerable | null |
| `Placeholder` | String | "Select" |
| `SelectedItem` | TreeItem | null |
| `ShowPlaceholder` | Boolean | true |

### 上传 `Upload`

拖拽/点选收集文件并列出，传输由宿主完成；拖拽区键盘可操作。

```xml
<antd:Upload Accept=".csv,.xlsx" AutoUpload="True"
            Uploader="UploadFileAsync" />

// 库只跑队列，传输函数由宿主提供
upload.Uploader = async (file, progress, token) =>
    await api.UploadAsync(file.Path, progress, token);
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Accept` | String | null |
| `ButtonText` | String | "Upload" |
| `HasFiles` | Boolean | false |
| `Hint` | String | "Click or drag files here" |
| `IsDraggingOver` | Boolean | false |
| `Multiple` | Boolean | true |

事件：`FileSelected`

### 分段控制器 `Segmented`

多选项的紧凑切换。

```xml
<antd:Segmented />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Block` | Boolean | false |
| `ItemCornerRadius` | CornerRadius | CornerRadius |
| `ItemFontSize` | Double | 14 |
| `TrackCornerRadius` | CornerRadius | CornerRadius |

### 头像 `Avatar`

图片、图标或文字头像，支持组。

```xml
<antd:Avatar />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `AvatarBackground` | Brush | theme token |
| `AvatarCornerRadius` | CornerRadius | CornerRadius |
| `AvatarFontSize` | Double | 14 |
| `AvatarForeground` | Brush | theme token |
| `HasImage` | Boolean | false |
| `Shape` | AntdAvatarShape | AntdAvatarShape |
| `Size` | Double | 32 |
| `Source` | ImageSource | null |

### 徽标数 `Badge`

红点或数字角标，可独立展示。

```xml
<antd:Badge />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `BadgeBackground` | Brush | theme token |
| `BadgeFontSize` | Double | 12 |
| `Content` | Object | null |
| `Count` | Int32 | 0 |
| `CountColor` | Brush | null |
| `DisplayText` | String | "" |
| `Dot` | Boolean | false |
| `IsBadgeVisible` | Boolean | false |
| `OverflowCount` | Int32 | 99 |
| `ShowZero` | Boolean | false |

### 日历 `Calendar`

月视图，日期格可键盘导航。

```xml
<antd:Calendar />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `CornerRadius` | CornerRadius | CornerRadius |
| `Days` | ObservableCollection`1 | null |
| `DisplayDate` | DateTime | DateTime |
| `MonthLabel` | String | "" |
| `SelectedDate` | Nullable`1 | null |
| `WeekDays` | String[] | null |

事件：`SelectedDateChanged`

### 卡片 `Card`

内容容器，带标题、额外操作与操作区。

```xml
<antd:Card />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Bordered` | Boolean | true |
| `CornerRadius` | CornerRadius | CornerRadius |
| `Extra` | Object | null |
| `HasHeader` | Boolean | false |
| `Title` | Object | null |

### 走马灯 `Carousel`

轮播内容，可自动播放（减动效时停）。

```xml
<antd:Carousel />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Autoplay` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `Interval` | Int32 | 3000 |
| `SelectedIndex` | Int32 | 0 |
| `SelectedItem` | Object | null |
| `ShowDots` | Boolean | true |

### 折叠面板 `Collapse`

手风琴式展开；头部可聚焦，空格/回车切换。

```xml
<antd:Collapse>
  <antd:CollapseItem Header="基础信息" IsExpanded="True">
    <TextBlock Text="…" />
  </antd:CollapseItem>
</antd:Collapse>
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Accordion` | Boolean | false |
| `Bordered` | Boolean | true |
| `CornerRadius` | CornerRadius | CornerRadius |

### 描述列表 `Descriptions`

键值对信息展示，支持多列。

```xml
<antd:Descriptions />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Bordered` | Boolean | false |
| `Column` | Int32 | 3 |

### 空状态 `Empty`

占位插图与说明文案。

```xml
<antd:Empty />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Description` | String | null |
| `DescriptionBrush` | Brush | theme token |
| `Image` | Geometry | GeometryGroup |
| `ImageBrush` | Brush | theme token |

### 图片 `Image`

带预览的图片。

```xml
<antd:Image />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `CornerRadius` | CornerRadius | CornerRadius |
| `IsPreviewVisible` | Boolean | false |
| `Preview` | Boolean | true |
| `Source` | ImageSource | null |
| `Stretch` | Stretch | Stretch |

### 列表 `List`

纵向条目，支持标题、描述与操作。

```xml
<antd:List />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Bordered` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |

### 二维码 `QRCode`

生成二维码图形，可设颜色与纠错级别。

```xml
<antd:QRCode />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `ErrorCorrection` | QrEcc | QrEcc |
| `ModuleBrush` | Brush | theme token |
| `QuietZone` | Int32 | 4 |
| `Value` | String | "" |

### 统计数值 `Statistic`

大字号数值 + 前缀/后缀。

```xml
<antd:Statistic />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Precision` | Int32 | -1 |
| `Prefix` | Object | null |
| `Suffix` | Object | null |
| `Title` | String | null |
| `TitleBrush` | Brush | theme token |
| `Value` | Double | 0 |
| `ValueBrush` | Brush | theme token |
| `ValueFontSize` | Double | 24 |
| `ValueText` | String | "0" |

### 表格 `Table`

基于 DataGrid 重绘，保留排序、列宽与虚拟化。

```xml
<antd:Table ItemsSource="{Binding Rows}" FrozenColumnCount="2">
  <antd:Table.Columns>
    <DataGridTextColumn Header="名称" Binding="{Binding Name}" Width="180" />
    <DataGridTextColumn Header="状态" Binding="{Binding State}" Width="120" />
  </antd:Table.Columns>
</antd:Table>

<!-- 树形行：指定子集合属性即可 -->
<antd:Table ItemsSource="{Binding Nodes}"
            TreeChildrenPath="Children" TreeTextPath="Name" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Bordered` | Boolean | true |
| `EmptyText` | String | null |
| `HasRows` | Boolean | false |
| `ShowRowDetails` | Boolean | false |
| `TreeChildrenPath` | String | null |
| `TreeIndent` | Double | 16 |
| `TreeTextPath` | String | null |

### 标签 `Tag`

状态色标签，可关闭。

```xml
<antd:Tag />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Bordered` | Boolean | true |
| `Closable` | Boolean | false |
| `Color` | AntdTagColor | AntdTagColor |
| `TagBackground` | Brush | theme token |
| `TagBorderBrush` | Brush | theme token |
| `TagCornerRadius` | CornerRadius | CornerRadius |
| `TagFontSize` | Double | 12 |
| `TagForeground` | Brush | theme token |

事件：`Closed`

### 时间轴 `Timeline`

纵向事件节点。

```xml
<antd:Timeline />
```

### 文字提示 `Tooltip`

悬停/聚焦时的简短说明，已带阴影。

```xml
<antd:Tooltip />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Text` | String | "" |

### 漫游式引导 `Tour`

分步高亮目标元素。

```xml
<antd:Tour />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Current` | Int32 | 0 |
| `CurrentDescription` | String | "" |
| `CurrentTitle` | String | "" |
| `IsLastStep` | Boolean | false |
| `IsOpen` | Boolean | false |
| `PositionText` | String | "" |
| `StepCount` | Int32 | 0 |

事件：`Finished`

### 警告提示 `Alert`

四种语义状态，可带标题、描述与关闭。

```xml
<antd:Alert Type="Success" Message="保存成功"
            Description="改动已同步到服务器。" ShowClose="True" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Action` | Object | null |
| `ActionVisibility` | Visibility | Visibility |
| `Banner` | Boolean | false |
| `Closable` | Boolean | false |
| `CloseVisibility` | Visibility | Visibility |
| `CornerRadius` | CornerRadius | CornerRadius |
| `Description` | String | null |
| `DescriptionVisibility` | Visibility | Visibility |
| `HasDescription` | Boolean | false |
| `IconBrush` | Brush | null |
| `IconGeometry` | Geometry | null |
| `IconSize` | Double | 14 |
| `IconVisibility` | Visibility | Visibility |
| `Message` | String | "" |
| `MessageFontSize` | Double | 14 |
| `MessageLineHeight` | Double | 22 |
| `ShowIcon` | Boolean | true |
| `Type` | AlertType | AlertType |

事件：`Closed`

### 抽屉 `Drawer`

从四边滑出，入退场带方向滑动。

```xml
<antd:Drawer />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `DrawerHeight` | Double | 378 |
| `DrawerWidth` | Double | 378 |
| `Footer` | Object | null |
| `FooterVisibility` | Visibility | Visibility |
| `Placement` | AntdPlacement | AntdPlacement |
| `Title` | Object | null |

### 对话框 `Modal`

遮罩 + 卡片，焦点循环、Esc 关闭、退场动效。

```xml
<!-- 声明式 -->
<antd:Modal x:Name="EditModal" Title="编辑" OkText="保存"
            Ok="OnSave" Canceled="OnCancel" />

<!-- 命令式：确认框不必写 XAML -->
Modal.Confirm("删除确认", "确定要删除这条记录吗？", () => Delete(), () => { });
Modal.Info("提示", "保存成功");
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `CancelText` | String | null |
| `Centered` | Boolean | false |
| `CustomFooterVisibility` | Visibility | Visibility |
| `DefaultFooterVisibility` | Visibility | Visibility |
| `DialogWidth` | Double | 520 |
| `Footer` | Object | null |
| `OkLoading` | Boolean | false |
| `OkText` | String | null |
| `ShowCancel` | Boolean | true |
| `ShowFooter` | Boolean | true |
| `Title` | Object | null |

事件：`Canceled`, `Ok`

### 气泡确认 `Popconfirm`

行内二次确认，面板带阴影。

```xml
<antd:Popconfirm />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `CancelText` | String | null |
| `ConfirmText` | String | null |
| `Description` | Object | null |
| `DescriptionVisibility` | Visibility | Visibility |
| `IconBrush` | Brush | null |
| `IconGeometry` | Geometry | GeometryGroup |
| `IconVisibility` | Visibility | Visibility |
| `ShowCancel` | Boolean | true |
| `ShowIcon` | Boolean | false |
| `Title` | Object | null |

事件：`Canceled`, `Confirmed`

### 气泡卡片 `Popover`

悬停/点击浮现的富内容面板。

```xml
<antd:Popover />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Body` | Object | null |
| `HasTitle` | Boolean | false |
| `Title` | Object | null |

### 进度条 `Progress`

线形与环形进度。

```xml
<antd:Progress Percent="{Binding Percent}" />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `CircleSize` | Double | 120 |
| `Percent` | Double | 0 |
| `ShowInfo` | Boolean | true |
| `Status` | AntdProgressStatus | AntdProgressStatus |
| `StrokeWidth` | Double | NaN |
| `Type` | AntdProgressType | AntdProgressType |

### 结果页 `Result`

操作结果的整页反馈，四种状态。

```xml
<antd:Result />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `ContentVisibility` | Visibility | Visibility |
| `Extra` | Object | null |
| `ExtraVisibility` | Visibility | Visibility |
| `IconBrush` | Brush | null |
| `IconGeometry` | Geometry | null |
| `IconVisibility` | Visibility | Visibility |
| `Status` | ResultStatus | ResultStatus |
| `StatusText` | String | "" |
| `StatusTextVisibility` | Visibility | Visibility |
| `SubTitle` | String | null |
| `SubTitleVisibility` | Visibility | Visibility |
| `Title` | String | null |
| `TitleVisibility` | Visibility | Visibility |

### 骨架屏 `Skeleton`

加载占位，支持头像与段落形态。

```xml
<antd:Skeleton />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `Active` | Boolean | false |
| `Avatar` | Boolean | false |
| `Rows` | Int32 | 3 |
| `Title` | Boolean | true |

### 加载中 `Spin`

旋转指示器，可包裹内容。

```xml
<antd:Spin />
```

| 参数 | 类型 | 默认值 |
|---|---|---|
| `IndicatorBrush` | Brush | theme token |
| `IndicatorSize` | Double | 20 |
| `ShowsTip` | Boolean | false |
| `Size` | AntdSize | AntdSize |
| `Spinning` | Boolean | true |
| `Tip` | String | null |
| `TipBrush` | Brush | theme token |

## 常见问题

| 现象 | 原因 | 处理 |
|---|---|---|
| 输入框文字被裁 | `TextBox.Padding` 被复制到内容宿主 | 1.0 已修；自定义输入控件照抄 `HostHeight` 的做法 |
| 切主题无效 | 样式里用了 `StaticResource` | 换成 `{DynamicResource Antd.*}` |
| 下拉没有阴影 | 只有 Select 与 OverlayBase 系列做了阴影槽 | DatePicker / TimePicker / Cascader 仍未做 |
| 看不到焦点环 | 自定义模板去掉了 `FocusVisualStyle` | 引用 `Antd.FocusVisual` |

另见：[迁移指南](migration.md) · [能力边界](progress.md) · [组件文档站](site/index.html)

