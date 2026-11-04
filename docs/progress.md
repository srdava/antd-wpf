# 进度状态

最后更新：2026-11-04

对照 `docs/antd-wpf-implementation-plan.md` 的 71 项组件清单。

## 里程碑

| 里程碑 | 计划项 | 已完成 | 说明 |
|---|---|---|---|
| M0 可行性 | 骨架 + 主题引擎 + 2 控件 | ✅ | DoD 全过：token 对拍、双 TFM、架构边界测试 |
| M1 原子层 | 22 | **22** | 清单全部落地 |
| M2 录入层 | 19 | **19** | 清单全部落地（能力边界见下） |
| M3 展示层 | 21 | **21** | 清单全部落地（能力边界见下） |
| M4 反馈层 | 7 | 0 | 未开始 |
| M5 打磨 | — | 0 | 无障碍、i18n、动效降级均未做 |

**组件进度：62 / 71（约 87%）。** 公开控件类型 92 个（Typography 拆 4、Layout 拆 5、Row/Col 拆 2、
Tabs/Collapse/List/Steps/Timeline/Descriptions 各拆出 item 类型，另有若干数据模型类）。 公开控件类型 66 个（Typography 拆 4、Layout 拆 5、Row/Col 拆 2、
Tabs 拆 2、Collapse 拆 2、List 拆 2，以及若干数据模型类）。

## 能力边界（做了但与 antd 有差距）

| 组件 | 现状 | antd 完整能力 |
|---|---|---|
| Select | 单选、无搜索、无虚拟化 | 多选、搜索、标签折叠、万项虚拟化 |
| Table | DataGrid 适配：虚拟化、排序、选择、列宽 | 固定列、树形行、展开行、可编辑、分页器 |
| DatePicker | 单日期 | 区间、禁用日期、周/月/季/年 |
| Slider | 单值 | 双值、刻度、竖向、带输入框 |
| Form | 同步校验 | 异步校验、依赖联动、INotifyDataErrorInfo |
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

## 参照实测值（tools/visual-ref）

| 部件 | 官方值 |
|---|---|
| Tabs 标签条 | 高 46（12 + 22 + 12），标签间距 32，指示条 2 |
| Card 头部 | 高 56、内边距 0 24、标题 16/600、下边线 colorSplit |
| Card 主体 | 内边距 24、圆角 8、边框 colorBorderSecondary |
| Collapse 表头 | 高 46、内边距 12 16、底色 colorFillAlter |
| List 行 | 内边距 8 16（small）、标题 14/600、描述 colorTextDescription |

## DoD 未验证项

- M1：「截图基准差异 < 0.5%」「主题切换 < 120ms」没有度量。
- M2：「Form 异步校验」「Select 万项虚拟化」「DatePicker 区间」未达标。
- 全部：无自动化无障碍测试，无 UI 自动化测试。

## 测试

- `tests/Antd.Wpf.TokenParity`：536 token × 10 配置 × 双 TFM 逐值对拍。
- `tests/Antd.Wpf.SmokeTests`：168 项断言（控件行为 + 视觉规格 + 架构边界），双 TFM 通过，
  附带渲染截图与 `--metrics` 尺寸导出。
- `tools/visual-ref`：真实 antd 在 Chrome 中的尺寸基线，用于视觉对拍。
