# 项目简介

**Antd.Wpf 是 Ant Design 在 WPF 上的实现：同一套设计 token、同一套组件语义，同时跑在 .NET Framework 4.8 与 .NET 8 上，零第三方依赖。**

---

## 它解决什么问题

WPF 项目想用 antd 的样子，通常只有三条路：照着图抄样式（改一次主题全崩）、用 WebView 嵌页面（交互和性能都别扭）、或者找一个只支持 .NET 8 的社区移植（4.8 项目直接没戏）。

这个库选的是第四条：**把 antd 的 token 计算原样搬到 C#，让控件从同一份 token 里取色和取尺寸**。所以换主题不是改样式表，是换一份快照；视觉一致性不是靠人眼对，是靠逐值比对。

## 核心特性

| 特性 | 具体做法 |
|---|---|
| **双框架** | 同一套源码产出 `net48` 与 `net8.0-windows` 两个目标，4.8 项目不需要改运行时 |
| **零依赖** | 不引用任何 NuGet 包；QR 编码器、图标几何、主题引擎都是自己实现 |
| **Token 对拍** | 536 个 token × 10 组主题配置，与 antd 官方 `theme.getDesignToken()` **逐值相等**，处理了 JS 与 C# 的舍入差异、HSV/HSL 差异 |
| **真·主题切换** | 不可变 `ThemeSnapshot` + 单点通知：亮/暗/紧凑任意组合，运行中切换无需重启，画刷冻结缓存不掉帧 |
| **无障碍** | 92 个可构造控件 100% 具备 AutomationPeer；统一焦点环；按钮、折叠头、菜单、评分、上传区都能纯键盘操作 |
| **本地化与动效** | `AntdLocale` 中英切换；`AntdMotion` 默认跟随系统"显示动画"，关闭后弹窗/下拉/轮播全部静止 |
| **图标** | 847 个（outlined 446 / filled 251 / twotone 150），懒加载 + 缓存，不拖慢启动 |

## 它不做什么（重要）

组件数量不等于能力齐全。这是**基础版本**，以下是明确没有做的：

- Select 支持本地搜索（`Filterable`）与多选（`IsMultiple` + `MaxTagCount` 折叠）；DatePicker 支持区间但没有禁用日期与快捷选项
- Table 基于 DataGrid 适配，固定列与展开行可用，但没有树形行与可编辑
- Form 只有同步校验；Upload 只收集文件，不做传输队列
- Tree 只吃静态数据；twotone 图标合并为单色

完整清单在 [`docs/progress.md`](progress.md)，接入前请先看它——**知道边界比看截图有用**。

## 怎么验证它值得信

数字都是可复跑的，不是宣传语：

| 验证项 | 数据 |
|---|---|
| Token 对齐 | 536 个 token × 10 组配置，与 antd 官方实现逐值比对 |
| 控件断言 | 291 项（含渲染截图） |
| 反馈层断言 | 76 项 |
| 真实页面自检 | 28 项（筛选、分页、校验、编辑、删除、主题切换） |
| 总断言 | **395 项**，net48 与 net8 双跑全过 |
| 包可用性 | 从 nuget.org 全新还原后跑通（pack URI、主题、图标） |

## 三十秒接入

```bash
dotnet add package Antd.Wpf.Controls --version 1.0.0
```

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

```csharp
ThemeManager.Current.Attach(Application.Current.Resources);
ThemeManager.Current.Apply(ThemeConfig.CreateDefault());
```

## 适合谁

- WPF 项目需要 antd 的观感，且**必须兼容 .NET Framework 4.8**
- 想把设计 token 作为唯一真相源，而不是散落的样式
- 需要无障碍与键盘可达（政企、医疗、工业软件常有的硬要求）
- 不想要第三方依赖的供应链风险

不适合：需要树形表格、跨字段联动校验、Web 组件生态的项目；非 antd 设计语言的项目。

## 结构

```
src/Antd.Wpf.Tokens     纯计算：Seed → Map → Alias，无 WPF 依赖
src/Antd.Wpf.Theme      主题引擎：快照、画刷缓存、运行时切换
src/Antd.Wpf.Icons      847 个图标的懒加载几何
src/Antd.Wpf.Controls   69 个组件 + Generic.xaml
demo/Gallery            组件图册
demo/Integration        真实页面接入示例（28 项自检）
tests/                  三个零依赖的断言程序
tools/                  token 导出、图标生成、文档生成、包可用性检查
```

## 状态与许可

1.0.0 已发布：nuget.org（`Antd.Wpf.Controls` 等四包）+ Gitee 发行版（含校验和）。MIT 许可。

文档：[组件文档站](site/index.html) · [迁移指南](migration.md) · [能力边界](progress.md) · [视觉对齐记录](visual-parity.md)
