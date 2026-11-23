# 迁移指南

面向三类读者：从 antd（React）迁过来的团队、从社区 `AntDesign.WPF` 迁过来的项目、以及第一次接入这个库的人。

---

## 0. 先确认它适合你

| 你的情况 | 结论 |
|---|---|
| WPF 项目，需要 antd 的视觉与交互，且要跑 net48 | 适合 |
| 需要 antd 的**全部**能力（Table 展开行、Form 异步校验、Select 远程搜索） | 暂不适合，见 `docs/progress.md` 能力边界 |
| 需要 Vue/React 那套受控组件心智 | 需要调整：这里是 WPF 双向绑定 |
| 需要第三方依赖为零 | 适合，本库不引用任何 NuGet 包 |

---

## 1. 从 antd（React）迁过来

### 1.1 组件对照

| antd | Antd.Wpf | 说明 |
|---|---|---|
| `<Button type="primary" />` | `<antd:Button Variant="Primary" />` | `type` → `Variant`（Default/Primary/Dashed/Text/Link） |
| `<Input placeholder />` | `<antd:Input Placeholder />` | 属性首字母大写 |
| `<InputNumber min max />` | `<antd:InputNumber Minimum Maximum />` | 外部赋值也会收敛到区间 |
| `<Select options />` | `<antd:Select ItemsSource />` | WPF 的 ItemsSource，不是 options 数组 |
| `<Form layout="horizontal" labelCol>` | `<antd:Form Layout="Horizontal" LabelWidth="120" />` | 标签列宽是固定像素 |
| `<Table columns />` | `<antd:Table.Columns>` + `DataGridColumn` | 底层是 DataGrid，排序/列宽/虚拟化保留 |
| `<Modal open />` | `<antd:Modal IsOpen />` | 关闭有 200ms 退场动画，`IsOpen` 立即为 false |
| `message.success()` | `Message.Success("...")` | 需要 `Message.Owner = window` |
| `notification.open()` | `Notification.Info(title, body)` | 同上 |

### 1.2 心智差异

| antd | WPF |
|---|---|
| 受控组件（value + onChange） | 依赖属性，默认双向绑定；`{Binding X, Mode=TwoWay}` 可省略 |
| 样式通过 `style`/`className` | 通过依赖属性 + token；不支持 CSS 类名 |
| `ConfigProvider theme={{token}}` | `ThemeManager.Current.Apply(ThemeConfig.Create(seed => ...))` |
| `ConfigProvider locale` | `AntdLocale.UseCulture("zh-CN")` |
| 组件内部动画由 CSS 控制 | `AntdMotion.Enabled`，默认跟随系统"显示动画" |

### 1.3 主题：token 是同一个源头

```csharp
// 只换主色
ThemeManager.Current.UpdateSeed(seed => seed.ColorPrimary = AntdColor.Parse("#00b96b"));

// 深色 + 紧凑
ThemeManager.Current.Apply(ThemeConfig.Create(null, ThemeAlgorithm.Dark | ThemeAlgorithm.Compact));

// 只切明暗，保留紧凑（1.0 修复：旧实现会把 Compact 抹掉）
ThemeManager.Current.ApplyDark();
```

XAML 里统一用 `{DynamicResource Antd.colorPrimary}` 这类 token 名，**不要**写死颜色。

---

## 2. 从社区 `AntDesign.WPF` 迁过来

| 项 | AntDesign.WPF | Antd.Wpf |
|---|---|---|
| 命名空间 | `AntDesign` / `AntDesign.Controls` | `Antd.Wpf.Controls`（程序集 `Antd.Wpf.Controls`） |
| 目标框架 | 仅 net8 | net48 + net8-windows |
| 主题切换 | `Application.Current.SwitchTheme()` | `ThemeManager.Current.Apply/ApplyDark/ToggleDark` |
| 图标 | `{antd:AntdIcon Kind=...}` 几百个枚举 | `AntdIcons.ByName("rocket")` 或 `{x:Static icons:AntdIcons.Rocket}` |
| 依赖 | 引用了若干第三方包 | 零依赖 |

两者可以并存于同一解决方案（命名空间不同），但**不要**同时合并两套 `Generic.xaml`。

---

## 3. 从 0.x 到 1.0

1.0 是第一个公开发布版本，没有对外承诺过 0.x API。1.0 冻结的约定：

- 命名空间 `Antd.Wpf.*` 不再改名；
- 依赖属性名与 antd 的属性名对应（首字母大写）；
- token 名与 antd 的 CSS 变量名对应（`colorPrimary`、`paddingLG`…）；
- `ThemeSnapshot` 一旦发布即不可变。

---

## 4. 常见坑（都是踩过的）

| 现象 | 原因 | 处理 |
|---|---|---|
| 输入框文字上下被裁 | WPF 把 `TextBox.Padding` 复制到内容宿主，边框又吃一次 | 1.0 已修；自定义输入类控件请照抄 `HostHeight` 的做法 |
| 弹窗里控件高度不对 | 弹层在独立 Popup，父级 `FontSize` 不继承 | 在弹层根上显式设置字号或主题资源 |
| 切主题后某些颜色没变 | 用了 `StaticResource` | 换成 `DynamicResource Antd.*` |
| 下拉面板没有阴影 | Select/DatePicker 自带 Popup，尚未加阴影槽 | 已知差异，见 `docs/visual-parity.md` |
| 数据绑定不生效 | 直接 `Value = x` 覆盖了绑定 | 用 `SetCurrentValue` 语义的控件属性，或在 VM 上改 |
| 键盘 Tab 看不到焦点 | 自定义模板关掉了 `FocusVisualStyle` | 1.0 已统一补焦点环；自写模板请引用 `Antd.FocusVisual` |
| 表格行点不动按钮 | `Table.IsReadOnly = true` | 模板列里的按钮仍可点；需要编辑请显式设 `IsReadOnly=false` |

---

## 5. 接入清单

- [ ] 引 `Antd.Wpf.Controls`（会带上 Theme / Tokens / Icons）
- [ ] `App.xaml` 合并 `Themes/Framework.xaml`
- [ ] 启动时 `ThemeManager.Current.Attach(Application.Current.Resources)` + `Apply(...)`
- [ ] 需要中文：`AntdLocale.UseCulture("zh-CN")`
- [ ] 需要减动效：`AntdMotion.UseSystemDefault()`（默认行为）
- [ ] 检查 `docs/progress.md` 的能力边界是否覆盖你的场景
- [ ] 跑一遍 `tests/Antd.Wpf.SmokeTests`（或对照自己的 UI 走查）
