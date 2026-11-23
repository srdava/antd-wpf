# Antd.Wpf

> Ant Design 规范的 WPF 实现 · 同时支持 **.NET Framework 4.8** 与 **.NET 8** · 零第三方依赖
>
> 中文 | [English](README.en.md)
>
> **GitHub（主仓库）** · https://github.com/srdava/antd-wpf ｜ **Gitee（镜像）** · https://gitee.com/davva/antd.-wpf.-tokens
>
> Issue 与 PR 走 GitHub；Gitee 是同步镜像，仅供国内访问（1.0.0 的发行版附件目前也在 Gitee Releases）。

[![CI](https://github.com/srdava/antd-wpf/actions/workflows/ci.yml/badge.svg)](https://github.com/srdava/antd-wpf/actions/workflows/ci.yml)

[使用手册](docs/manual.zh.md) · [组件文档站](docs/site/index.html) · [迁移指南](docs/migration.md) · [能力边界](docs/progress.md) · [项目简介](docs/intro.md)

---

## 它是什么

把 antd 的**设计 token 计算原样搬到 C#**，让 WPF 控件从同一份 token 取色取值。换主题不是改样式表，而是替换一份不可变快照（`ThemeSnapshot`），画刷全部来自冻结缓存。

| 项 | 取值 |
|---|---|
| 支持框架 | `net48`、`net8.0-windows` |
| 组件 | 计划 71 项中完成 **69 项**（基础版本，能力边界见下） |
| 设计 token | **536 个**，与 antd `theme.getDesignToken()` 逐值对拍 × 10 组配置 |
| 图标 | **847 个**（outlined / filled / twotone），懒加载 + 缓存 |
| 断言 | 组件 347 + 反馈层 76 + 真实页面 28，net48 与 net8 双跑 |
| 无障碍 | 92 个可构造控件 100% 有 AutomationPeer，统一焦点环，键盘可达 |

## 安装

```bash
dotnet add package Antd.Wpf.Controls --version 1.1.0
```

`Antd.Wpf.Controls` 会自动带上 `Antd.Wpf.Theme` / `Antd.Wpf.Icons` / `Antd.Wpf.Tokens`。

```xml
<!-- App.xaml -->
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

```csharp
// App.xaml.cs
ThemeManager.Current.Attach(Application.Current.Resources);
ThemeManager.Current.Apply(ThemeConfig.CreateDefault());

AntdLocale.UseCulture("zh-CN");   // 可选：中文文案
AntdMotion.UseSystemDefault();    // 可选：跟随系统减动效
```

```xml
<Window xmlns:antd="clr-namespace:Antd.Wpf.Controls;assembly=Antd.Wpf.Controls">
    <StackPanel>
        <antd:Button Content="主要按钮" Variant="Primary" />
        <antd:Input Width="240" Placeholder="搜索" AllowClear="True" />
        <antd:Select Width="240" Filterable="True" IsMultiple="True" MaxTagCount="3" />
    </StackPanel>
</Window>
```

## 手册与文档

| 文档 | 内容 |
|---|---|
| **[使用手册（中文）](docs/manual.zh.md)** | 框架用法 + **74 个组件逐个的用法与全部参数表**（参数表由程序集反射生成，不会过期） |
| **[Handbook (English)](docs/manual.en.md)** | 英文手册，与中文版一一对应 |
| [组件文档站](docs/site/index.html) | 单文件 HTML，74 个组件的属性表与能力边界 |
| [迁移指南](docs/migration.md) | 从 antd / AntDesign.WPF / 0.x 迁过来，含 7 个常见坑 |
| [能力边界](docs/progress.md) | 每个组件"做了什么、还缺什么"——**接入前必看** |
| [项目简介](docs/intro.md) | 定位、特性、不适合谁 |
| [视觉对齐记录](docs/visual-parity.md) | 与 antd 官方渲染的尺寸/配色实测对比 |

## 主题

```csharp
ThemeManager.Current.ApplyLight();
ThemeManager.Current.ApplyDark();
ThemeManager.Current.UpdateSeed(s => s.ColorPrimary = AntdColor.Parse("#00b96b"));
ThemeManager.Current.Apply(ThemeConfig.Create(null, ThemeAlgorithm.Dark | ThemeAlgorithm.Compact));
```

XAML 里统一用 `{DynamicResource Antd.colorPrimary}` 这类 token 名；`StaticResource` 不会跟随切换。

## 已实现的关键能力

| 能力 | 说明 |
|---|---|
| **Select** | 单选 / 多选（`+N` 标签折叠）、本地搜索、远程搜索（`FilterLocally=false`）、虚拟化 |
| **DatePicker** | 单日期、双月区间、禁用日期（`DisabledDate`） |
| **Table** | 固定列（`FrozenColumnCount`）、展开行（`ShowRowDetails`）、树形行（`TreeChildrenPath`） |
| **Form** | 同步 `Validator` + 异步 `AsyncValidator` / `ValidateAsync()` |
| **Modal** | 声明式 + 命令式 `Modal.Confirm / Info / Warning / Error` |
| **Upload** | 队列 + 并发上限 + 进度 + 取消（传输函数由宿主提供） |
| **无障碍** | 焦点环；Collapse 头部、Menu 节点、Rate、Upload 区均可键盘操作 |
| **本地化** | `AntdLocale` 中英切换；日期格式跟随 `CultureInfo` |
| **减动效** | `AntdMotion` 默认跟随系统，关闭后弹窗 / 下拉 / 轮播全部静止 |

**尚未实现**：Table 可编辑单元格、Form 跨字段联动、Select 分组、DatePicker 周/月视图、RTL、高对比度、twotone 双色渲染。完整清单见 [能力边界](docs/progress.md)。

## 开发

```bash
dotnet build Antd.Wpf.sln                                  # 双目标全量构建

# 断言（不依赖任何测试框架）
dotnet run --project tests/Antd.Wpf.TokenParity -f net8.0
dotnet run --project tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --shot artifacts
dotnet run --project tests/Antd.Wpf.FeedbackTests -f net8.0-windows
dotnet run --project demo/Antd.Wpf.Integration -f net8.0-windows -- --selftest

# 生成文档站与中英文手册
dotnet run --project tools/docs-gen

# 打包并验证包能被真实项目安装
dotnet pack src/Antd.Wpf.Controls -c Release -o artifacts/packages
dotnet run --project tools/package-check
```

## 项目结构

```
src/Antd.Wpf.Tokens     netstandard2.0   纯计算：Seed → Map → Alias，无 WPF 依赖
src/Antd.Wpf.Theme      net48;net8        ThemeManager / ThemeSnapshot / 画刷缓存 / XAML 扩展
src/Antd.Wpf.Icons      net48;net8        847 个图标的懒加载 Geometry
src/Antd.Wpf.Controls   net48;net8        69 个组件 + Generic.xaml
demo/Gallery                              组件图册（逐个控件看外观）
demo/Integration                          真实页面接入示例（28 项自检）
tests/                                    三个零依赖断言程序
tools/token-sync                          Node：从 antd 导出 golden token 对拍
tools/icon-gen                            Node：从 @ant-design/icons-svg 生成 Geometry
tools/docs-gen                            net8：反射程序集生成文档站与中英文手册
tools/package-check                       消费者工程：验证发布的包能装能用
```

## 发布

```bash
dotnet pack src/Antd.Wpf.Controls -c Release -o artifacts/packages   # 其余三个包同理
dotnet nuget push "artifacts/packages/*.nupkg" -k $env:NUGET_API_KEY -s https://api.nuget.org/v3/index.json
```

1.0.0 已发布：[Controls](https://www.nuget.org/packages/Antd.Wpf.Controls) · [Theme](https://www.nuget.org/packages/Antd.Wpf.Theme) · [Icons](https://www.nuget.org/packages/Antd.Wpf.Icons) · [Tokens](https://www.nuget.org/packages/Antd.Wpf.Tokens)；
发行版附件（含 SHA-256）见 [Gitee Releases](https://gitee.com/davva/antd.-wpf.-tokens/releases/tag/v1.0.0)。

## 许可证

[MIT](LICENSE)。包内以 `<license type="expression">MIT</license>` 声明。
