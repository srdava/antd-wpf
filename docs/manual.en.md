# Antd.Wpf Handbook

> Ant Design for WPF · 1.2.0 · **English** · [中文](manual.zh.md)

Property tables in this handbook are generated from the compiled assembly (`tools/docs-gen`), so they cannot drift from the code.

## Basics

A WPF port of Ant Design that runs on .NET Framework 4.8 and .NET 8 with zero third-party dependencies.
Controls read colours and metrics from the same design tokens as antd, so switching a theme replaces one immutable snapshot instead of rewriting styles.

| Item | Value |
|---|---|
| Frameworks | `net48`, `net8.0-windows` |
| Packages | `Antd.Wpf.Controls` (pulls Theme / Icons / Tokens) |
| Tokens | 536, value-compared against antd's `theme.getDesignToken()` |
| Icons | 847 (outlined / filled / two-tone flattened to one colour) |
| Accessibility | AutomationPeer on every constructible control, shared focus ring, keyboard paths |

## Getting started

```bash
dotnet add package Antd.Wpf.Controls --version 1.2.0
```

Merge the resource dictionary so the control templates can resolve tokens:

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

Initialise the theme once at startup:

```csharp
ThemeManager.Current.Attach(Application.Current.Resources);
ThemeManager.Current.Apply(ThemeConfig.CreateDefault());

AntdLocale.UseCulture("zh-CN");   // optional
AntdMotion.UseSystemDefault();    // optional
```

Add `xmlns:antd="clr-namespace:Antd.Wpf.Controls;assembly=Antd.Wpf.Controls"` to the window.

## Theming

```csharp
ThemeManager.Current.ApplyLight();
ThemeManager.Current.ApplyDark();
ThemeManager.Current.UpdateSeed(s => s.ColorPrimary = AntdColor.Parse("#00b96b"));
ThemeManager.Current.Apply(ThemeConfig.Create(null, ThemeAlgorithm.Dark | ThemeAlgorithm.Compact));
```

Reference tokens in XAML (`{DynamicResource Antd.colorPrimary}`); `StaticResource` will not follow a switch. Code-behind reads `ThemeManager.Current.Snapshot.Tokens`.

## Localisation and accessibility

- `AntdLocale.UseCulture("zh-CN")` switches built-in strings; `AntdStrings.CreateEnglish()` for English.
- `AntdMotion.Enabled = false` stops dialog, dropdown and carousel motion.
- Keyboard: Tab reaches every control; the focus ring comes from `Antd.FocusVisual`.

## Index

**General**

| Component | Purpose |
|---|---|
| `Button` | Five variants (default/primary/dashed/text/link) with danger, icon and loading states. |
| `Icon` | Drawn as Geometry and tinted by Foreground; 847 glyphs via AntdIcons.ByName. |
| `Text` | Body copy with secondary/success/warning/danger tones. |
| `Title` | Heading levels 1-5. |
| `Paragraph` | Multi-line body copy with ellipsis and copy support. |
| `Link` | Text link with underline and hover states. |
| `Divider` | Horizontal or vertical rule, optionally with a caption. |
| `Space` | Even gaps between children without a trailing edge. |
| `Flex` | One-dimensional layout with alignment, wrapping and gaps. |
| `Watermark` | Tilted text or image watermark over the content. |
| `BorderBeam` | Highlight travelling along a card border. |

**Data Entry**

| Component | Purpose |
|---|---|
| `Input` | Single-line input with a clear button and three sizes. |
| `AutoComplete` | Suggests entries while typing; arrow keys pick. |
| `Cascader` | Cascading picker, one column per level. |
| `Checkbox` | Checkbox with an indeterminate state and disabled support. |
| `ColorPicker` | Preset swatches plus a hue strip in a popup. |
| `DatePicker` | Single date, or a two-month range with DisabledDate rules. |
| `Form` | Label layout, required rules and error text; horizontal mode and label width. |
| `InputNumber` | Stepper input that clamps even when the value is set from code. |
| `Mentions` | Mention suggestions triggered by @. |
| `Radio` | Mutually exclusive choice inside a group. |
| `Rate` | Star rating with half stars; arrow keys adjust. |
| `Select` | Single or multiple selection with tag folding, local/remote search and virtualisation. |
| `Slider` | Continuous or stepped value picker. |
| `Switch` | Immediate two-state toggle. |
| `TimePicker` | Hour / minute / second panel. |
| `Transfer` | Move items between two lists. |
| `TreeSelect` | Tree picker inside a dropdown. |
| `Upload` | Collects files by drop or pick with a queue, progress and cancel; transport comes from the host. |
| `Segmented` | Compact multi-option switch. |

**Layout**

| Component | Purpose |
|---|---|
| `Layout` | Page shell used with Sider / Header / Footer / Content. |
| `Sider` | Collapsible side bar that keeps an icon rail when collapsed. |
| `Header` | Top bar with token-driven height and padding. |
| `Footer` | Bottom bar. |
| `Content` | Main content area with token padding. |
| `Row` | 24-column grid row, used with Col. |
| `Col` | Grid column with span and offset. |
| `Masonry` | Multi-column layout that fills the shortest column. |
| `Splitter` | Draggable divider between two panes. |
| `Affix` | Pins content to the top after a scroll threshold. |

**Navigation**

| Component | Purpose |
|---|---|
| `Anchor` | In-page navigation that highlights the current section. |
| `Breadcrumb` | Hierarchy trail with automatic separators. |
| `Dropdown` | Click or hover trigger whose rows reuse MenuNode data. |
| `Menu` | Vertical or horizontal menu with nesting and selection; arrow keys work. |
| `Pagination` | Page numbers with prev/next and an ellipsis for long ranges. |
| `Steps` | Process progress with a current step and status colours. |
| `Tabs` | Horizontal tab strip, line or centred style. |
| `Tree` | Hierarchical display and selection, keyboard navigable. |

**Data Display**

| Component | Purpose |
|---|---|
| `Avatar` | Image, icon or text avatar, groupable. |
| `Badge` | Dot or numeric badge, standalone too. |
| `Calendar` | Month grid with keyboard navigation and DisabledDate. |
| `Card` | Content container with title, extra slot and actions. |
| `Carousel` | Sliding carousel; autoplay stops under reduced motion. |
| `Collapse` | Accordion; the header is focusable and toggles with Space or Enter. |
| `Descriptions` | Key/value pairs in one or more columns. |
| `Empty` | Placeholder illustration with a caption. |
| `Image` | Image with a preview overlay. |
| `List` | Vertical rows with title, description and actions. |
| `QRCode` | Byte-mode QR symbol with adjustable colour and error correction. |
| `Statistic` | Large numeric value with prefix and suffix. |
| `Table` | DataGrid-derived grid: sorting, frozen columns, expandable rows and tree rows. |
| `Tag` | Status tag, optionally closable. |
| `Timeline` | Vertical event nodes. |
| `Tooltip` | Short hint on hover or focus, with a shadow. |
| `Tour` | Step-by-step spotlight walkthrough. |

**Feedback**

| Component | Purpose |
|---|---|
| `Alert` | Four semantic types with title, description and close. |
| `Drawer` | Slides in from any edge with direction-aware motion. |
| `Modal` | Mask plus card with focus trap, Esc, exit motion and imperative Modal.Confirm. |
| `Popconfirm` | Inline confirmation bubble with a shadow. |
| `Popover` | Rich content panel on hover or click. |
| `Progress` | Linear and circular progress. |
| `Result` | Full-page outcome screen with five states. |
| `Skeleton` | Loading placeholder with avatar and paragraph shapes. |
| `Spin` | Spinner that can wrap its content. |

## Component reference

### Button `Button`

Five variants (default/primary/dashed/text/link) with danger, icon and loading states.

```xml
<antd:Button Content="OK" Variant="Primary" Click="OnOk" />
<antd:Button Content="Cancel" />
<antd:Button Content="Delete" Danger="True" />
```

| Property | Type | Default |
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

### Icon `Icon`

Drawn as Geometry and tinted by Foreground; 847 glyphs via AntdIcons.ByName.

```xml
<antd:Icon />
```

| Property | Type | Default |
|---|---|---|
| `Data` | Geometry | null |
| `Size` | Double | NaN |
| `Spin` | Boolean | false |

### Input `Input`

Single-line input with a clear button and three sizes.

```xml
<antd:Input Width="240" Placeholder="Type here" AllowClear="True"
           Text="{Binding Name, Mode=TwoWay}" />

<!-- prefix / suffix / clear -->
<antd:Input Prefix="https://" Suffix=".com" AllowClear="True" />
```

| Property | Type | Default |
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

### Text `Text`

Body copy with secondary/success/warning/danger tones.

```xml
<antd:Text />
```

| Property | Type | Default |
|---|---|---|
| `Code` | Boolean | false |
| `Disabled` | Boolean | false |
| `Italic` | Boolean | false |
| `Strong` | Boolean | false |
| `Type` | TextType | TextType |
| `Underline` | Boolean | false |

### Title `Title`

Heading levels 1-5.

```xml
<antd:Title />
```

| Property | Type | Default |
|---|---|---|
| `Level` | Int32 | 1 |

### Paragraph `Paragraph`

Multi-line body copy with ellipsis and copy support.

```xml
<antd:Paragraph />
```

| Property | Type | Default |
|---|---|---|
| `Type` | TextType | TextType |

### Link `Link`

Text link with underline and hover states.

```xml
<antd:Link />
```

| Property | Type | Default |
|---|---|---|
| `Disabled` | Boolean | false |
| `Underline` | Boolean | false |

### Divider `Divider`

Horizontal or vertical rule, optionally with a caption.

```xml
<antd:Divider />
```

| Property | Type | Default |
|---|---|---|
| `Dashed` | Boolean | false |
| `LineBrush` | Brush | theme token |
| `Orientation` | DividerOrientation | DividerOrientation |
| `PlainText` | Boolean | false |
| `Text` | String | null |
| `TextBrush` | Brush | theme token |
| `TextFontWeight` | FontWeight | FontWeight |
| `TextLineHeight` | Double | 22 |

### Space `Space`

Even gaps between children without a trailing edge.

```xml
<antd:Space />
```

| Property | Type | Default |
|---|---|---|
| `Orientation` | Orientation | Orientation |
| `Size` | AntdSize | AntdSize |
| `Spacing` | Double | NaN |

### Flex `Flex`

One-dimensional layout with alignment, wrapping and gaps.

```xml
<antd:Flex />
```

| Property | Type | Default |
|---|---|---|
| `Align` | FlexAlign | FlexAlign |
| `Direction` | FlexDirection | FlexDirection |
| `Gap` | Double | NaN |
| `Justify` | FlexJustify | FlexJustify |
| `Wrap` | Boolean | false |

### Watermark `Watermark`

Tilted text or image watermark over the content.

```xml
<antd:Watermark />
```

| Property | Type | Default |
|---|---|---|
| `Gap` | Double | 80 |
| `Rotate` | Double | -22 |
| `Text` | String | "Ant Design" |
| `TileBrush` | Brush | null |
| `WatermarkFontSize` | Double | NaN |

### Border beam `BorderBeam`

Highlight travelling along a card border.

```xml
<antd:BorderBeam />
```

| Property | Type | Default |
|---|---|---|
| `BaseBrush` | Brush | theme token |
| `BeamBrush` | Brush | null |
| `BeamThickness` | Double | 2 |
| `CornerRadius` | CornerRadius | CornerRadius |
| `Duration` | TimeSpan | TimeSpan |

### Layout `Layout`

Page shell used with Sider / Header / Footer / Content.

```xml
<antd:Layout />
```

| Property | Type | Default |
|---|---|---|
| `Orientation` | Orientation | Orientation |

### Sider `Sider`

Collapsible side bar that keeps an icon rail when collapsed.

```xml
<antd:Sider />
```

| Property | Type | Default |
|---|---|---|
| `Collapsed` | Boolean | false |
| `CollapsedWidth` | Double | 80 |
| `SiderWidth` | Double | 200 |

### Header `Header`

Top bar with token-driven height and padding.

```xml
<antd:Header />
```

### Footer `Footer`

Bottom bar.

```xml
<antd:Footer />
```

### Content `Content`

Main content area with token padding.

```xml
<antd:Content />
```

### Row `Row`

24-column grid row, used with Col.

```xml
<antd:Row />
```

| Property | Type | Default |
|---|---|---|
| `Align` | GridAlign | GridAlign |
| `CurrentBreakpoint` | Breakpoint | Breakpoint |
| `Gutter` | Double | 0 |
| `RowGutter` | Double | 0 |

### Col `Col`

Grid column with span and offset.

```xml
<antd:Col />
```

| Property | Type | Default |
|---|---|---|
| `Lg` | Int32 | -1 |
| `Md` | Int32 | -1 |
| `Offset` | Int32 | 0 |
| `Sm` | Int32 | -1 |
| `Span` | Int32 | 24 |
| `Xl` | Int32 | -1 |
| `Xs` | Int32 | -1 |
| `Xxl` | Int32 | -1 |

### Masonry `Masonry`

Multi-column layout that fills the shortest column.

```xml
<antd:Masonry />
```

| Property | Type | Default |
|---|---|---|
| `Columns` | Int32 | 3 |
| `Gutter` | Double | 16 |

### Splitter `Splitter`

Draggable divider between two panes.

```xml
<antd:Splitter />
```

| Property | Type | Default |
|---|---|---|
| `BarBrush` | Brush | theme token |
| `BarThickness` | Double | 1 |

### Affix `Affix`

Pins content to the top after a scroll threshold.

```xml
<antd:Affix />
```

| Property | Type | Default |
|---|---|---|
| `IsPinned` | Boolean | false |
| `OffsetTop` | Double | 0 |

### Anchor `Anchor`

In-page navigation that highlights the current section.

```xml
<antd:Anchor />
```

| Property | Type | Default |
|---|---|---|
| `ActiveIndex` | Int32 | -1 |
| `ScrollTarget` | ScrollViewer | null |

### Breadcrumb `Breadcrumb`

Hierarchy trail with automatic separators.

```xml
<antd:Breadcrumb />
```

| Property | Type | Default |
|---|---|---|
| `Separator` | String | "/" |

### Dropdown `Dropdown`

Click or hover trigger whose rows reuse MenuNode data.

```xml
<antd:Dropdown />
```

| Property | Type | Default |
|---|---|---|
| `ItemTemplate` | DataTemplate | null |
| `ItemsSource` | IEnumerable | null |

Events: `ItemInvoked`

### Menu `Menu`

Vertical or horizontal menu with nesting and selection; arrow keys work.

```xml
<antd:Menu SelectedIndex="{Binding MenuIndex, Mode=TwoWay}">
  <antd:MenuNode Header="Services">
    <antd:MenuNode Header="Instances" />
  </antd:MenuNode>
  <antd:MenuNode Header="Settings" />
</antd:Menu>
```

| Property | Type | Default |
|---|---|---|
| `Mode` | MenuMode | MenuMode |
| `SelectedItem` | MenuNode | null |

Events: `ItemSelected`

### Pagination `Pagination`

Page numbers with prev/next and an ellipsis for long ranges.

```xml
<antd:Pagination Total="{Binding Total}" PageSize="10"
               CurrentPage="{Binding Page, Mode=TwoWay}" />
```

| Property | Type | Default |
|---|---|---|
| `Current` | Int32 | 1 |
| `PageSize` | Int32 | 10 |
| `Total` | Int32 | 0 |

Events: `PageChanged`

### Steps `Steps`

Process progress with a current step and status colours.

```xml
<antd:Steps Current="{Binding Step}">
  <antd:StepsItem Title="Fill in" />
  <antd:StepsItem Title="Validate" />
  <antd:StepsItem Title="Done" />
</antd:Steps>
```

| Property | Type | Default |
|---|---|---|
| `Current` | Int32 | 0 |
| `Direction` | Orientation | Orientation |

### Tabs `Tabs`

Horizontal tab strip, line or centred style.

```xml
<antd:Tabs SelectedIndex="{Binding TabIndex, Mode=TwoWay}">
  <antd:TabsItem Header="Overview"><TextBlock Text="..." /></antd:TabsItem>
  <antd:TabsItem Header="Logs"><TextBlock Text="..." /></antd:TabsItem>
</antd:Tabs>
```

| Property | Type | Default |
|---|---|---|
| `ItemGutter` | Double | 32 |

### Tree `Tree`

Hierarchical display and selection, keyboard navigable.

```xml
<antd:Tree />
```

| Property | Type | Default |
|---|---|---|
| `Checkable` | Boolean | false |
| `ShowIcon` | Boolean | false |

### AutoComplete `AutoComplete`

Suggests entries while typing; arrow keys pick.

```xml
<antd:AutoComplete />
```

| Property | Type | Default |
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

### Cascader `Cascader`

Cascading picker, one column per level.

```xml
<antd:Cascader />
```

| Property | Type | Default |
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

### Checkbox `Checkbox`

Checkbox with an indeterminate state and disabled support.

```xml
<antd:Checkbox />
```

| Property | Type | Default |
|---|---|---|
| `BoxBorderBrush` | Brush | theme token |
| `BoxBrush` | Brush | theme token |
| `BoxSize` | Double | 16 |
| `CheckBrush` | Brush | theme token |
| `Size` | AntdSize | AntdSize |

### ColorPicker `ColorPicker`

Preset swatches plus a hue strip in a popup.

```xml
<antd:ColorPicker />
```

| Property | Type | Default |
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

### DatePicker `DatePicker`

Single date, or a two-month range with DisabledDate rules.

```xml
<antd:DatePicker Width="220" AllowClear="True"
                SelectedDate="{Binding Date, Mode=TwoWay}" />

<!-- range -->
<antd:DatePicker Width="280" IsRange="True"
                RangeStart="{Binding From, Mode=TwoWay}"
                RangeEnd="{Binding To, Mode=TwoWay}" />

<!-- block dates from code -->
picker.DisabledDate = d => d < DateTime.Today;
```

| Property | Type | Default |
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

### Form `Form`

Label layout, required rules and error text; horizontal mode and label width.

```xml
<antd:Form LabelWidth="96">
  <antd:FormItem Label="Name" Required="True">
    <antd:Input Text="{Binding Name, Mode=TwoWay}" />
  </antd:FormItem>
  <antd:FormItem Label="Code" Required="True" Width="120" />
</antd:Form>

if (!await form.ValidateAsync()) return;   // runs async validators too
```

| Property | Type | Default |
|---|---|---|
| `ItemSpacing` | Double | NaN |
| `LabelWidth` | Double | 96 |
| `Layout` | FormLayout | FormLayout |

### InputNumber `InputNumber`

Stepper input that clamps even when the value is set from code.

```xml
<antd:InputNumber />
```

| Property | Type | Default |
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

### Mentions `Mentions`

Mention suggestions triggered by @.

```xml
<antd:Mentions />
```

| Property | Type | Default |
|---|---|---|
| `CornerRadius` | CornerRadius | CornerRadius |
| `IsDropDownOpen` | Boolean | false |
| `ItemsSource` | IEnumerable | null |
| `Placeholder` | String | "Type @ to mention" |
| `ShowPlaceholder` | Boolean | true |
| `Text` | String | "" |
| `TriggerChar` | Char | Char |

### Radio `Radio`

Mutually exclusive choice inside a group.

```xml
<antd:Radio />
```

| Property | Type | Default |
|---|---|---|
| `DotBrush` | Brush | theme token |
| `RingBrush` | Brush | theme token |
| `RingSize` | Double | 16 |

### Rate `Rate`

Star rating with half stars; arrow keys adjust.

```xml
<antd:Rate />
```

| Property | Type | Default |
|---|---|---|
| `AllowHalf` | Boolean | false |
| `Count` | Int32 | 5 |
| `Gap` | Double | 8 |
| `IsReadOnly` | Boolean | false |
| `StarSize` | Double | 20 |
| `Value` | Double | 0 |

### Select `Select`

Single or multiple selection with tag folding, local/remote search and virtualisation.

```xml
<antd:Select Width="240" Placeholder="Select a state"
             ItemsSource="{Binding States}"
             SelectedItem="{Binding State, Mode=TwoWay}" />

<!-- searchable, multiple, folded tags -->
<antd:Select Width="320" IsMultiple="True" MaxTagCount="3"
             Filterable="True" ItemsSource="{Binding Tags}" />
```

| Property | Type | Default |
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

Events: `SearchTextChanged`

### Slider `Slider`

Continuous or stepped value picker.

```xml
<antd:Slider />
```

| Property | Type | Default |
|---|---|---|
| `Size` | AntdSize | AntdSize |
| `ThumbSize` | Double | 14 |
| `TrackThickness` | Double | 4 |

### Switch `Switch`

Immediate two-state toggle.

```xml
<antd:Switch />
```

| Property | Type | Default |
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

### TimePicker `TimePicker`

Hour / minute / second panel.

```xml
<antd:TimePicker />
```

| Property | Type | Default |
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

### Transfer `Transfer`

Move items between two lists.

```xml
<antd:Transfer />
```

| Property | Type | Default |
|---|---|---|
| `ItemsSource` | IEnumerable | null |
| `SourceCount` | String | "0 items" |
| `SourceTitle` | String | "Source" |
| `TargetCount` | String | "0 items" |
| `TargetTitle` | String | "Target" |

Events: `Changed`

### TreeSelect `TreeSelect`

Tree picker inside a dropdown.

```xml
<antd:TreeSelect />
```

| Property | Type | Default |
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

### Upload `Upload`

Collects files by drop or pick with a queue, progress and cancel; transport comes from the host.

```xml
<antd:Upload Accept=".csv,.xlsx" AutoUpload="True"
            Uploader="UploadFileAsync" />

// the library only runs the queue; transport comes from the host
upload.Uploader = async (file, progress, token) =>
    await api.UploadAsync(file.Path, progress, token);
```

| Property | Type | Default |
|---|---|---|
| `Accept` | String | null |
| `ButtonText` | String | "Upload" |
| `HasFiles` | Boolean | false |
| `Hint` | String | "Click or drag files here" |
| `IsDraggingOver` | Boolean | false |
| `Multiple` | Boolean | true |

Events: `FileSelected`

### Segmented `Segmented`

Compact multi-option switch.

```xml
<antd:Segmented />
```

| Property | Type | Default |
|---|---|---|
| `Block` | Boolean | false |
| `ItemCornerRadius` | CornerRadius | CornerRadius |
| `ItemFontSize` | Double | 14 |
| `TrackCornerRadius` | CornerRadius | CornerRadius |

### Avatar `Avatar`

Image, icon or text avatar, groupable.

```xml
<antd:Avatar />
```

| Property | Type | Default |
|---|---|---|
| `AvatarBackground` | Brush | theme token |
| `AvatarCornerRadius` | CornerRadius | CornerRadius |
| `AvatarFontSize` | Double | 14 |
| `AvatarForeground` | Brush | theme token |
| `HasImage` | Boolean | false |
| `Shape` | AntdAvatarShape | AntdAvatarShape |
| `Size` | Double | 32 |
| `Source` | ImageSource | null |

### Badge `Badge`

Dot or numeric badge, standalone too.

```xml
<antd:Badge />
```

| Property | Type | Default |
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

### Calendar `Calendar`

Month grid with keyboard navigation and DisabledDate.

```xml
<antd:Calendar />
```

| Property | Type | Default |
|---|---|---|
| `CornerRadius` | CornerRadius | CornerRadius |
| `Days` | ObservableCollection`1 | null |
| `DisplayDate` | DateTime | DateTime |
| `MonthLabel` | String | "" |
| `SelectedDate` | Nullable`1 | null |
| `WeekDays` | String[] | null |

Events: `SelectedDateChanged`

### Card `Card`

Content container with title, extra slot and actions.

```xml
<antd:Card />
```

| Property | Type | Default |
|---|---|---|
| `Bordered` | Boolean | true |
| `CornerRadius` | CornerRadius | CornerRadius |
| `Extra` | Object | null |
| `HasHeader` | Boolean | false |
| `Title` | Object | null |

### Carousel `Carousel`

Sliding carousel; autoplay stops under reduced motion.

```xml
<antd:Carousel />
```

| Property | Type | Default |
|---|---|---|
| `Autoplay` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |
| `Interval` | Int32 | 3000 |
| `SelectedIndex` | Int32 | 0 |
| `SelectedItem` | Object | null |
| `ShowDots` | Boolean | true |

### Collapse `Collapse`

Accordion; the header is focusable and toggles with Space or Enter.

```xml
<antd:Collapse>
  <antd:CollapseItem Header="Basic info" IsExpanded="True">
    <TextBlock Text="..." />
  </antd:CollapseItem>
</antd:Collapse>
```

| Property | Type | Default |
|---|---|---|
| `Accordion` | Boolean | false |
| `Bordered` | Boolean | true |
| `CornerRadius` | CornerRadius | CornerRadius |

### Descriptions `Descriptions`

Key/value pairs in one or more columns.

```xml
<antd:Descriptions />
```

| Property | Type | Default |
|---|---|---|
| `Bordered` | Boolean | false |
| `Column` | Int32 | 3 |

### Empty `Empty`

Placeholder illustration with a caption.

```xml
<antd:Empty />
```

| Property | Type | Default |
|---|---|---|
| `Description` | String | null |
| `DescriptionBrush` | Brush | theme token |
| `Image` | Geometry | GeometryGroup |
| `ImageBrush` | Brush | theme token |

### Image `Image`

Image with a preview overlay.

```xml
<antd:Image />
```

| Property | Type | Default |
|---|---|---|
| `CornerRadius` | CornerRadius | CornerRadius |
| `IsPreviewVisible` | Boolean | false |
| `Preview` | Boolean | true |
| `Source` | ImageSource | null |
| `Stretch` | Stretch | Stretch |

### List `List`

Vertical rows with title, description and actions.

```xml
<antd:List />
```

| Property | Type | Default |
|---|---|---|
| `Bordered` | Boolean | false |
| `CornerRadius` | CornerRadius | CornerRadius |

### QRCode `QRCode`

Byte-mode QR symbol with adjustable colour and error correction.

```xml
<antd:QRCode />
```

| Property | Type | Default |
|---|---|---|
| `ErrorCorrection` | QrEcc | QrEcc |
| `ModuleBrush` | Brush | theme token |
| `QuietZone` | Int32 | 4 |
| `Value` | String | "" |

### Statistic `Statistic`

Large numeric value with prefix and suffix.

```xml
<antd:Statistic />
```

| Property | Type | Default |
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

### Table `Table`

DataGrid-derived grid: sorting, frozen columns, expandable rows and tree rows.

```xml
<antd:Table ItemsSource="{Binding Rows}" FrozenColumnCount="2">
  <antd:Table.Columns>
    <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="180" />
    <DataGridTextColumn Header="State" Binding="{Binding State}" Width="120" />
  </antd:Table.Columns>
</antd:Table>

<!-- tree rows: point at the children property -->
<antd:Table ItemsSource="{Binding Nodes}"
            TreeChildrenPath="Children" TreeTextPath="Name" />
```

| Property | Type | Default |
|---|---|---|
| `Bordered` | Boolean | true |
| `EmptyText` | String | null |
| `HasRows` | Boolean | false |
| `ShowRowDetails` | Boolean | false |
| `TreeChildrenPath` | String | null |
| `TreeIndent` | Double | 16 |
| `TreeTextPath` | String | null |

### Tag `Tag`

Status tag, optionally closable.

```xml
<antd:Tag />
```

| Property | Type | Default |
|---|---|---|
| `Bordered` | Boolean | true |
| `Closable` | Boolean | false |
| `Color` | AntdTagColor | AntdTagColor |
| `TagBackground` | Brush | theme token |
| `TagBorderBrush` | Brush | theme token |
| `TagCornerRadius` | CornerRadius | CornerRadius |
| `TagFontSize` | Double | 12 |
| `TagForeground` | Brush | theme token |

Events: `Closed`

### Timeline `Timeline`

Vertical event nodes.

```xml
<antd:Timeline />
```

### Tooltip `Tooltip`

Short hint on hover or focus, with a shadow.

```xml
<antd:Tooltip />
```

| Property | Type | Default |
|---|---|---|
| `Text` | String | "" |

### Tour `Tour`

Step-by-step spotlight walkthrough.

```xml
<antd:Tour />
```

| Property | Type | Default |
|---|---|---|
| `Current` | Int32 | 0 |
| `CurrentDescription` | String | "" |
| `CurrentTitle` | String | "" |
| `IsLastStep` | Boolean | false |
| `IsOpen` | Boolean | false |
| `PositionText` | String | "" |
| `StepCount` | Int32 | 0 |

Events: `Finished`

### Alert `Alert`

Four semantic types with title, description and close.

```xml
<antd:Alert Type="Success" Message="Saved"
            Description="Changes are synced to the server." ShowClose="True" />
```

| Property | Type | Default |
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

Events: `Closed`

### Drawer `Drawer`

Slides in from any edge with direction-aware motion.

```xml
<antd:Drawer />
```

| Property | Type | Default |
|---|---|---|
| `DrawerHeight` | Double | 378 |
| `DrawerWidth` | Double | 378 |
| `Footer` | Object | null |
| `FooterVisibility` | Visibility | Visibility |
| `Placement` | AntdPlacement | AntdPlacement |
| `Title` | Object | null |

### Modal `Modal`

Mask plus card with focus trap, Esc, exit motion and imperative Modal.Confirm.

```xml
<!-- declarative -->
<antd:Modal x:Name="EditModal" Title="Edit" OkText="Save"
            Ok="OnSave" Canceled="OnCancel" />

<!-- imperative: no XAML needed for a confirm -->
Modal.Confirm("Delete", "Remove this record?", () => Delete(), () => { });
Modal.Info("Done", "Saved successfully");
```

| Property | Type | Default |
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

Events: `Canceled`, `Ok`

### Popconfirm `Popconfirm`

Inline confirmation bubble with a shadow.

```xml
<antd:Popconfirm />
```

| Property | Type | Default |
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

Events: `Canceled`, `Confirmed`

### Popover `Popover`

Rich content panel on hover or click.

```xml
<antd:Popover />
```

| Property | Type | Default |
|---|---|---|
| `Body` | Object | null |
| `HasTitle` | Boolean | false |
| `Title` | Object | null |

### Progress `Progress`

Linear and circular progress.

```xml
<antd:Progress />
```

| Property | Type | Default |
|---|---|---|
| `CircleSize` | Double | 120 |
| `Percent` | Double | 0 |
| `ShowInfo` | Boolean | true |
| `Status` | AntdProgressStatus | AntdProgressStatus |
| `StrokeWidth` | Double | NaN |
| `Type` | AntdProgressType | AntdProgressType |

### Result `Result`

Full-page outcome screen with five states.

```xml
<antd:Result />
```

| Property | Type | Default |
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

### Skeleton `Skeleton`

Loading placeholder with avatar and paragraph shapes.

```xml
<antd:Skeleton />
```

| Property | Type | Default |
|---|---|---|
| `Active` | Boolean | false |
| `Avatar` | Boolean | false |
| `Rows` | Int32 | 3 |
| `Title` | Boolean | true |

### Spin `Spin`

Spinner that can wrap its content.

```xml
<antd:Spin />
```

| Property | Type | Default |
|---|---|---|
| `IndicatorBrush` | Brush | theme token |
| `IndicatorSize` | Double | 20 |
| `ShowsTip` | Boolean | false |
| `Size` | AntdSize | AntdSize |
| `Spinning` | Boolean | true |
| `Tip` | String | null |
| `TipBrush` | Brush | theme token |

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Text clipped inside an input | `TextBox.Padding` is copied into the content host | Fixed in 1.0; custom inputs should copy the `HostHeight` approach |
| Theme switch does nothing | Style uses `StaticResource` | Switch to `{DynamicResource Antd.*}` |
| Dropdown has no shadow | Only Select and the OverlayBase family got the shadow gutter | Known gap for DatePicker / TimePicker / Cascader |
| No visible focus ring | A custom template dropped `FocusVisualStyle` | Reference `Antd.FocusVisual` |

See also: [migration](migration.md) · [capability notes](progress.md) · [component site](site/index.html)

