# Antd.Wpf

Ant Design 规范的 WPF 实现，目标同时运行于 **.NET Framework 4.8+** 与 **.NET 8+**。

当前状态：M1–M4 的**基础版本**全部落地（69/71 组件）。**能力完成度以 `docs/progress.md` 为准**——
组件数量不等于能力齐全（Select 无多选、DatePicker 无区间、Table 无固定列等），接入前请先看那份能力表。
M5 打磨已完成：无障碍 Peer 与焦点环、`AntdLocale` 中英文案、入退场动效与减动效开关、弹窗/抽屉/Toast/浮层的阴影分层。
**1.0.0 已于 2026-11-20 发布到 nuget.org**：[Controls](https://www.nuget.org/packages/Antd.Wpf.Controls) · [Theme](https://www.nuget.org/packages/Antd.Wpf.Theme) · [Icons](https://www.nuget.org/packages/Antd.Wpf.Icons) · [Tokens](https://www.nuget.org/packages/Antd.Wpf.Tokens)（MIT）。发行版附件（8 个包 + 校验和）见 [Gitee Releases](https://gitee.com/davva/antd.-wpf.-tokens/releases/tag/v1.0.0)；组件文档站见 `docs/site/index.html`，迁移指南见 `docs/migration.md`。
每个组件的能力边界都写在 `docs/progress.md` 里，避免把「能跑」当成「对齐 antd」。
项目简介（定位、特性、不适合谁）见 [`docs/intro.md`](docs/intro.md)。
完整路线图见 [`docs/antd-wpf-implementation-plan.md`](docs/antd-wpf-implementation-plan.md)，
M0 落地记录见 [`docs/m0-implementation-notes.md`](docs/m0-implementation-notes.md)。

## 快速开始

```bash
dotnet add package Antd.Wpf.Controls --version 1.0.0
```

```bash
dotnet build Antd.Wpf.sln          # 双目标全量构建
dotnet run --project demo/Antd.Wpf.Gallery -f net8.0-windows      # 组件图册
dotnet run --project demo/Antd.Wpf.Integration -f net8.0-windows  # 真实页面接入示例
dotnet pack src/Antd.Wpf.Controls -c Release -o artifacts/packages   # 打 1.0.0 包
dotnet run --project tools/docs-gen                              # 生成 docs/site/index.html
```

## 验证

```bash
# 与 antd 官方 theme.getDesignToken() 逐 token 对拍（536 × 10 组配置）
dotnet run --project tests/Antd.Wpf.TokenParity -f net8.0-windows
dotnet run --project tests/Antd.Wpf.TokenParity -f net48

# 控件冒烟 + 主题切换 + 架构边界，附带渲染截图
dotnet run --project tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --shot artifacts
dotnet run --project tests/Antd.Wpf.SmokeTests -f net48

# M4 反馈层（Alert / Result / Modal / Drawer / Popconfirm / Message / Notification）
dotnet run --project tests/Antd.Wpf.FeedbackTests -f net8.0-windows -- --shot artifacts
dotnet run --project tests/Antd.Wpf.FeedbackTests -f net48

# 真实页面接入自检（28 项：筛选、分页、校验、编辑、删除、主题切换）
dotnet run --project demo/Antd.Wpf.Integration -f net8.0-windows -- --selftest
dotnet run --project demo/Antd.Wpf.Integration -f net8.0-windows -- --shot artifacts/integration-light.png
```

进度与能力边界见 `docs/progress.md`。

QR 编码器是手写的（无第三方依赖），用真实解码器校验：

```bash
dotnet run --project tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --qr-dump artifacts/qr
python tools/qr-ref/verify.py artifacts/qr     # 需要 pip install zxing-cpp pillow
```

视觉对齐（token 相等不代表长得像，这套验证尺寸与配色）：

```bash
cd tools/visual-ref
node fetch-vendor.js          # 取 antd 6.6.8 UMD + React 18 UMD
node serve.js &               # 本机 Chrome 不能用 file:// 加载外部脚本
# 用 Chrome 无头渲染并把官方尺寸 dump 成 out/metrics.json（见 docs/visual-parity.md）
dotnet run --project ../../tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --metrics out/ours.json
python compare.py out/metrics.json out/ours.json
```

重新生成 golden token 数据（需要 Node）：

```bash
cd tools/token-sync && npm install && node dump-tokens.js
```

## 结构

```
src/Antd.Wpf.Tokens     netstandard2.0   纯计算：Seed → Map → Alias，无 WPF 依赖
src/Antd.Wpf.Theme      net48;net8        ThemeManager / ThemeSnapshot / BrushSet / XAML 扩展
src/Antd.Wpf.Icons      net48;net8        446 个 outlined 图标的懒加载 Geometry（由 tools/icon-gen 生成）
src/Antd.Wpf.Controls   net48;net8        97 个控件类型 + Generic.xaml
demo/Antd.Wpf.Gallery   net48;net8        组件图册（逐个控件看外观）
demo/Antd.Wpf.Integration net48;net8   真实页面接入示例：表格 + 表单校验 + 弹窗 + 主题切换，自带 --selftest
tests/                  三个可执行断言程序（不依赖任何测试框架）
tools/token-sync        Node 脚本，从 antd 导出 golden token
tools/icon-gen          Node 脚本，从 @ant-design/icons-svg 生成 Geometry
tools/docs-gen          net8 控制台，反射程序集生成单文件文档站
docs/site/index.html    生成的组件文档（单文件，无外部依赖）
tools/visual-ref        真实 antd 参照页 + 尺寸对比脚本（视觉对齐用）
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

## 打包与发布

```bash
# 1. 打四个包（含 symbol 包）
dotnet pack src/Antd.Wpf.Controls -c Release -o artifacts/packages
dotnet pack src/Antd.Wpf.Theme    -c Release -o artifacts/packages
dotnet pack src/Antd.Wpf.Icons    -c Release -o artifacts/packages
dotnet pack src/Antd.Wpf.Tokens   -c Release -o artifacts/packages

# 2. 验证包能被真实项目安装并跑通（pack URI、主题、图标）
dotnet run --project tools/package-check

# 3. 发布（需要 nuget.org API Key，见下）
dotnet nuget push "artifacts/packages/*.nupkg" -k $env:NUGET_API_KEY -s https://api.nuget.org/v3/index.json
```

`tools/package-check` 是一个消费者工程：从 `artifacts/packages` 还原 `Antd.Wpf.Controls 1.0.0`，
加载 `Themes/Framework.xaml`，实例化 Button/Modal，取图标集。包缺文件、依赖漏写都会在这里失败。

### 首次发布：拿到 API Key

1. 用 Microsoft 账号（Outlook / Hotmail / 企业 Entra ID）登录 <https://www.nuget.org>，
   首次登录要注册一个用户名——它不可修改，会显示在包页面上。
2. 在 Account settings 里开启 **两步验证**；没开 2FA 无法创建 API Key。
3. 右上角头像 → **API Keys** → **Create**：
   - Name：`antd-wpf-release`
   - Expiration：90 天（上限 365）
   - Scopes：**Push new packages and package versions**（首次发布必须有这个，否则新 ID 推不上去）
   - Glob pattern：`Antd.Wpf.*`（限制这把钥匙只能推这四个包）
4. 创建后**立刻复制**——页面只显示一次，丢了只能重新生成。
5. 本地用环境变量保存，不要写进仓库：

```bash
setx NUGET_API_KEY "oy2xxxxxxxxxxxxxxxx"     # Windows，开新终端生效
```

推送后 nuget.org 有 10–60 秒的校验与索引时间，期间 `dotnet add package` 可能 404，属正常。

## 许可证

[MIT](LICENSE)。包内以 `<license type="expression">MIT</license>` 声明。
