# 进度状态

最后更新：2026-11-19

对照 `docs/antd-wpf-implementation-plan.md` 的 71 项组件清单。

## 里程碑

| 里程碑 | 计划项 | 已完成 | 说明 |
|---|---|---|---|
| M0 可行性 | 骨架 + 主题引擎 + 2 控件 | ✅ | DoD 全过：token 对拍、双 TFM、架构边界测试 |
| M1 原子层 | 22 | **22** | 清单全部落地 |
| M2 录入层 | 19 | **19** | 清单全部落地（能力边界见下） |
| M3 展示层 | 21 | **21** | 清单全部落地（能力边界见下） |
| M4 反馈层 | 7 | **7** | 清单全部落地（能力边界见下） |
| M5 打磨 | — | **完成** | 无障碍全量 Peer + 焦点环；i18n 中/英；入退场 + 减动效；弹窗/抽屉/Toast/浮层阴影；847 个图标（outlined/filled/twotone，懒加载） |
| M6 稳定发布 | — | **已发布** | 1.0.0 四包（MIT）2026-11-20 上线 nuget.org；文档站、迁移指南、包可用性检查齐备；剩 4 周观察期 |

**组件进度：69 / 71。** 公开控件类型 97 个（Typography 拆 4、Layout 拆 5、Row/Col 拆 2、
Tabs/Collapse/List/Steps/Timeline/Descriptions 各拆出 item 类型，另有若干数据模型类）。
余下 2 项不在任何里程碑内，属计划里标为「不实现」的部分。

## 接入验证（真实页面）

`demo/Antd.Wpf.Integration` 是一个能直接跑的页面：服务实例列表。它把控件放进真实流程里，
而不是像 Gallery 那样一件件摆开：

- 搜索框 + 状态筛选实时过滤，分页跟随结果重置；
- 表格多选驱动"批量停止"，行内"编辑 / 删除"；
- 新建 / 编辑走同一个弹窗：Required + 自定义校验（长度、重名），校验失败弹窗不关；
- 保存、删除、批量操作分别走 Message / Notification；
- 顶部"切换主题"在弹窗打开时也能切换，弹窗和提示层跟着换色。

`--selftest` 会驱动这个页面本身跑 28 项断言（不是另建一套控件），双 TFM 通过。

这一轮接入**抓到一个真 bug**：`Input` 的边框和内层 `ScrollViewer` 各吃了一次 `Padding`——
WPF 会把 `TextBox.Padding` 复制到内容宿主上，于是 22px 的文字区实际只剩 14.4px，字的上半截被裁掉。
同一问题也影响 `InputNumber`。修法：边框不再承担内边距，内容宿主高度改为"控件高 - 上下边框"，
前缀 / 后缀 / 占位符在代码里补内缩。**只跑断言发现不了这种问题，必须看真实页面。**
## M5 交付说明

| 能力 | 做法 | 验证 |
|---|---|---|
| 无障碍 | `Automation.cs`：Button（Invoke）/ Alert / Result / Tag / Modal / Drawer 专用 Peer，容器类用 `AntdContainerAutomationPeer`（Card、Pagination、Menu、Collapse、Steps、Descriptions、Timeline、List、Statistic、QRCode、Carousel、Transfer、Tour）。Button 补 2px 焦点环 | SmokeTests 8 项、FeedbackTests 8 项 |
| i18n | `AntdLocale` + `AntdStrings`（中/英）：必填提示、空状态、上传移除、文件对话框筛选、关闭按钮朗读名；控件通过 `LocaleLink` 跟随切换 | SmokeTests 7 项 |
| 动效降级 | `AntdMotion.Enabled` 默认跟随 `SystemParameters.ClientAreaAnimation`；关闭时弹窗直接到终态，不等动画 | FeedbackTests 2 项 |
| 阴影分层 | `Antd.Effect.Elevated`（弹窗/抽屉）、冻结的 Toast 阴影、浮层 16px 阴影槽 + 定位补偿 | FeedbackTests 2 项 |
| 图标全量 | `@ant-design/icons-svg` 全部 446 个 outlined 图标，生成改为懒加载属性 + 缓存（原 57 个静态字段会在类型初始化时全部解析） | SmokeTests 5 项 |

### 收尾补丁（同一天）

上面四项交付后，又按「能不能真的用键盘走完」补了一轮：

| 项 | 做法 |
|---|---|
| 焦点可视化 | 新增共享 `Antd.FocusVisual`（adorner 焦点环），17 个原本 `FocusVisualStyle=null` 的样式全部启用；Input/Button 保留模板内焦点环 |
| 键盘操作 | Collapse 头部可聚焦、Enter/Space 展开；Menu 节点方向键移动、Enter/Space 激活；Rate 方向键/Home/End 调值；Upload 拖拽区可聚焦并用 Enter/Space 打开 |
| 文案 | `AntdStrings.Ok/Cancel`；Modal、Popconfirm 的按钮文案改为「宿主没覆盖时跟随 locale」 |
| 动效 | `AntdMotion.Configure(popup)` 应用于 13 处 PART_Popup，减动效时不再淡入；Carousel 自动播放在减动效时停止 |

已知取舍：Spin 的旋转保留（属于「必要反馈」）；日期格式仍跟随 `CultureInfo`，与 `AntdLocale` 是两套机制。
文档站不在 M5 内，单列为 M6（见路线图）。
## M6 发布物

| 产物 | 位置 | 说明 |
|---|---|---|
| NuGet 包 | `artifacts/packages/*.nupkg` | Tokens / Theme / Icons / Controls，各含 net48 + net8-windows 与 snupkg；包内含 README |
| 组件文档站 | `docs/site/index.html` | 单文件、零外部依赖；73 个组件的属性表由 `tools/docs-gen` 反射程序集生成，不会与代码漂移 |
| 迁移指南 | `docs/migration.md` | antd / AntDesign.WPF / 0.x 三条迁移路线 + 常见坑 |

**发布记录**：2026-11-20 10:05–10:06 推送，10:19 全部索引完成（首次发布走了约 14 分钟校验）。
线上验证方式：清空本地包缓存后，用 `-p:RestoreSources=https://api.nuget.org/v3/index.json`
让 `tools/package-check` 只从 nuget.org 还原，输出 `VERIFY: PASS`。
**发布前仍需**：连续 4 周无 P0/P1；给 nuget.org 一个 API Key 后 `dotnet nuget push`。许可证为 MIT：仓库根 `LICENSE` + 包内 `license type="expression"`；`tools/package-check` 会从 `artifacts/packages` 还原并验证包可用。

四个包 ID 已核查：`Antd.Wpf.Tokens` / `.Theme` / `.Icons` / `.Controls` 在 nuget.org 上均未被占用（flatcontainer 返回 404），可直接上传。

打包命令：

```bash
dotnet pack src/Antd.Wpf.Controls -c Release -o artifacts/packages
dotnet pack src/Antd.Wpf.Theme    -c Release -o artifacts/packages
dotnet pack src/Antd.Wpf.Icons    -c Release -o artifacts/packages
dotnet pack src/Antd.Wpf.Tokens   -c Release -o artifacts/packages
```
## 能力完成度（别把组件数量当成能力齐全）

上面的 69/71 是**基础版本**：属性、外观与常用交互可用。按「完整能力」口径，只有
Button / Input（单行）/ Tag / Divider / Alert / Result / Popconfirm 等少数几个算对齐了 antd。
接入业务前请先看下面的清单，不要按组件名假设能力：

**1.1.0 已补齐**：Select 的多选/标签折叠/本地与远程搜索/虚拟化、DatePicker 的双月区间、
Table 的固定列与展开行、Form 的异步校验。下面是**仍然缺**的部分：

| 业务常见需求 | 涉及组件 | 现状 |
|---|---|---|
| 上传队列、进度、取消 | Upload | 只收集文件，不做网络传输 |
| 树异步加载、拖拽排序 | Tree / TreeSelect | 静态数据 |
| 输入框 addon、多行、字数统计 | Input | 单行 |

统计口径：**基础版本 69 / 71；完整能力的逐项校准尚未开始**，上表每关掉一行才加一。

## 能力边界（做了但与 antd 有差距）

| 组件 | 现状 | antd 完整能力 |
|---|---|---|
| Select | 单选/多选、标签折叠、本地/远程搜索、虚拟化 | 分组、搜索节流 |
| Table | DataGrid 适配：虚拟化、排序、选择、列宽、固定列、展开行 | 树形行、可编辑、分页器 |
| DatePicker | 单日期、区间（双月） | 禁用日期、周/月/季/年 |
| Slider | 单值 | 双值、刻度、竖向、带输入框 |
| Form | 同步 + 异步校验 | 依赖联动、INotifyDataErrorInfo |
| Input | 单行 | addon、password、textarea、字数统计 |
| Upload | 选择/拖放/移除，不做网络传输 | 上传队列、进度、取消、多文件并发 |
| ColorPicker | 色板 + 色相条 + 预设 | 取色器、透明度、历史色 |
| Tabs | 线条型、单行换行、2px 指示条 | 卡片型、可编辑、可关闭、溢出折叠菜单 |
| Collapse | 手风琴/独立展开、首尾圆角 | 幽灵模式、可折叠图标自定义、嵌套动效 |
| List | 分隔行、ItemTemplate | 分页、加载更多、栅格布局、虚拟滚动 |
| Tooltip | 悬停气泡、上下左右四个方向 | 12 个方向、箭头、富内容、延迟配置 |
| Tabs | 线条型、换行、指示条 | 卡片型、可编辑、可关闭、溢出折叠 |
| Carousel | 单张 + 圆点 + 自动播放 | 滑动/淡入动画、手势、纵向 |
| Image | 预览浮层 | 旋转、缩放、工具栏、多图切换 |
| Menu | 内联展开 / 顶部浮层子菜单 | 折叠态、mega menu、分组与分割线 |
| Tour | 聚光灯 + 卡片 | 12 个方向自适应、滚动跟随、箭头 |
| QRCode | 字节模式、版本 1-10、四档纠错 | 汉字/数字模式、Logo、版本 11-40 |
| Pagination | 页码省略、上一页/下一页 | 每页条数切换、快速跳转、简单模式 |
| Anchor | 滚动高亮、点击跳转 | 横向模式、嵌套、固定偏移 |
| Steps | 横/纵、状态机 | 点状、错误态重试、自定义图标 |
| Timeline | 圆点 + 竖线、自定义颜色 | 左右交替、自定义节点内容 |
| Masonry | 最短列填充 | 动画重排、响应式列数 |
| Popover | 标题 + 内容、点击触发 | 12 个方向、受控/非受控混合、箭头 |
| Dropdown | 菜单项 + ItemInvoked | 子菜单、分组、分割线、多选、右键触发 |
| Modal | 遮罩 + 焦点陷阱 + Esc + 入退场动效 + 减动效 | 命令式静态方法（`Modal.confirm`）、拖拽标题栏、可调 `top` |
| Drawer | 四方向滑入 + 尺寸 + 页脚 + 退场动效 | 嵌套抽屉、推挤父内容 |
| Message | 顶部堆叠、最多 5 条、定时关闭、key 合并未做 | 可更新同 key 的内容、暂停 hover 计时、自定义图标 |
| Notification | 四角放置 + 标题/描述 + 关闭 | hover 暂停倒计时、进度条、按钮区 |
| Popconfirm | 标题 + 描述 + 确定/取消 + 22px 图标槽 | 箭头、12 方向自适应、异步确认 loading |
| Alert | 4 类型 × 单行/带描述/横幅/可关闭/操作槽 | 跑马灯、自定义图标、无边框变体 |
| Result | 5 状态 + 404/500 数字 + 操作区 | antd 的 SVG 插画（当前用图标/数字代替） |

## 参照实测值（tools/visual-ref）

| 部件 | 官方值 |
|---|---|
| Tabs 标签条 | 高 46（12 + 22 + 12），标签间距 32，指示条 2 |
| Card 头部 | 高 56、内边距 0 24、标题 16/600、下边线 colorSplit |
| Card 主体 | 内边距 24、圆角 8、边框 colorBorderSecondary |
| Collapse 表头 | 高 46、内边距 12 16、底色 colorFillAlter |
| List 行 | 内边距 8 16（small）、标题 14/600、描述 colorTextDescription |
| Alert 单行 | 高 40、内边距 8 12、圆角 8、1px 边框、图标 14 + 间距 8 |
| Alert 带描述 | 高 97、内边距 20 24、标题 16/25 且下边距 8、图标 24 + 间距 12 |
| Alert 横幅 | 高 38、内边距 8 12、圆角 0、无边框 |
| Result | 内边距 48 32、图标 72、标题 24/32 常规字重、副标题间距 8、操作区间距 24 |
| Modal | 宽 520、容器内边距 20 24、圆角 8、标题 16/600、标题下 8、页脚上 12、关闭区 32×32 |
| Drawer | 宽 378、头部 57（16 24 + 1px 线）、内容内边距 24、页脚 49（8 16 + 1px 线） |
| Message | 高 40、内边距 9 12、圆角 8、图标 16 + 间距 8 |
| Notification | 宽 384、内边距 20 24、圆角 8、标题 16/24、描述 14/22 间距 8、图标 24 + 间距 12 |
| Popconfirm | 内边距 12、圆角 8、标题 14/600、描述间距 4、按钮行间距 8（24 高小按钮）、文案左缩进 22 |

## DoD 未验证项

- M1：「截图基准差异 < 0.5%」「主题切换 < 120ms」没有度量。
- M2 遗留项已在 1.1.0 补齐：DatePicker 区间、Form 异步校验、Select 远程搜索与虚拟化。
- M4：「Modal 焦点陷阱与 Esc 通过 UIA 测试」只做了进程内断言（TabNavigation=Cycle、Esc 关窗），
  没有 FlaUI / UIA 用例；「Message 队列合并」未做，因为当前 API 没有 key。
- 全部：无自动化无障碍测试，无 UI 自动化测试。

## 测试

- `tests/Antd.Wpf.TokenParity`：536 token × 10 配置 × 双 TFM 逐值对拍。
- `tests/Antd.Wpf.SmokeTests`：264 项断言（控件行为 + 视觉规格 + 架构边界），双 TFM 通过，
  附带渲染截图与 `--metrics` 尺寸导出。
- `tests/Antd.Wpf.FeedbackTests`：72 项断言（M4 反馈层：配色、尺寸、弹层锚定、焦点陷阱、
  Esc/遮罩关闭、toast 队列与自动关闭），双 TFM 通过，`--shot` 输出 `m4-*.png`。
- `tools/visual-ref`：真实 antd 在 Chrome 中的尺寸基线，用于视觉对拍。
