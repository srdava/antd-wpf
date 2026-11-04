# 进度状态

最后更新：2026-11-04

对照 `docs/antd-wpf-implementation-plan.md` 的 71 项组件清单。

## 里程碑

| 里程碑 | 计划项 | 已完成 | 说明 |
|---|---|---|---|
| M0 可行性 | 骨架 + 主题引擎 + 2 控件 | ✅ | DoD 全过：token 对拍、双 TFM、架构边界测试 |
| M1 原子层 | 22 | **22** | 清单全部落地 |
| M2 录入层 | 19 | **19** | 清单全部落地（能力边界见下） |
| M3 展示层 | 21 | **2**（Tree、Table） | 其余 19 项未开始 |
| M4 反馈层 | 7 | 0 | 未开始 |
| M5 打磨 | — | 0 | 无障碍、i18n、动效降级均未做 |

**组件进度：43 / 71（约 61%）。** 公开控件类型 51 个（Typography 拆 4、Layout 拆 5、Row/Col 拆 2、
以及若干数据模型类）。

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

## DoD 未验证项

- M1：「截图基准差异 < 0.5%」「主题切换 < 120ms」没有度量。
- M2：「Form 异步校验」「Select 万项虚拟化」「DatePicker 区间」未达标。
- 全部：无自动化无障碍测试，无 UI 自动化测试。

## 测试

- `tests/Antd.Wpf.TokenParity`：536 token × 10 配置 × 双 TFM 逐值对拍。
- `tests/Antd.Wpf.SmokeTests`：168 项断言（控件行为 + 视觉规格 + 架构边界），双 TFM 通过，
  附带渲染截图与 `--metrics` 尺寸导出。
- `tools/visual-ref`：真实 antd 在 Chrome 中的尺寸基线，用于视觉对拍。
