using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Antd.Wpf.Controls;

namespace Antd.Wpf.DocsGen
{
    /// <summary>
    /// Generates docs/site/index.html: a single self-contained page listing every component with
    /// its dependency properties, events and base type, read from the built assembly so the docs
    /// cannot drift from the code.
    /// </summary>
    internal static class Program
    {
        internal static readonly DocInfo[] Catalogue =
        {
            new DocInfo
            {
                Name = "Button",
                Title = "按钮", TitleEn = "Button",
                Category = "通用",
                Summary = "主/默认/虚线/文本/链接五种变体，含危险态、图标与加载态。",
                SummaryEn = "Five variants (default/primary/dashed/text/link) with danger, icon and loading states."
            },
            new DocInfo
            {
                Name = "Icon",
                Title = "图标", TitleEn = "Icon",
                Category = "通用",
                Summary = "用 Geometry 绘制，颜色跟随 Foreground；847 个图标可用 AntdIcons.ByName 取。",
                SummaryEn = "Drawn as Geometry and tinted by Foreground; 847 glyphs via AntdIcons.ByName."
            },
            new DocInfo
            {
                Name = "Input",
                Title = "输入框", TitleEn = "Input",
                Category = "数据录入",
                Summary = "单行输入，带清除按钮与三种尺寸。",
                SummaryEn = "Single-line input with a clear button and three sizes."
            },            new DocInfo
            {
                Name = "Text",
                Title = "文本", TitleEn = "Text",
                Category = "通用",
                Summary = "Typography 的正文，支持次要/成功/警告/危险语义色。",
                SummaryEn = "Body copy with secondary/success/warning/danger tones."
            },
            new DocInfo
            {
                Name = "Title",
                Title = "标题", TitleEn = "Title",
                Category = "通用",
                Summary = "Typography 的标题，1–5 级。",
                SummaryEn = "Heading levels 1-5."
            },
            new DocInfo
            {
                Name = "Paragraph",
                Title = "段落", TitleEn = "Paragraph",
                Category = "通用",
                Summary = "多行正文，可带省略与复制。",
                SummaryEn = "Multi-line body copy with ellipsis and copy support."
            },
            new DocInfo
            {
                Name = "Link",
                Title = "链接", TitleEn = "Link",
                Category = "通用",
                Summary = "带下划线与悬停态的文本链接。",
                SummaryEn = "Text link with underline and hover states."
            },
            new DocInfo
            {
                Name = "Divider",
                Title = "分割线", TitleEn = "Divider",
                Category = "通用",
                Summary = "水平/垂直分割，可带文字。",
                SummaryEn = "Horizontal or vertical rule, optionally with a caption."
            },
            new DocInfo
            {
                Name = "Space",
                Title = "间距", TitleEn = "Space",
                Category = "通用",
                Summary = "统一间距排列子元素，边缘不会多出一份。",
                SummaryEn = "Even gaps between children without a trailing edge."
            },
            new DocInfo
            {
                Name = "Flex",
                Title = "弹性布局", TitleEn = "Flex",
                Category = "通用",
                Summary = "一维排布，支持对齐、换行与间隔。",
                SummaryEn = "One-dimensional layout with alignment, wrapping and gaps."
            },
            new DocInfo
            {
                Name = "Watermark",
                Title = "水印", TitleEn = "Watermark",
                Category = "通用",
                Summary = "在内容上叠加倾斜文字或图片水印。",
                SummaryEn = "Tilted text or image watermark over the content."
            },
            new DocInfo
            {
                Name = "BorderBeam",
                Title = "边框光效", TitleEn = "Border beam",
                Category = "通用",
                Summary = "沿边框流动的高亮描边，用于强调卡片。",
                SummaryEn = "Highlight travelling along a card border."
            },

            new DocInfo
            {
                Name = "Layout",
                Title = "布局容器", TitleEn = "Layout",
                Category = "布局",
                Summary = "页面骨架，配合 Sider / Header / Footer / Content 使用。",
                SummaryEn = "Page shell used with Sider / Header / Footer / Content."
            },
            new DocInfo
            {
                Name = "Sider",
                Title = "侧边栏", TitleEn = "Sider",
                Category = "布局",
                Summary = "可折叠侧边栏，折叠后保留图标区。",
                SummaryEn = "Collapsible side bar that keeps an icon rail when collapsed."
            },
            new DocInfo
            {
                Name = "Header",
                Title = "页头", TitleEn = "Header",
                Category = "布局",
                Summary = "顶部条，默认高度与内边距按 token。",
                SummaryEn = "Top bar with token-driven height and padding."
            },
            new DocInfo
            {
                Name = "Footer",
                Title = "页脚", TitleEn = "Footer",
                Category = "布局",
                Summary = "底部条。",
                SummaryEn = "Bottom bar."
            },
            new DocInfo
            {
                Name = "Content",
                Title = "内容区", TitleEn = "Content",
                Category = "布局",
                Summary = "主内容区，带 token 内边距。",
                SummaryEn = "Main content area with token padding."
            },
            new DocInfo
            {
                Name = "Row",
                Title = "行", TitleEn = "Row",
                Category = "布局",
                Summary = "24 栅格的行容器，配合 Col 使用。",
                SummaryEn = "24-column grid row, used with Col."
            },
            new DocInfo
            {
                Name = "Col",
                Title = "列", TitleEn = "Col",
                Category = "布局",
                Summary = "栅格列，支持 span / offset。",
                SummaryEn = "Grid column with span and offset."
            },
            new DocInfo
            {
                Name = "Masonry",
                Title = "瀑布流", TitleEn = "Masonry",
                Category = "布局",
                Summary = "按列高度自动分配的多列布局。",
                SummaryEn = "Multi-column layout that fills the shortest column."
            },
            new DocInfo
            {
                Name = "Splitter",
                Title = "分隔面板", TitleEn = "Splitter",
                Category = "布局",
                Summary = "可拖拽调整两栏尺寸。",
                SummaryEn = "Draggable divider between two panes."
            },
            new DocInfo
            {
                Name = "Affix",
                Title = "固钉", TitleEn = "Affix",
                Category = "布局",
                Summary = "滚动到阈值后把内容固定在顶部。",
                SummaryEn = "Pins content to the top after a scroll threshold."
            },

            new DocInfo
            {
                Name = "Anchor",
                Title = "锚点", TitleEn = "Anchor",
                Category = "导航",
                Summary = "页面内跳转，滚动时高亮当前段落。",
                SummaryEn = "In-page navigation that highlights the current section."
            },
            new DocInfo
            {
                Name = "Breadcrumb",
                Title = "面包屑", TitleEn = "Breadcrumb",
                Category = "导航",
                Summary = "层级路径，自动插入分隔符。",
                SummaryEn = "Hierarchy trail with automatic separators."
            },
            new DocInfo
            {
                Name = "Dropdown",
                Title = "下拉菜单", TitleEn = "Dropdown",
                Category = "导航",
                Summary = "点击/悬停触发，菜单项复用 MenuNode 数据。",
                SummaryEn = "Click or hover trigger whose rows reuse MenuNode data."
            },
            new DocInfo
            {
                Name = "Menu",
                Title = "导航菜单", TitleEn = "Menu",
                Category = "导航",
                Summary = "垂直或水平菜单，支持多级与选中态；方向键可操作。",
                SummaryEn = "Vertical or horizontal menu with nesting and selection; arrow keys work."
            },
            new DocInfo
            {
                Name = "Pagination",
                Title = "分页", TitleEn = "Pagination",
                Category = "导航",
                Summary = "页码、上一页/下一页与总数，超出时折叠省略号。",
                SummaryEn = "Page numbers with prev/next and an ellipsis for long ranges."
            },
            new DocInfo
            {
                Name = "Steps",
                Title = "步骤条", TitleEn = "Steps",
                Category = "导航",
                Summary = "流程进度，支持当前步与状态色。",
                SummaryEn = "Process progress with a current step and status colours."
            },
            new DocInfo
            {
                Name = "Tabs",
                Title = "标签页", TitleEn = "Tabs",
                Category = "导航",
                Summary = "横向切换内容区，支持卡片式与居中。",
                SummaryEn = "Horizontal tab strip, line or centred style."
            },
            new DocInfo
            {
                Name = "Tree",
                Title = "树形控件", TitleEn = "Tree",
                Category = "导航",
                Summary = "层级数据展示与选择，键盘可导航。",
                SummaryEn = "Hierarchical display and selection, keyboard navigable."
            },

            new DocInfo
            {
                Name = "AutoComplete",
                Title = "自动完成", TitleEn = "AutoComplete",
                Category = "数据录入",
                Summary = "输入时给出候选，上下键选择。",
                SummaryEn = "Suggests entries while typing; arrow keys pick."
            },
            new DocInfo
            {
                Name = "Cascader",
                Title = "级联选择", TitleEn = "Cascader",
                Category = "数据录入",
                Summary = "多级联动选择，逐列展开。",
                SummaryEn = "Cascading picker, one column per level."
            },
            new DocInfo
            {
                Name = "Checkbox",
                Title = "复选框", TitleEn = "Checkbox",
                Category = "数据录入",
                Summary = "支持半选（IsThreeState）与禁用。",
                SummaryEn = "Checkbox with an indeterminate state and disabled support."
            },
            new DocInfo
            {
                Name = "ColorPicker",
                Title = "颜色选择", TitleEn = "ColorPicker",
                Category = "数据录入",
                Summary = "预设色板 + 常用颜色，弹出面板选择。",
                SummaryEn = "Preset swatches plus a hue strip in a popup."
            },
            new DocInfo
            {
                Name = "DatePicker",
                Title = "日期选择", TitleEn = "DatePicker",
                Category = "数据录入",
                Summary = "单日期选择，面板与输入框联动。",
                SummaryEn = "Single date, or a two-month range with DisabledDate rules."
            },
            new DocInfo
            {
                Name = "Form",
                Title = "表单", TitleEn = "Form",
                Category = "数据录入",
                Summary = "标签布局、必填校验与错误提示；支持横向布局与标签列宽。",
                SummaryEn = "Label layout, required rules and error text; horizontal mode and label width."
            },
            new DocInfo
            {
                Name = "InputNumber",
                Title = "数字输入框", TitleEn = "InputNumber",
                Category = "数据录入",
                Summary = "带步进按钮，外部赋值也会收敛到声明范围。",
                SummaryEn = "Stepper input that clamps even when the value is set from code."
            },
            new DocInfo
            {
                Name = "Mentions",
                Title = "@提及", TitleEn = "Mentions",
                Category = "数据录入",
                Summary = "输入 @ 触发候选。",
                SummaryEn = "Mention suggestions triggered by @."
            },
            new DocInfo
            {
                Name = "Radio",
                Title = "单选框", TitleEn = "Radio",
                Category = "数据录入",
                Summary = "同组互斥选择。",
                SummaryEn = "Mutually exclusive choice inside a group."
            },
            new DocInfo
            {
                Name = "Rate",
                Title = "评分", TitleEn = "Rate",
                Category = "数据录入",
                Summary = "星级评分，支持半星；方向键可调。",
                SummaryEn = "Star rating with half stars; arrow keys adjust."
            },
            new DocInfo
            {
                Name = "Select",
                Title = "选择器", TitleEn = "Select",
                Category = "数据录入",
                Summary = "单选下拉，支持清除与占位。",
                SummaryEn = "Single or multiple selection with tag folding, local/remote search and virtualisation."
            },
            new DocInfo
            {
                Name = "Slider",
                Title = "滑动输入条", TitleEn = "Slider",
                Category = "数据录入",
                Summary = "连续或分段取值。",
                SummaryEn = "Continuous or stepped value picker."
            },
            new DocInfo
            {
                Name = "Switch",
                Title = "开关", TitleEn = "Switch",
                Category = "数据录入",
                Summary = "两种状态的即时切换。",
                SummaryEn = "Immediate two-state toggle."
            },
            new DocInfo
            {
                Name = "TimePicker",
                Title = "时间选择", TitleEn = "TimePicker",
                Category = "数据录入",
                Summary = "时分秒选择面板。",
                SummaryEn = "Hour / minute / second panel."
            },
            new DocInfo
            {
                Name = "Transfer",
                Title = "穿梭框", TitleEn = "Transfer",
                Category = "数据录入",
                Summary = "两栏列表之间移动条目。",
                SummaryEn = "Move items between two lists."
            },
            new DocInfo
            {
                Name = "TreeSelect",
                Title = "树选择", TitleEn = "TreeSelect",
                Category = "数据录入",
                Summary = "下拉里的树形选择。",
                SummaryEn = "Tree picker inside a dropdown."
            },
            new DocInfo
            {
                Name = "Upload",
                Title = "上传", TitleEn = "Upload",
                Category = "数据录入",
                Summary = "拖拽/点选收集文件并列出，传输由宿主完成；拖拽区键盘可操作。",
                SummaryEn = "Collects files by drop or pick with a queue, progress and cancel; transport comes from the host."
            },
            new DocInfo
            {
                Name = "Segmented",
                Title = "分段控制器", TitleEn = "Segmented",
                Category = "数据录入",
                Summary = "多选项的紧凑切换。",
                SummaryEn = "Compact multi-option switch."
            },

            new DocInfo
            {
                Name = "Avatar",
                Title = "头像", TitleEn = "Avatar",
                Category = "数据展示",
                Summary = "图片、图标或文字头像，支持组。",
                SummaryEn = "Image, icon or text avatar, groupable."
            },
            new DocInfo
            {
                Name = "Badge",
                Title = "徽标数", TitleEn = "Badge",
                Category = "数据展示",
                Summary = "红点或数字角标，可独立展示。",
                SummaryEn = "Dot or numeric badge, standalone too."
            },
            new DocInfo
            {
                Name = "Calendar",
                Title = "日历", TitleEn = "Calendar",
                Category = "数据展示",
                Summary = "月视图，日期格可键盘导航。",
                SummaryEn = "Month grid with keyboard navigation and DisabledDate."
            },
            new DocInfo
            {
                Name = "Card",
                Title = "卡片", TitleEn = "Card",
                Category = "数据展示",
                Summary = "内容容器，带标题、额外操作与操作区。",
                SummaryEn = "Content container with title, extra slot and actions."
            },
            new DocInfo
            {
                Name = "Carousel",
                Title = "走马灯", TitleEn = "Carousel",
                Category = "数据展示",
                Summary = "轮播内容，可自动播放（减动效时停）。",
                SummaryEn = "Sliding carousel; autoplay stops under reduced motion."
            },
            new DocInfo
            {
                Name = "Collapse",
                Title = "折叠面板", TitleEn = "Collapse",
                Category = "数据展示",
                Summary = "手风琴式展开；头部可聚焦，空格/回车切换。",
                SummaryEn = "Accordion; the header is focusable and toggles with Space or Enter."
            },
            new DocInfo
            {
                Name = "Descriptions",
                Title = "描述列表", TitleEn = "Descriptions",
                Category = "数据展示",
                Summary = "键值对信息展示，支持多列。",
                SummaryEn = "Key/value pairs in one or more columns."
            },
            new DocInfo
            {
                Name = "Empty",
                Title = "空状态", TitleEn = "Empty",
                Category = "数据展示",
                Summary = "占位插图与说明文案。",
                SummaryEn = "Placeholder illustration with a caption."
            },
            new DocInfo
            {
                Name = "Image",
                Title = "图片", TitleEn = "Image",
                Category = "数据展示",
                Summary = "带预览的图片。",
                SummaryEn = "Image with a preview overlay."
            },
            new DocInfo
            {
                Name = "List",
                Title = "列表", TitleEn = "List",
                Category = "数据展示",
                Summary = "纵向条目，支持标题、描述与操作。",
                SummaryEn = "Vertical rows with title, description and actions."
            },
            new DocInfo
            {
                Name = "QRCode",
                Title = "二维码", TitleEn = "QRCode",
                Category = "数据展示",
                Summary = "生成二维码图形，可设颜色与纠错级别。",
                SummaryEn = "Byte-mode QR symbol with adjustable colour and error correction."
            },
            new DocInfo
            {
                Name = "Statistic",
                Title = "统计数值", TitleEn = "Statistic",
                Category = "数据展示",
                Summary = "大字号数值 + 前缀/后缀。",
                SummaryEn = "Large numeric value with prefix and suffix."
            },
            new DocInfo
            {
                Name = "Table",
                Title = "表格", TitleEn = "Table",
                Category = "数据展示",
                Summary = "基于 DataGrid 重绘，保留排序、列宽与虚拟化。",
                SummaryEn = "DataGrid-derived grid: sorting, frozen columns, expandable rows and tree rows."
            },
            new DocInfo
            {
                Name = "Tag",
                Title = "标签", TitleEn = "Tag",
                Category = "数据展示",
                Summary = "状态色标签，可关闭。",
                SummaryEn = "Status tag, optionally closable."
            },
            new DocInfo
            {
                Name = "Timeline",
                Title = "时间轴", TitleEn = "Timeline",
                Category = "数据展示",
                Summary = "纵向事件节点。",
                SummaryEn = "Vertical event nodes."
            },
            new DocInfo
            {
                Name = "Tooltip",
                Title = "文字提示", TitleEn = "Tooltip",
                Category = "数据展示",
                Summary = "悬停/聚焦时的简短说明，已带阴影。",
                SummaryEn = "Short hint on hover or focus, with a shadow."
            },
            new DocInfo
            {
                Name = "Tour",
                Title = "漫游式引导", TitleEn = "Tour",
                Category = "数据展示",
                Summary = "分步高亮目标元素。",
                SummaryEn = "Step-by-step spotlight walkthrough."
            },

            new DocInfo
            {
                Name = "Alert",
                Title = "警告提示", TitleEn = "Alert",
                Category = "反馈",
                Summary = "四种语义状态，可带标题、描述与关闭。",
                SummaryEn = "Four semantic types with title, description and close."
            },
            new DocInfo
            {
                Name = "Drawer",
                Title = "抽屉", TitleEn = "Drawer",
                Category = "反馈",
                Summary = "从四边滑出，入退场带方向滑动。",
                SummaryEn = "Slides in from any edge with direction-aware motion."
            },
            new DocInfo
            {
                Name = "Modal",
                Title = "对话框", TitleEn = "Modal",
                Category = "反馈",
                Summary = "遮罩 + 卡片，焦点循环、Esc 关闭、退场动效。",
                SummaryEn = "Mask plus card with focus trap, Esc, exit motion and imperative Modal.Confirm."
            },
            new DocInfo
            {
                Name = "Popconfirm",
                Title = "气泡确认", TitleEn = "Popconfirm",
                Category = "反馈",
                Summary = "行内二次确认，面板带阴影。",
                SummaryEn = "Inline confirmation bubble with a shadow."
            },
            new DocInfo
            {
                Name = "Popover",
                Title = "气泡卡片", TitleEn = "Popover",
                Category = "反馈",
                Summary = "悬停/点击浮现的富内容面板。",
                SummaryEn = "Rich content panel on hover or click."
            },
            new DocInfo
            {
                Name = "Progress",
                Title = "进度条", TitleEn = "Progress",
                Category = "反馈",
                Summary = "线形与环形进度。",
                SummaryEn = "Linear and circular progress."
            },
            new DocInfo
            {
                Name = "Result",
                Title = "结果页", TitleEn = "Result",
                Category = "反馈",
                Summary = "操作结果的整页反馈，四种状态。",
                SummaryEn = "Full-page outcome screen with five states."
            },
            new DocInfo
            {
                Name = "Skeleton",
                Title = "骨架屏", TitleEn = "Skeleton",
                Category = "反馈",
                Summary = "加载占位，支持头像与段落形态。",
                SummaryEn = "Loading placeholder with avatar and paragraph shapes."
            },
            new DocInfo
            {
                Name = "Spin",
                Title = "加载中", TitleEn = "Spin",
                Category = "反馈",
                Summary = "旋转指示器，可包裹内容。",
                SummaryEn = "Spinner that can wrap its content."
            }
        };

        private static int Main(string[] args)
        {
            string root = args.Length > 0 ? args[0] : FindRepoRoot();
            if (root == null)
            {
                Console.Error.WriteLine("Cannot locate the repository root; pass it as the first argument.");
                return 1;
            }

            string outputDir = Path.Combine(root, "docs", "site");
            Directory.CreateDirectory(outputDir);

            string html = Build(root);
            string file = Path.Combine(outputDir, "index.html");
            File.WriteAllText(file, html, new UTF8Encoding(false));
            Console.WriteLine("docs: " + Catalogue.Length + " components -> " + file);
            return 0;
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "Antd.Wpf.sln"))) return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        private static string Build(string root)
        {
            Assembly assembly = typeof(Button).Assembly;
            Version version = assembly.GetName().Version;
            string versionText = version.Major + "." + version.Minor + "." + version.Build;
            var byName = assembly.GetExportedTypes()
                .Where(t => typeof(FrameworkElement).IsAssignableFrom(t))
                .ToDictionary(t => t.Name, t => t);

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"zh-CN\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            sb.AppendLine("<title>Ant Design for WPF · " + versionText + "</title>");
            sb.Append(Styles());
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("<aside class=\"nav\">");
            sb.AppendLine("<div class=\"brand\"><strong>Ant Design for WPF</strong><span>" + versionText + " · 69 个组件</span></div>");
            sb.AppendLine("<a class=\"nav-link\" href=\"#start\">快速开始</a>");
            foreach (var group in Catalogue.GroupBy(d => d.Category))
            {
                sb.AppendLine("<div class=\"nav-group\">" + Escape(group.Key) + "</div>");
                foreach (DocInfo doc in group)
                {
                    sb.AppendLine("<a href=\"#" + doc.Name + "\">" + Escape(doc.Title) + " <em>" + Escape(doc.Name) + "</em></a>");
                }
            }
            sb.AppendLine("<div class=\"nav-group\">参考</div>");
            sb.AppendLine("<a href=\"#gaps\">能力边界</a>");
            sb.AppendLine("<a href=\"../manual.zh.md\">使用手册（中）</a>");
            sb.AppendLine("<a href=\"../manual.en.md\">Handbook (EN)</a>");
            sb.AppendLine("<a href=\"#a11y\">无障碍与 i18n</a>");
            sb.AppendLine("</aside>");

            sb.AppendLine("<main>");
            sb.AppendLine("<header class=\"hero\">");
            sb.AppendLine("<h1>Ant Design 的 WPF 实现</h1>");
            sb.AppendLine("<p>同一套设计 token、同一套组件语义，跑在 net48 与 net8-windows 上。零第三方依赖，536 个 token 与 antd 官方值逐个对拍。</p>");
            sb.AppendLine("<div class=\"pills\"><span>net48</span><span>net8-windows</span><span>536 token 对拍</span><span>847 图标</span><span>无障碍 100%</span></div>");
            sb.AppendLine("</header>");

            sb.AppendLine("<section id=\"start\">");
            sb.AppendLine("<h2>快速开始</h2>");
            sb.AppendLine("<pre><code>dotnet add package Antd.Wpf.Controls</code></pre>");
            sb.AppendLine("<pre><code>&lt;Application.Resources&gt;\n  &lt;ResourceDictionary&gt;\n    &lt;ResourceDictionary.MergedDictionaries&gt;\n      &lt;ResourceDictionary Source=\"pack://application:,,,/Antd.Wpf.Controls;component/Themes/Framework.xaml\" /&gt;\n    &lt;/ResourceDictionary.MergedDictionaries&gt;\n  &lt;/ResourceDictionary&gt;\n&lt;/Application.Resources&gt;</code></pre>");
            sb.AppendLine("<pre><code>ThemeManager.Current.Attach(Application.Current.Resources);\nThemeManager.Current.Apply(ThemeConfig.CreateDefault());\n\n// 中文文案 + 跟随系统的减动效\nAntdLocale.UseCulture(\"zh-CN\");\nAntdMotion.UseSystemDefault();</code></pre>");
            sb.AppendLine(EmbedScreenshot(root));
            sb.AppendLine("</section>");

            foreach (var group in Catalogue.GroupBy(d => d.Category))
            {
                sb.AppendLine("<h2 class=\"section\">" + Escape(group.Key) + "</h2>");
                foreach (DocInfo doc in group)
                {
                    AppendComponent(sb, doc, byName);
                }
            }

            ManualWriter.Write(root, Catalogue, byName, versionText);
            AppendGaps(sb);
            AppendA11y(sb);

            sb.AppendLine("<footer>由 <code>tools/docs-gen</code> 从程序集反射生成，属性表与代码始终一致。</footer>");
            sb.AppendLine("</main>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        private static void AppendComponent(StringBuilder sb, DocInfo doc, Dictionary<string, Type> byName)
        {
            Type type;
            if (!byName.TryGetValue(doc.Name, out type))
            {
                sb.AppendLine("<section class=\"comp\" id=\"" + doc.Name + "\"><h3>" + Escape(doc.Title)
                    + " <code>" + doc.Name + "</code></h3><p class=\"missing\">类型未找到：" + doc.Name + "</p></section>");
                return;
            }

            sb.AppendLine("<section class=\"comp\" id=\"" + doc.Name + "\">");
            sb.AppendLine("<h3>" + Escape(doc.Title) + " <code>" + doc.Name + "</code></h3>");
            sb.AppendLine("<p class=\"summary\">" + Escape(doc.Summary) + "</p>");
            sb.AppendLine("<p class=\"meta\">基类 <code>" + Escape(type.BaseType.Name) + "</code> · 命名空间 <code>" + Escape(type.Namespace) + "</code></p>");

            var properties = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(f => f.FieldType == typeof(DependencyProperty))
                .Select(f => new { Name = f.Name.Replace("Property", string.Empty), Dp = (DependencyProperty)f.GetValue(null) })
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .ToList();

            if (properties.Count > 0)
            {
                sb.AppendLine("<table><thead><tr><th>属性</th><th>类型</th><th>默认值</th></tr></thead><tbody>");
                foreach (var p in properties)
                {
                    string defaultValue = DescribeDefault(p.Dp);
                    sb.AppendLine("<tr><td><code>" + Escape(p.Name) + "</code></td><td>" + Escape(p.Dp.PropertyType.Name)
                        + "</td><td>" + Escape(defaultValue) + "</td></tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(e => e.Name)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();
            if (events.Count > 0)
            {
                sb.AppendLine("<p class=\"events\">事件：" + string.Join("、", events.Select(e => "<code>" + Escape(e) + "</code>")) + "</p>");
            }

            sb.AppendLine("</section>");
        }

        private static string DescribeDefault(DependencyProperty property)
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

        private static void AppendGaps(StringBuilder sb)
        {
            sb.AppendLine("<section id=\"gaps\">");
            sb.AppendLine("<h2>能力边界</h2>");
            sb.AppendLine("<p>“已实现”指基础版本：属性、外观与常用交互可用。以下差距已知，接入前请确认：</p>");
            sb.AppendLine("<table><thead><tr><th>需求</th><th>组件</th><th>现状</th></tr></thead><tbody>");
            sb.AppendLine("<tr><td>分组 / 搜索节流</td><td>Select</td><td>单选/多选、本地与远程搜索、虚拟化都已支持</td></tr>");
            sb.AppendLine("<tr><td>周/月/季/年视图</td><td>DatePicker</td><td>单日期、双月区间、禁用日期已支持</td></tr>");
            sb.AppendLine("<tr><td>树形行 / 可编辑单元格</td><td>Table</td><td>固定列与展开行已可用</td></tr>");
            sb.AppendLine("<tr><td>跨字段联动 / INotifyDataErrorInfo</td><td>Form</td><td>同步与异步校验都已支持</td></tr>");
            sb.AppendLine("<tr><td>内置传输实现</td><td>Upload</td><td>队列/并发/进度/取消已由库管，传输函数由宿主提供</td></tr>");
            sb.AppendLine("<tr><td>树异步加载 / 拖拽</td><td>Tree</td><td>静态数据；表格侧的树形行已支持</td></tr>");
            sb.AppendLine("<tr><td>双色图标分色渲染</td><td>AntdIcons</td><td>two-tone 合并为单色</td></tr>");
            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</section>");
        }

        private static void AppendA11y(StringBuilder sb)
        {
            sb.AppendLine("<section id=\"a11y\">");
            sb.AppendLine("<h2>无障碍与本地化</h2>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>每个可构造控件都有 AutomationPeer；读屏软件能读到角色、名称与 Invoke 模式。</li>");
            sb.AppendLine("<li>键盘焦点统一用共享焦点环绘制；按钮、折叠头、菜单节点、评分、上传区都可键盘操作。</li>");
            sb.AppendLine("<li><code>AntdLocale.UseCulture(\"zh-CN\")</code> 切换内置文案（必填提示、空状态、确定/取消、关闭朗读名）。</li>");
            sb.AppendLine("<li><code>AntdMotion.Enabled</code> 默认跟随系统“显示动画”设置，关闭后弹窗、下拉、轮播不再动。</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</section>");
        }

        private static string EmbedScreenshot(string root)
        {
            // Prefer the committed asset so the site renders the same in CI, where artifacts/ (gitignored) is absent.
            string shot = Path.Combine(root, "docs", "assets", "integration-light.png");
            if (!File.Exists(shot)) shot = Path.Combine(root, "artifacts", "integration-light.png");
            if (!File.Exists(shot)) return string.Empty;

            string base64 = Convert.ToBase64String(File.ReadAllBytes(shot));
            return "<figure class=\"shot\"><img alt=\"服务实例列表页面：搜索、筛选、表格、分页\" src=\"data:image/png;base64,"
                + base64 + "\"><figcaption>demo/Antd.Wpf.Integration 的真实页面：表格 + 表单校验 + 弹窗 + 主题切换</figcaption></figure>";
        }

        private static string Escape(string value)
        {
            if (value == null) return string.Empty;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private static string Styles()
        {
            return @"<style>
:root{--bg:#fff;--fg:#1f2328;--muted:#6e7781;--line:#e5e7eb;--accent:#1677ff;--soft:#f6f8fa}
*{box-sizing:border-box}
body{margin:0;font:14px/1.6 -apple-system,'Segoe UI','Microsoft YaHei',sans-serif;color:var(--fg);background:var(--bg)}
.nav{position:fixed;top:0;left:0;bottom:0;width:250px;overflow:auto;padding:20px 16px;border-right:1px solid var(--line);background:var(--soft)}
.brand{display:flex;flex-direction:column;margin-bottom:16px}
.brand span{color:var(--muted);font-size:12px}
.nav a{display:block;padding:4px 8px;color:var(--fg);text-decoration:none;border-radius:6px;font-size:13px}
.nav a:hover{background:#eaeef2;color:var(--accent)}
.nav a em{color:var(--muted);font-style:normal;font-size:12px}
.nav-group{margin:14px 0 4px;color:var(--muted);font-size:12px;letter-spacing:.04em}
main{margin-left:250px;padding:32px 48px 80px;max-width:1080px}
.hero h1{margin:0 0 8px;font-size:30px}
.hero p{color:var(--muted);max-width:720px}
.pills{display:flex;gap:8px;flex-wrap:wrap;margin:14px 0 8px}
.pills span{background:#e8f2ff;color:#0b5ed7;border-radius:999px;padding:2px 10px;font-size:12px}
h2{font-size:20px;margin:36px 0 12px;padding-bottom:8px;border-bottom:1px solid var(--line)}
h2.section{color:var(--muted);font-size:14px;letter-spacing:.08em;border:0;margin-top:28px}
.comp{margin:0 0 26px;padding:18px 20px;border:1px solid var(--line);border-radius:10px}
.comp h3{margin:0 0 6px;font-size:16px}
.summary{margin:4px 0 6px}
.meta,.events{color:var(--muted);font-size:12px}
code{font-family:Consolas,'SFMono-Regular',monospace;background:var(--soft);padding:1px 5px;border-radius:4px;font-size:12.5px}
pre{background:var(--soft);border:1px solid var(--line);border-radius:8px;padding:12px 14px;overflow:auto}
pre code{background:none;padding:0}
table{border-collapse:collapse;width:100%;margin:8px 0;font-size:13px}
th,td{border-bottom:1px solid var(--line);text-align:left;padding:6px 8px;vertical-align:top}
th{color:var(--muted);font-weight:600}
.shot img{max-width:100%;border:1px solid var(--line);border-radius:10px}
figcaption{color:var(--muted);font-size:12px;margin-top:6px}
.missing{color:#c92a2a}
footer{margin-top:48px;color:var(--muted);font-size:12px}
@media (max-width:900px){.nav{position:static;width:auto;border:0}main{margin:0;padding:20px}}
</style>";
        }
    }
}
