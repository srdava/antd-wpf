# Antd.Wpf

Ant Design 规范的 WPF 实现，目标同时运行于 **.NET Framework 4.8+** 与 **.NET 8+**。

当前状态：**M2 进行中（36 个控件）**。M0/M1 已完成；M2 已交付表单控件
（InputNumber/Checkbox/Radio/Switch/Rate/Slider/Select/Form）。
日期选择、级联、上传等 8 个复杂组件待做，见 `docs/m2-implementation-notes.md`。
完整路线图见 [`docs/antd-wpf-implementation-plan.md`](docs/antd-wpf-implementation-plan.md)，
M0 落地记录见 [`docs/m0-implementation-notes.md`](docs/m0-implementation-notes.md)。

## 快速开始

```bash
dotnet build Antd.Wpf.sln          # 双目标全量构建
dotnet run --project demo/Antd.Wpf.Gallery -f net8.0-windows
dotnet run --project demo/Antd.Wpf.Gallery -f net48
```

## 验证

```bash
# 与 antd 官方 theme.getDesignToken() 逐 token 对拍（536 × 10 组配置）
dotnet run --project tests/Antd.Wpf.TokenParity -f net8.0-windows
dotnet run --project tests/Antd.Wpf.TokenParity -f net48

# 控件冒烟 + 主题切换 + 架构边界，附带渲染截图
dotnet run --project tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --shot artifacts
dotnet run --project tests/Antd.Wpf.SmokeTests -f net48
```

重新生成 golden token 数据（需要 Node）：

```bash
cd tools/token-sync && npm install && node dump-tokens.js
```

## 结构

```
src/Antd.Wpf.Tokens     netstandard2.0   纯计算：Seed → Map → Alias，无 WPF 依赖
src/Antd.Wpf.Theme      net48;net8        ThemeManager / ThemeSnapshot / BrushSet / XAML 扩展
src/Antd.Wpf.Icons      net48;net8        57 个 antd 图标的冻结 Geometry（由 tools/icon-gen 生成）
src/Antd.Wpf.Controls   net48;net8        36 个控件 + Generic.xaml
demo/Antd.Wpf.Gallery   net48;net8        演示程序
tests/                  两个可执行断言程序（不依赖任何测试框架）
tools/token-sync        Node 脚本，从 antd 导出 golden token
tools/icon-gen          Node 脚本，从 @ant-design/icons-svg 生成 Geometry
```

## 用法

```xml
<Window xmlns:antd="clr-namespace:Antd.Wpf.Controls;assembly=Antd.Wpf.Controls"
        Background="{DynamicResource Antd.colorBgLayout}"
        FontFamily="{DynamicResource Antd.FontFamily}">
    <StackPanel>
        <antd:Button Content="Primary" Variant="Primary" />
        <antd:Input Width="220" Placeholder="Search" AllowClear="True" />
    </StackPanel>
</Window>
```

```csharp
// App.xaml.cs：把 token 字典挂到应用资源，之后切换主题全自动
ThemeManager.Current.Attach(Resources);
ThemeManager.Current.ApplyDark();                       // 或 ApplyLight() / ToggleDark()
ThemeManager.Current.UpdateSeed(s => s.ColorPrimary = AntdColor.Parse("#00b96b"));
```

## 设计要点

- 主题分发采用「不可变快照 + 单点通知」：`ThemeManager.Apply()` 一次性替换 `ThemeSnapshot`，
  所有画刷从冻结缓存取用，避免 `DynamicResource` 全树失效与未冻结画刷导致的掉帧。
- XAML 用 `{DynamicResource Antd.<tokenName>}`（因为 `Setter.Value` 不允许 Binding），
  代码用 `ThemeManager.Current.Snapshot`，两条路径同源。
- 所有 token 数值与 antd 官方实现逐值对拍，含 JS 与 C# 舍入差异、HSB/HSL 差异等坑。
