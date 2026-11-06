# 视觉对齐（visual parity）

Token 逐值相等 **不等于** 看起来像 antd。M0/M1 的对拍只验证了「派生出来的数值对不对」，没有验证
「控件用这些数值画出来的东西像不像」。这是两个不同的问题，之前只做了前者，所以 UI 与官方差距明显。

本文记录第二套验证：把真实 antd 渲染出来，量它的尺寸，再量我们的，逐项比对。

## 1. 参照物怎么来的

`tools/visual-ref/` 是一个真实 antd 页面，不是截图拼的：

- `vendor/` 里放 `antd.min.js`（6.6.8 UMD）、`react.js` / `react-dom.js`（18.3.1 UMD）、`dayjs`。
  antd 6 的 UMD 包仍然可用，但 React 19 已不再发 UMD，所以参照页钉在 React 18。
- `index.html` 用与 Gallery 相同的卡片结构和同样的组件组合渲染一遍，元素上打 `data-m` 标记。
- 页面加载后把每个标记元素的 `getBoundingClientRect()` 与 `getComputedStyle()` 写进 `<pre id="metrics">`，
  用 `chrome --dump-dom` 取出来，得到一份**官方尺寸基线**（`out/metrics.json`）。
- 截图用 `--force-device-scale-factor=1.25`，与本机 125% 缩放对齐，否则 1px 线条会被压成 0.8px。

本机 Chrome 无法从 `file://` 加载外部脚本，所以用 `serve.js` 起 8787 端口再截图。

我们的尺寸由 `tests/Antd.Wpf.SmokeTests --metrics <path>` 输出（`MetricsProbe.cs`），格式与官方一致，
用 `tools/visual-ref/compare.py` 逐字段 diff。

```bash
# 官方基线
cd tools/visual-ref && node serve.js &
chrome --headless=new --dump-dom http://127.0.0.1:8787/index.html > out/dom.html
# 我们的尺寸
dotnet run --project tests/Antd.Wpf.SmokeTests -f net8.0-windows -- --metrics out/ours.json
python compare.py out/metrics.json out/ours.json
```

## 2. 官方实测值（antd 6.6.8，单位 CSS px）

| 组件 | 关键值 |
|---|---|
| Button 默认 | 32 高，`padding: 0 15`，圆角 6，字号 14，**text/link 也带 1px 透明边框** |
| Button large / small | 40 高 / 字号 16 / 圆角 8；24 高 / `padding: 0 7` / 圆角 4（字号仍是 14） |
| Input | 32 高，`padding: 4 11`，圆角 6，行高 22 |
| Divider 带文字 | 高 25，**字号 16，字重 500**；块级外边距 24；线条 `colorSplit` |
| Tag | 22 高，`padding: 0 7`，圆角 4，字号 12，预设色背景填充、边框透明 |
| Avatar 32 | 字号 14，背景 `rgba(0,0,0,.25)`，白字 |
| Progress 线形 | 行高 14，条高 8，百分比字号 14，文字在条右侧 8px |
| Segmented | 32 高，轨道内边距 2，圆角 6，字号 14 |
| Statistic 数值 | 24 号，**常规字重** |

## 3. 修掉的问题

按影响排序，都是量出来的，不是猜的：

1. **字体栈**。`ThemeSnapshot.BuildFontFamily` 把 `Microsoft YaHei UI` 硬塞到第一位，而 antd 在 Windows
   上解析到 `Segoe UI`。整个界面因此偏宽、偏重。改为 Segoe UI 优先、YaHei 作为 CJK 兜底。
2. **`TextFormattingMode="Display"`**。整像素 hinting 让小字号变宽约 3%。改为 `Ideal` 后段落宽度与
   浏览器差 2px 以内。
3. **Divider 文字规格**。原来用 14/600，官方是 16/500，高度只有 17.8（官方 25）。
4. **Divider 线条消失**。截图按 96 DPI 渲染 120 DPI 的布局，1px 线被压成 0.8px 后洗白。这既是截图
   管线的问题（已改为按真实 DPI 输出），也提醒：**截图不能作为判断依据时，要打印几何值**。
5. **Badge 布局**。原来 Badge 在 StackPanel 里被拉满整行（宽 814），计数飘到文字左侧并压住文案。
   改为内容自适应宽度 + Canvas 叠加 + 按自身尺寸 50% 偏移，与 antd 的 `position:absolute` 等价。
6. **Progress 百分比被裁**。`OnRender` 里写 `SetCurrentValue(FontSizeProperty, …)`，在渲染阶段触发
   重新布局，控件被撑得比容器宽，百分比画到卡片外。改为在主题回调里设置。同时页面根节点从
   `MaxWidth + HorizontalAlignment=Left` 改为居中的 `Border`，避免子控件撑宽整列。
7. **Tag 尺寸**。原来高度跟着文字走（17.9），且被拉满整行。改为 22 高、内容自适应、7px 内边距。
8. **Segmented**。字号 12→14，项内边距 12→11，轨道内边距绑定到控件。
9. **Statistic** 数值由 SemiBold 改 Normal；**Avatar** 字号按尺寸取 12/14/16，背景改 `colorTextQuaternary`。
10. **危险按钮**。默认变体的描边与文字应为 `colorError`，原来用的是 `colorBorder`/`colorText`。
11. **加载指示器**。原来是 1/4 圆弧，官方是四点环形；Button 与 Spin 都改掉了。
12. **清除按钮**。官方是灰色实心圆 + 白色叉，我们原来是裸叉。

13. **输入框文字被裁**（接入真实页面时才发现）。`TextBox.Padding` 会被 WPF 复制到模板里的
    `PART_ContentHost`（`ScrollViewer.Padding` 也变成 11,4,11,4），而我们又在边框上吃了一次，
    22px 的文字区只剩 14.4px。实测：`input h=32 / scroll h=22.4 / scroll padding=11,4,11,4`。
    修法：边框不再绑 `Padding`，内容宿主高度改为"控件高 − 上下边框"（30），前缀/后缀/占位符
    在代码里补内缩。`InputNumber` 同样处理，并同步收窄上下箭头。
## 4. 仍然存在的差异（已知，不打算假装没有）

| 项 | 差距 | 原因 |
|---|---|---|
| 文本宽度 | 按钮字宽比官方窄 1–2px（约 2%） | WPF 与 DirectWrite 的字距取整不同，无 API 对齐 |
| Badge 自身高度 | 18.6 vs 14 | antd 用 `line-height:1`；WPF 没有可继承的 `TextElement.LineHeight` |
| Empty 插图 | 用的是 inbox 图标 | 官方是专属 SVG，尚未移植 |
| 动画细节 | 四点同时旋转，官方是分时透明度 | 需要逐点 Storyboard，成本高于收益 |
| 阴影 | Modal / Drawer / Toast 已加单层 `DropShadowEffect`（Blur 18 / Depth 4 / 12% 黑）；下拉类浮层没有 | WPF 只有一个模糊，antd 叠三层；且 `Popup` 尺寸跟着内容走，超出内容的阴影会被裁掉，需要预留阴影边距 |

## 5. 回归门禁

`CheckVisualParity()` 把上表的数字写成断言（按钮 32/15、大号 40/8、Tag 22/7、Divider 16/500、
Progress 不得超出容器等）。token 全等但视觉漂移时，这里会先失败。

## 6. M4 反馈层的实测基线

参照页新增了 `feedback` 卡片（四条 Alert + 一个 Result）以及三个常开的浮层（Modal、Drawer、
Popconfirm），消息与通知用命令式 API 拉起后固定不倒计时。测法与前面相同：`data-m` 标记 +
类名查找 + `chrome --dump-dom`。

antd 6 把弹层改写成了嵌套结构，选择器也随之变化，记在这里免得下次再猜：

| 部件 | antd 6.6.8 实际类名 |
|---|---|
| 弹窗容器 | `.ant-modal`（外层，宽度写在 style）/ `.ant-modal-container`（真正带内边距与圆角的盒子） |
| 抽屉 | `.ant-drawer-header` / `.ant-drawer-body` / `.ant-drawer-footer` |
| 消息 | `.ant-message-notice`（一条）、`.ant-message-notice-icon` |
| 通知 | `.ant-notification-notice`、`.ant-notification-notice-title` |
| 气泡卡片 | `.ant-popover-container`（v5 的 `.ant-popover-inner` 已不在 DOM 里） |

量出来的值（单位 CSS px）：

| 部件 | 关键值 |
|---|---|
| Alert 单行 | 40 高，8/12 内边距，8 圆角，1px 边框，图标 14 间距 8，文字 14/22 |
| Alert 带描述 | 97 高，20/24 内边距，标题 16（行高 25）下边距 8，图标 24 间距 12 |
| Alert 横幅 | 38 高，8/12 内边距，圆角 0，无边框 |
| Result | 48/32 内边距，72 图标，标题 24/32 **常规字重**，副标题 8，操作区 24 |
| Modal | 520 宽，20/24 内边距，8 圆角，标题 16/600，标题下 8，页脚上 12，关闭按钮 32×32 |
| Drawer | 378 宽，头部 57，内容 24，页脚 49（8/16 + 1px 上边线） |
| Message | 40 高，9/12 内边距，8 圆角，图标 16 间距 8 |
| Notification | 384 宽，20/24 内边距，8 圆角，标题 16/24，描述 8 间距，图标 24 间距 12 |
| Popconfirm | 12 内边距，8 圆角，标题 14/600，描述间距 4，按钮行上 8（24 高），文案左缩进 22 |

两个容易踩的点：

1. **动画没关就别量**。`--virtual-time-budget` 下 CSS 动画不会推进，Modal 停在 `scale(0.2)`，
   `.ant-modal` 量出来只有 104×29。给浮层套 `ConfigProvider theme={{token:{motion:false}}}` 才是稳定值。
2. **外边距塌陷**。`<p>` 的 margin 会穿过没有 padding 的 `.ant-modal-body`，让 body 的 y 比
   「内边距 + 标题高度」多出 6px。真实间距以 CSS 的 `margin-bottom: 8` / `margin-top: 12` 为准。

## 7. M4 的已知差异

| 项 | 差距 | 原因 |
|---|---|---|
| 阴影 | 弹窗、抽屉、气泡、toast 都没有投影 | 与 Dropdown/Popover 保持一致，留到 M5 统一做阴影分层 |
| 退出动效 | 只有入场（缩放/滑入），关闭是瞬时的 | 需要 Storyboard 完成回调，留到 M5 动效系统 |
| Result 插画 | 404/500 用大号数字，其他状态用图标 | 未移植 antd 的 SVG 插画 |
| 遮罩点击 | 只响应左键按下 | antd 同样如此，此处一致 |
