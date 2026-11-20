# Antd.Wpf 1.0.0

Ant Design 规范的 WPF 实现，同时支持 **net48** 与 **net8.0-windows**。零第三方依赖。

## 安装

```bash
dotnet add package Antd.Wpf.Controls --version 1.0.0
```

`Antd.Wpf.Controls` 会自动带上 `Antd.Wpf.Theme` / `Antd.Wpf.Icons` / `Antd.Wpf.Tokens`。

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

AntdLocale.UseCulture("zh-CN");   // 可选：中文文案
AntdMotion.UseSystemDefault();    // 可选：跟随系统减动效
```

## 本版包含

- **69 / 71 个组件**：通用、布局、导航、数据录入、数据展示、反馈六层；能力边界逐条写在 `docs/progress.md`。
- **Token 逐值对拍**：536 个 token × 10 组主题配置，与 antd `theme.getDesignToken()` 全等（含 JS 与 C# 舍入差异处理）。
- **847 个图标**：outlined 446 / filled 251 / twotone 150，懒加载 + 缓存。
- **无障碍**：92 个可构造控件 100% 具备 AutomationPeer；按钮、折叠头、菜单、评分、上传区均可键盘操作，统一的焦点环。
- **本地化**：`AntdLocale` 中英切换（必填提示、空状态、确定/取消、关闭朗读名）。
- **动效降级**：`AntdMotion` 默认跟随系统「显示动画」，关闭后弹窗、下拉、轮播不再动。
- **阴影分层**：弹窗 / 抽屉 / Toast / 气泡浮层。
- **文档**：组件文档站（单文件，`docs/site/index.html`）、迁移指南（`docs/migration.md`）。

## 验证

| 套件 | 数量 | 结果 |
|---|---|---|
| TokenParity | 536 × 10 组 | PASS |
| SmokeTests | 291 | PASS（net48 + net8） |
| FeedbackTests | 76 | PASS（net48 + net8） |
| Integration 自检 | 28 | PASS（net48 + net8） |

## 已知边界

Select 无搜索/多选、DatePicker 无区间、Table 无固定列、Form 只有同步校验、Upload 只收集文件不做传输、Tree 静态数据、twotone 图标合并为单色。详见 `docs/progress.md`。

## 附件校验和（SHA-256）

| 文件 | 大小 | SHA-256 |
|---|---|---|
| Antd.Wpf.Controls.1.0.0.nupkg | 234 KB | `e618b259ad7c1075441e953cf88ff1e2defe3d1838fc7caec2ca8163cddcf846` |
| Antd.Wpf.Icons.1.0.0.nupkg | 482 KB | `d51a36df2032525cf433aa3721413366be92cd38e3c240bd2c4c12c59cf52359` |
| Antd.Wpf.Theme.1.0.0.nupkg | 21 KB | `6cc59c8d51469c02d4c065abda53cfc3d0f648ee2897f7337f1144bddbebf1ca` |
| Antd.Wpf.Tokens.1.0.0.nupkg | 25 KB | `76006aa65225aa34e4828b66e94a2f8d6b9e506e4647326f8b3081d0e4c632bf` |

这些包与 nuget.org 上的是同一份构建（`v1.0.0` 标签），符号包 `.snupkg` 见 nuget.org。
## 许可证

MIT。
