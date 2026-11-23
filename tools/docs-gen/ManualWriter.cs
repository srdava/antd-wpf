using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Antd.Wpf.DocsGen
{
    /// <summary>One documented component: bilingual name, category and one-liner.</summary>
    internal sealed class DocInfo
    {
        public string Name;
        public string Title;
        public string TitleEn;
        public string Category;
        public string Summary;
        public string SummaryEn;
    }

    /// <summary>
    /// Writes docs/manual.zh.md and docs/manual.en.md. Property tables come from the built
    /// assembly, so a handbook page cannot disagree with the control it documents.
    /// </summary>
    internal static class ManualWriter
    {
        private static readonly Dictionary<string, string> Samples = new Dictionary<string, string>
        {
            {
                "Button",
                "<antd:Button Content=\"确定\" Variant=\"Primary\" Click=\"OnOk\" />\n" +
                "<antd:Button Content=\"取消\" />\n" +
                "<antd:Button Content=\"删除\" Danger=\"True\" />"
            },
            {
                "Input",
                "<antd:Input Width=\"240\" Placeholder=\"请输入\" AllowClear=\"True\"\n" +
                "           Text=\"{Binding Name, Mode=TwoWay}\" />\n\n" +
                "<!-- 前后缀 / clear -->\n" +
                "<antd:Input Prefix=\"https://\" Suffix=\".com\" AllowClear=\"True\" />"
            },
            {
                "Select",
                "<antd:Select Width=\"240\" Placeholder=\"选择状态\"\n" +
                "             ItemsSource=\"{Binding States}\"\n" +
                "             SelectedItem=\"{Binding State, Mode=TwoWay}\" />\n\n" +
                "<!-- 可搜索 + 多选（标签折叠） -->\n" +
                "<antd:Select Width=\"320\" IsMultiple=\"True\" MaxTagCount=\"3\"\n" +
                "             Filterable=\"True\" ItemsSource=\"{Binding Tags}\" />"
            },
            {
                "DatePicker",
                "<antd:DatePicker Width=\"220\" AllowClear=\"True\"\n" +
                "                SelectedDate=\"{Binding Date, Mode=TwoWay}\" />\n\n" +
                "<!-- 区间 -->\n" +
                "<antd:DatePicker Width=\"280\" IsRange=\"True\"\n" +
                "                RangeStart=\"{Binding From, Mode=TwoWay}\"\n" +
                "                RangeEnd=\"{Binding To, Mode=TwoWay}\" />\n\n" +
                "<!-- 代码里加禁用规则 -->\n" +
                "picker.DisabledDate = d => d < DateTime.Today;"
            },
            {
                "Form",
                "<antd:Form LabelWidth=\"96\">\n" +
                "  <antd:FormItem Label=\"名称\" Required=\"True\">\n" +
                "    <antd:Input Text=\"{Binding Name, Mode=TwoWay}\" />\n" +
                "  </antd:FormItem>\n" +
                "  <antd:FormItem Label=\"编码\" Required=\"True\" Width=\"120\" />\n" +
                "</antd:Form>\n\n" +
                "if (!await form.ValidateAsync()) return;   // 含异步校验"
            },
            {
                "Table",
                "<antd:Table ItemsSource=\"{Binding Rows}\" FrozenColumnCount=\"2\">\n" +
                "  <antd:Table.Columns>\n" +
                "    <DataGridTextColumn Header=\"名称\" Binding=\"{Binding Name}\" Width=\"180\" />\n" +
                "    <DataGridTextColumn Header=\"状态\" Binding=\"{Binding State}\" Width=\"120\" />\n" +
                "  </antd:Table.Columns>\n" +
                "</antd:Table>\n\n" +
                "<!-- 树形行：指定子集合属性即可 -->\n" +
                "<antd:Table ItemsSource=\"{Binding Nodes}\"\n" +
                "            TreeChildrenPath=\"Children\" TreeTextPath=\"Name\" />"
            },
            {
                "Modal",
                "<!-- 声明式 -->\n" +
                "<antd:Modal x:Name=\"EditModal\" Title=\"编辑\" OkText=\"保存\"\n" +
                "            Ok=\"OnSave\" Canceled=\"OnCancel\" />\n\n" +
                "<!-- 命令式：确认框不必写 XAML -->\n" +
                "Modal.Confirm(\"删除确认\", \"确定要删除这条记录吗？\", () => Delete(), () => { });\n" +
                "Modal.Info(\"提示\", \"保存成功\");"
            },
            {
                "Upload",
                "<antd:Upload Accept=\".csv,.xlsx\" AutoUpload=\"True\"\n" +
                "            Uploader=\"UploadFileAsync\" />\n\n" +
                "// 库只跑队列，传输函数由宿主提供\n" +
                "upload.Uploader = async (file, progress, token) =>\n" +
                "    await api.UploadAsync(file.Path, progress, token);"
            },
            {
                "Tabs",
                "<antd:Tabs SelectedIndex=\"{Binding TabIndex, Mode=TwoWay}\">\n" +
                "  <antd:TabsItem Header=\"概览\"><TextBlock Text=\"…\" /></antd:TabsItem>\n" +
                "  <antd:TabsItem Header=\"日志\"><TextBlock Text=\"…\" /></antd:TabsItem>\n" +
                "</antd:Tabs>"
            },
            {
                "Collapse",
                "<antd:Collapse>\n" +
                "  <antd:CollapseItem Header=\"基础信息\" IsExpanded=\"True\">\n" +
                "    <TextBlock Text=\"…\" />\n" +
                "  </antd:CollapseItem>\n" +
                "</antd:Collapse>"
            },
            {
                "Alert",
                "<antd:Alert Type=\"Success\" Message=\"保存成功\"\n" +
                "            Description=\"改动已同步到服务器。\" ShowClose=\"True\" />"
            },
            {
                "Pagination",
                "<antd:Pagination Total=\"{Binding Total}\" PageSize=\"10\"\n" +
                "               CurrentPage=\"{Binding Page, Mode=TwoWay}\" />"
            },
            {
                "Progress",
                "<antd:Progress Percent=\"{Binding Percent}\" />"
            },
            {
                "Menu",
                "<antd:Menu SelectedIndex=\"{Binding MenuIndex, Mode=TwoWay}\">\n" +
                "  <antd:MenuNode Header=\"服务\">\n" +
                "    <antd:MenuNode Header=\"实例列表\" />\n" +
                "  </antd:MenuNode>\n" +
                "  <antd:MenuNode Header=\"设置\" />\n" +
                "</antd:Menu>"
            },
            {
                "Tree",
                "<antd:Tree ItemsSource=\"{Binding Nodes}\" DisplayMemberPath=\"Name\" />"
            },
            {
                "Steps",
                "<antd:Steps Current=\"{Binding Step}\">\n" +
                "  <antd:StepsItem Title=\"填写\" />\n" +
                "  <antd:StepsItem Title=\"校验\" />\n" +
                "  <antd:StepsItem Title=\"完成\" />\n" +
                "</antd:Steps>"
            },
            {
                "Switch",
                "<antd:Switch IsChecked=\"{Binding Enabled, Mode=TwoWay}\" />"
            }
        };

        private static readonly Dictionary<string, string> SamplesEn = new Dictionary<string, string>
        {
            {
                "Button",
                "<antd:Button Content=\"OK\" Variant=\"Primary\" Click=\"OnOk\" />\n" +
                "<antd:Button Content=\"Cancel\" />\n" +
                "<antd:Button Content=\"Delete\" Danger=\"True\" />"
            },
            {
                "Input",
                "<antd:Input Width=\"240\" Placeholder=\"Type here\" AllowClear=\"True\"\n" +
                "           Text=\"{Binding Name, Mode=TwoWay}\" />\n\n" +
                "<!-- prefix / suffix / clear -->\n" +
                "<antd:Input Prefix=\"https://\" Suffix=\".com\" AllowClear=\"True\" />"
            },
            {
                "Select",
                "<antd:Select Width=\"240\" Placeholder=\"Select a state\"\n" +
                "             ItemsSource=\"{Binding States}\"\n" +
                "             SelectedItem=\"{Binding State, Mode=TwoWay}\" />\n\n" +
                "<!-- searchable, multiple, folded tags -->\n" +
                "<antd:Select Width=\"320\" IsMultiple=\"True\" MaxTagCount=\"3\"\n" +
                "             Filterable=\"True\" ItemsSource=\"{Binding Tags}\" />"
            },
            {
                "DatePicker",
                "<antd:DatePicker Width=\"220\" AllowClear=\"True\"\n" +
                "                SelectedDate=\"{Binding Date, Mode=TwoWay}\" />\n\n" +
                "<!-- range -->\n" +
                "<antd:DatePicker Width=\"280\" IsRange=\"True\"\n" +
                "                RangeStart=\"{Binding From, Mode=TwoWay}\"\n" +
                "                RangeEnd=\"{Binding To, Mode=TwoWay}\" />\n\n" +
                "<!-- block dates from code -->\n" +
                "picker.DisabledDate = d => d < DateTime.Today;"
            },
            {
                "Form",
                "<antd:Form LabelWidth=\"96\">\n" +
                "  <antd:FormItem Label=\"Name\" Required=\"True\">\n" +
                "    <antd:Input Text=\"{Binding Name, Mode=TwoWay}\" />\n" +
                "  </antd:FormItem>\n" +
                "  <antd:FormItem Label=\"Code\" Required=\"True\" Width=\"120\" />\n" +
                "</antd:Form>\n\n" +
                "if (!await form.ValidateAsync()) return;   // runs async validators too"
            },
            {
                "Table",
                "<antd:Table ItemsSource=\"{Binding Rows}\" FrozenColumnCount=\"2\">\n" +
                "  <antd:Table.Columns>\n" +
                "    <DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" Width=\"180\" />\n" +
                "    <DataGridTextColumn Header=\"State\" Binding=\"{Binding State}\" Width=\"120\" />\n" +
                "  </antd:Table.Columns>\n" +
                "</antd:Table>\n\n" +
                "<!-- tree rows: point at the children property -->\n" +
                "<antd:Table ItemsSource=\"{Binding Nodes}\"\n" +
                "            TreeChildrenPath=\"Children\" TreeTextPath=\"Name\" />"
            },
            {
                "Modal",
                "<!-- declarative -->\n" +
                "<antd:Modal x:Name=\"EditModal\" Title=\"Edit\" OkText=\"Save\"\n" +
                "            Ok=\"OnSave\" Canceled=\"OnCancel\" />\n\n" +
                "<!-- imperative: no XAML needed for a confirm -->\n" +
                "Modal.Confirm(\"Delete\", \"Remove this record?\", () => Delete(), () => { });\n" +
                "Modal.Info(\"Done\", \"Saved successfully\");"
            },
            {
                "Upload",
                "<antd:Upload Accept=\".csv,.xlsx\" AutoUpload=\"True\"\n" +
                "            Uploader=\"UploadFileAsync\" />\n\n" +
                "// the library only runs the queue; transport comes from the host\n" +
                "upload.Uploader = async (file, progress, token) =>\n" +
                "    await api.UploadAsync(file.Path, progress, token);"
            },
            {
                "Tabs",
                "<antd:Tabs SelectedIndex=\"{Binding TabIndex, Mode=TwoWay}\">\n" +
                "  <antd:TabsItem Header=\"Overview\"><TextBlock Text=\"...\" /></antd:TabsItem>\n" +
                "  <antd:TabsItem Header=\"Logs\"><TextBlock Text=\"...\" /></antd:TabsItem>\n" +
                "</antd:Tabs>"
            },
            {
                "Collapse",
                "<antd:Collapse>\n" +
                "  <antd:CollapseItem Header=\"Basic info\" IsExpanded=\"True\">\n" +
                "    <TextBlock Text=\"...\" />\n" +
                "  </antd:CollapseItem>\n" +
                "</antd:Collapse>"
            },
            {
                "Alert",
                "<antd:Alert Type=\"Success\" Message=\"Saved\"\n" +
                "            Description=\"Changes are synced to the server.\" ShowClose=\"True\" />"
            },
            {
                "Pagination",
                "<antd:Pagination Total=\"{Binding Total}\" PageSize=\"10\"\n" +
                "               CurrentPage=\"{Binding Page, Mode=TwoWay}\" />"
            },
            {
                "Menu",
                "<antd:Menu SelectedIndex=\"{Binding MenuIndex, Mode=TwoWay}\">\n" +
                "  <antd:MenuNode Header=\"Services\">\n" +
                "    <antd:MenuNode Header=\"Instances\" />\n" +
                "  </antd:MenuNode>\n" +
                "  <antd:MenuNode Header=\"Settings\" />\n" +
                "</antd:Menu>"
            },
            {
                "Steps",
                "<antd:Steps Current=\"{Binding Step}\">\n" +
                "  <antd:StepsItem Title=\"Fill in\" />\n" +
                "  <antd:StepsItem Title=\"Validate\" />\n" +
                "  <antd:StepsItem Title=\"Done\" />\n" +
                "</antd:Steps>"
            }
        };
        public static void Write(string root, IList<DocInfo> catalogue, Dictionary<string, Type> byName, string version)
        {
            File.WriteAllText(Path.Combine(root, "docs", "manual.zh.md"), Build(false, catalogue, byName, version), new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(root, "docs", "manual.en.md"), Build(true, catalogue, byName, version), new UTF8Encoding(false));
            Console.WriteLine("manual: " + catalogue.Count + " components -> docs/manual.zh.md + docs/manual.en.md");
        }

        private static string Build(bool en, IList<DocInfo> catalogue, Dictionary<string, Type> byName, string version)
        {
            var sb = new StringBuilder();
            sb.AppendLine(en ? "# Antd.Wpf Handbook" : "# Antd.Wpf 使用手册");
            sb.AppendLine();
            sb.AppendLine(en
                ? "> Ant Design for WPF · " + version + " · **English** · [中文](manual.zh.md)"
                : "> Ant Design 规范的 WPF 实现 · " + version + " · **中文** · [English](manual.en.md)");
            sb.AppendLine();
            sb.AppendLine(en
                ? "Property tables in this handbook are generated from the compiled assembly (`tools/docs-gen`), so they cannot drift from the code."
                : "本手册的参数表由 `tools/docs-gen` 从编译后的程序集生成，不会与代码脱节。");
            sb.AppendLine();

            AppendIntro(sb, en);
            AppendStart(sb, en, version);
            AppendTheming(sb, en);
            AppendI18n(sb, en);
            AppendIndex(sb, en, catalogue);
            AppendComponentList(sb, en, catalogue, byName);
            AppendFaq(sb, en);
            return sb.ToString();
        }

        private static void AppendIntro(StringBuilder sb, bool en)
        {
            sb.AppendLine(en ? "## Basics" : "## 基础");
            sb.AppendLine();
            if (en)
            {
                sb.AppendLine("A WPF port of Ant Design that runs on .NET Framework 4.8 and .NET 8 with zero third-party dependencies.");
                sb.AppendLine("Controls read colours and metrics from the same design tokens as antd, so switching a theme replaces one immutable snapshot instead of rewriting styles.");
                sb.AppendLine();
                sb.AppendLine("| Item | Value |");
                sb.AppendLine("|---|---|");
                sb.AppendLine("| Frameworks | `net48`, `net8.0-windows` |");
                sb.AppendLine("| Packages | `Antd.Wpf.Controls` (pulls Theme / Icons / Tokens) |");
                sb.AppendLine("| Tokens | 536, value-compared against antd's `theme.getDesignToken()` |");
                sb.AppendLine("| Icons | 847 (outlined / filled / two-tone flattened to one colour) |");
                sb.AppendLine("| Accessibility | AutomationPeer on every constructible control, shared focus ring, keyboard paths |");
            }
            else
            {
                sb.AppendLine("Ant Design 的 WPF 实现，同时支持 .NET Framework 4.8 与 .NET 8，零第三方依赖。");
                sb.AppendLine("控件的颜色与尺寸全部取自与 antd 同一套设计 token，换主题是替换一份不可变快照，而不是改样式表。");
                sb.AppendLine();
                sb.AppendLine("| 项 | 取值 |");
                sb.AppendLine("|---|---|");
                sb.AppendLine("| 支持框架 | `net48`、`net8.0-windows` |");
                sb.AppendLine("| 包 | `Antd.Wpf.Controls`（自动带 Theme / Icons / Tokens） |");
                sb.AppendLine("| 设计 token | 536 个，与 antd `theme.getDesignToken()` 逐值对拍 |");
                sb.AppendLine("| 图标 | 847 个（outlined / filled / twotone，twotone 合并为单色） |");
                sb.AppendLine("| 无障碍 | 每个可构造控件都有 AutomationPeer；统一焦点环；键盘可达 |");
            }
            sb.AppendLine();
        }

        private static void AppendStart(StringBuilder sb, bool en, string version)
        {
            sb.AppendLine(en ? "## Getting started" : "## 快速开始");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("dotnet add package Antd.Wpf.Controls --version " + version);
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine(en
                ? "Merge the resource dictionary so the control templates can resolve tokens:"
                : "合并资源字典，让控件模板能取到 token：");
            sb.AppendLine();
            sb.AppendLine("```xml");
            sb.AppendLine("<Application.Resources>");
            sb.AppendLine("  <ResourceDictionary>");
            sb.AppendLine("    <ResourceDictionary.MergedDictionaries>");
            sb.AppendLine("      <ResourceDictionary Source=\"pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml\" />");
            sb.AppendLine("    </ResourceDictionary.MergedDictionaries>");
            sb.AppendLine("  </ResourceDictionary>");
            sb.AppendLine("</Application.Resources>");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine(en ? "Initialise the theme once at startup:" : "启动时初始化一次主题：");
            sb.AppendLine();
            sb.AppendLine("```csharp");
            sb.AppendLine("ThemeManager.Current.Attach(Application.Current.Resources);");
            sb.AppendLine("ThemeManager.Current.Apply(ThemeConfig.CreateDefault());");
            sb.AppendLine();
            sb.AppendLine("AntdLocale.UseCulture(\"zh-CN\");   // optional");
            sb.AppendLine("AntdMotion.UseSystemDefault();    // optional");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine(en
                ? "Add `xmlns:antd=\"clr-namespace:Antd.Wpf.Controls;assembly=Antd.Wpf.Controls\"` to the window."
                : "在窗口上加 `xmlns:antd=\"clr-namespace:Antd.Wpf.Controls;assembly=Antd.Wpf.Controls\"`。");
            sb.AppendLine();
        }

        private static void AppendTheming(StringBuilder sb, bool en)
        {
            sb.AppendLine(en ? "## Theming" : "## 主题");
            sb.AppendLine();
            sb.AppendLine("```csharp");
            sb.AppendLine("ThemeManager.Current.ApplyLight();");
            sb.AppendLine("ThemeManager.Current.ApplyDark();");
            sb.AppendLine("ThemeManager.Current.UpdateSeed(s => s.ColorPrimary = AntdColor.Parse(\"#00b96b\"));");
            sb.AppendLine("ThemeManager.Current.Apply(ThemeConfig.Create(null, ThemeAlgorithm.Dark | ThemeAlgorithm.Compact));");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine(en
                ? "Reference tokens in XAML (`{DynamicResource Antd.colorPrimary}`); `StaticResource` will not follow a switch. Code-behind reads `ThemeManager.Current.Snapshot.Tokens`."
                : "XAML 里引用 token（`{DynamicResource Antd.colorPrimary}`），用 `StaticResource` 不会跟随切换；代码里读 `ThemeManager.Current.Snapshot.Tokens`。");
            sb.AppendLine();
        }

        private static void AppendI18n(StringBuilder sb, bool en)
        {
            sb.AppendLine(en ? "## Localisation and accessibility" : "## 本地化与无障碍");
            sb.AppendLine();
            sb.AppendLine(en ? "- `AntdLocale.UseCulture(\"zh-CN\")` switches built-in strings; `AntdStrings.CreateEnglish()` for English." : "- `AntdLocale.UseCulture(\"zh-CN\")` 切中文文案；`AntdStrings.CreateEnglish()` 切英文。");
            sb.AppendLine(en ? "- `AntdMotion.Enabled = false` stops dialog, dropdown and carousel motion." : "- `AntdMotion.Enabled = false` 关闭弹窗、下拉、轮播动效。");
            sb.AppendLine(en ? "- Keyboard: Tab reaches every control; the focus ring comes from `Antd.FocusVisual`." : "- 键盘：Tab 可达所有控件；焦点环来自 `Antd.FocusVisual`。");
            sb.AppendLine();
        }

        private static void AppendIndex(StringBuilder sb, bool en, IList<DocInfo> catalogue)
        {
            sb.AppendLine(en ? "## Index" : "## 组件索引");
            sb.AppendLine();

            foreach (var group in catalogue.GroupBy(d => d.Category))
            {
                sb.AppendLine("**" + CategoryName(group.Key, en) + "**");
                sb.AppendLine();
                sb.AppendLine(en ? "| Component | Purpose |" : "| 组件 | 用途 |");
                sb.AppendLine("|---|---|");
                foreach (DocInfo doc in group)
                {
                    sb.AppendLine("| `" + doc.Name + "` | " + Escape(en ? doc.SummaryEn : doc.Summary) + " |");
                }
                sb.AppendLine();
            }
        }

        private static string CategoryName(string category, bool en)
        {
            if (!en) return category;
            switch (category)
            {
                case "通用": return "General";
                case "布局": return "Layout";
                case "导航": return "Navigation";
                case "数据录入": return "Data Entry";
                case "数据展示": return "Data Display";
                case "反馈": return "Feedback";
                default: return category;
            }
        }
        private static void AppendComponentList(StringBuilder sb, bool en, IList<DocInfo> catalogue, Dictionary<string, Type> byName)
        {
            sb.AppendLine(en ? "## Component reference" : "## 组件详解");
            sb.AppendLine();

            foreach (DocInfo doc in catalogue)
            {
                sb.AppendLine("### " + (en ? doc.TitleEn : doc.Title) + " `" + doc.Name + "`");
                sb.AppendLine();
                sb.AppendLine(en ? doc.SummaryEn : doc.Summary);
                sb.AppendLine();

                string sample;
                Dictionary<string, string> table = en ? SamplesEn : Samples;
                if (!table.TryGetValue(doc.Name, out sample)) sample = "<antd:" + doc.Name + " />";

                sb.AppendLine("```xml");
                sb.AppendLine(sample);
                sb.AppendLine("```");
                sb.AppendLine();

                Type type;
                if (!byName.TryGetValue(doc.Name, out type))
                {
                    sb.AppendLine(en ? "_Type not found in the assembly._" : "_程序集中找不到该类型。_");
                    sb.AppendLine();
                    continue;
                }

                var properties = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(f => f.FieldType == typeof(DependencyProperty))
                    .Select(f => new { Name = f.Name.Replace("Property", string.Empty), Dp = (DependencyProperty)f.GetValue(null) })
                    .Where(p => !string.IsNullOrEmpty(p.Name))
                    .OrderBy(p => p.Name, StringComparer.Ordinal)
                    .ToList();

                if (properties.Count > 0)
                {
                    sb.AppendLine(en ? "| Property | Type | Default |" : "| 参数 | 类型 | 默认值 |");
                    sb.AppendLine("|---|---|---|");
                    foreach (var p in properties)
                    {
                        sb.AppendLine("| `" + p.Name + "` | " + Escape(p.Dp.PropertyType.Name) + " | " + Escape(Default(p.Dp)) + " |");
                    }
                    sb.AppendLine();
                }

                var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Select(e => e.Name)
                    .OrderBy(n => n, StringComparer.Ordinal)
                    .ToList();
                if (events.Count > 0)
                {
                    sb.AppendLine((en ? "Events: " : "事件：") + string.Join(", ", events.Select(e => "`" + e + "`")));
                    sb.AppendLine();
                }
            }
        }

        private static void AppendFaq(StringBuilder sb, bool en)
        {
            sb.AppendLine(en ? "## Troubleshooting" : "## 常见问题");
            sb.AppendLine();
            sb.AppendLine(en ? "| Symptom | Cause | Fix |" : "| 现象 | 原因 | 处理 |");
            sb.AppendLine("|---|---|---|");
            if (en)
            {
                sb.AppendLine("| Text clipped inside an input | `TextBox.Padding` is copied into the content host | Fixed in 1.0; custom inputs should copy the `HostHeight` approach |");
                sb.AppendLine("| Theme switch does nothing | Style uses `StaticResource` | Switch to `{DynamicResource Antd.*}` |");
                sb.AppendLine("| Dropdown has no shadow | Only Select and the OverlayBase family got the shadow gutter | Known gap for DatePicker / TimePicker / Cascader |");
                sb.AppendLine("| No visible focus ring | A custom template dropped `FocusVisualStyle` | Reference `Antd.FocusVisual` |");
            }
            else
            {
                sb.AppendLine("| 输入框文字被裁 | `TextBox.Padding` 被复制到内容宿主 | 1.0 已修；自定义输入控件照抄 `HostHeight` 的做法 |");
                sb.AppendLine("| 切主题无效 | 样式里用了 `StaticResource` | 换成 `{DynamicResource Antd.*}` |");
                sb.AppendLine("| 下拉没有阴影 | 只有 Select 与 OverlayBase 系列做了阴影槽 | DatePicker / TimePicker / Cascader 仍未做 |");
                sb.AppendLine("| 看不到焦点环 | 自定义模板去掉了 `FocusVisualStyle` | 引用 `Antd.FocusVisual` |");
            }
            sb.AppendLine();
            sb.AppendLine(en
                ? "See also: [migration](migration.md) · [capability notes](progress.md) · [component site](site/index.html)"
                : "另见：[迁移指南](migration.md) · [能力边界](progress.md) · [组件文档站](site/index.html)");
            sb.AppendLine();
        }

        private static string Default(DependencyProperty property)
        {
            object value = property.DefaultMetadata == null ? null : property.DefaultMetadata.DefaultValue;
            if (value == null) return "null";
            if (value is string) return "\"" + value + "\"";
            if (value is bool) return (bool)value ? "true" : "false";
            if (value is double) return ((double)value).ToString("0.###");
            if (value is int) return value.ToString();
            if (value is Thickness) return value.ToString();
            if (value is System.Windows.Media.Brush) return "theme token";
            return value.GetType().Name;
        }

        private static string Escape(string value)
        {
            return value == null ? string.Empty : value.Replace("|", "\\|");
        }
    }
}
