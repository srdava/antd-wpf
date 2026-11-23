# Antd.Wpf 1.1.1

修复一个从 1.0.0 起就存在的严重缺陷：**Select 无法用鼠标展开**。

## 缺陷

`Select` 沿用 `ComboBox` 的引擎，但自定义模板里缺少 WPF 默认模板才有的 `dropDownToggle` 开关。WPF 的下拉展开动作绑在那个 `ToggleButton` 上，模板里没有它，点击整个字段就没有任何反应——键盘（Alt+↓ / F4）和代码设置 `IsDropDownOpen` 都正常，所以此前的自动化断言一直是绿的。

## 修复

在字段层补一个透明 `ToggleButton`（`PART_Toggle`），双向绑定 `IsDropDownOpen`：

```xml
<ToggleButton x:Name="PART_Toggle" Grid.ColumnSpan="3"
              Focusable="False" ClickMode="Press"
              IsChecked="{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}" />
```

同时把 `SelectionHost` / `PlaceholderHost` 设为 `IsHitTestVisible="False"`，让点击穿透到开关；清除按钮与多选标签仍在开关之上，行为不变。

## 新增断言（防止再退化）

- 模板中存在 `PART_Toggle`
- 触发开关后 `IsDropDownOpen` 为真
- 展开后生成 2 个 item 容器
- 选中后字段显示对应文本
- 清除按钮仍能清空选中

## 升级

```bash
dotnet add package Antd.Wpf.Controls --version 1.1.1
```

纯修复，无 API 变更，可直接替换 1.1.0。
