必须修复的问题
1. InputNumber 的范围约束没有覆盖外部赋值。
   [InputNumber.cs (line 131)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/src/Antd.Wpf.Controls/InputNumber.cs:131) 中，输入文字和步进会调用 Clamp，但设置 Value、修改上下限时只同步文字。比如 Maximum=20、绑定值为 100，控件仍可保留 100。最小修复是在依赖属性层统一约束，并处理上下限变化；否则界面声明的范围不能保护业务数据。
2. Form 暴露了横向布局接口，但没有实现对应行为。
   [Form.cs (line 32)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/src/Antd.Wpf.Controls/Form.cs:32) 定义了 Layout 和 LabelWidth，实际布局代码未使用它们，[表单模板 (line 1249)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/src/Antd.Wpf.Controls/Themes/Generic.xaml:1249)也固定纵向排列。设置 Layout=Horizontal 不会得到预期效果。应补齐布局或撤下未实现接口，否则调用者只能绕过控件自行排版。
3. Upload 的文件对话框筛选与自身约定不一致。
   [Upload.cs (line 210)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/src/Antd.Wpf.Controls/Upload.cs:210) 将文档约定的 .png;.jpg 直接放进 Windows 筛选串，而对话框需要 *.png;*.jpg。这会使文件选择列表无法按预期展示。应统一扩展名解析，再分别用于对话框筛选和拖放校验。
4. 紧凑主题在明暗切换时会丢失。
   [ThemeManager.cs (line 85)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/src/Antd.Wpf.Theme/ThemeManager.cs:85) 的 ApplyLight、`ApplyDark` 直接覆盖整个算法值。从 Dark | Compact 切回亮色后，`Compact` 也被清除。最小修复是只修改 Dark 标志；否则切换颜色模式还会意外改变控件尺寸。
应该优先补齐的问题
5. Token 对拍存在“缺失也通过”的漏洞。
   [TokenParity/Program.cs (line 48)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/tests/Antd.Wpf.TokenParity/Program.cs:48) 遇到缺失 Token 时只记录并跳过，最终退出码仅取决于数值不匹配数量。因此，删除一个 Token 的实现仍可能得到 RESULT: PASS。应将缺失项计入失败；如果允许部分缺失，就明确列出允许清单。否则这条核心验证无法保证完整性。
6. 组件数量不能代表业务能力完成度。
   [进度文档 (line 23)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/docs/progress.md:23) 已明确：Select 缺搜索和多选，DatePicker 只有单日期，Form 缺异步校验，Upload 不负责网络传输。这些范围限制本身可以接受，但应区分“基础版本已实现”和“完整能力已完成”。否则业务接入后，会在常见需求上继续开发替代实现。
7. 文档存在多个互相冲突的状态。
   [README (line 5)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/README.md:5) 写的是 50/71，[progress.md (line 19)](/C:/Users/13599/Desktop/RDY/Antd_WPF_DS/docs/progress.md:19) 写的是 62/71，后者还同时出现 92 和 66 个公开控件类型；工作区已经有反馈层实现，文档却仍写未开始。应以一份能力表作为状态来源，避免使用者依据过期信息选型。
我建议下一阶段先暂停扩充组件数量：修复上述行为问题，收紧验证门禁，再选一个真实业务页面完成接入。这样才能验证绑定、校验、焦点、主题切换和复杂数据是否共同工作。面向广泛发布前，再补齐无障碍、国际化、DPI 和长时间运行验证。

---

## 处理结论（2026-11-05）

七条全部复核成立，逐条修复并补了断言（`tests/Antd.Wpf.SmokeTests` 的 `CheckReviewFixes()`）：

| # | 问题 | 处理 | 断言 |
|---|---|---|---|
| 1 | InputNumber 外部赋值绕过范围 | `OnValueChanged` 统一走 `CoerceToLimits()`，Value / Minimum / Maximum 任一变化都收敛；上下限写反时按区间处理而不是返回区间外的值 | 6 条 |
| 2 | Form 的 Layout / LabelWidth 没有行为 | 两个依赖属性改由 `FormItem` 声明并带 `Inherits`，Form 用 `AddOwner` 复用；模板换成行列网格，代码按布局摆放（横向 120px 标签列、标签上移 5px 对齐 32px 控件中线） | 7 条 |
| 3 | Upload 筛选串缺 `*` | 新增 `ParseAccept` 统一解析（`.png` / `png` / `*.png` / MIME 通配），`BuildFilter` 与 `Accepts` 共用同一份解析 | 6 条 |
| 4 | 明暗切换丢掉 Compact | `ApplyLight` / `ApplyDark` 改为只改 Dark 位（内部 `SetDark`），Compact 保留 | 3 条 |
| 5 | 对拍「缺失也通过」 | 缺失计入失败；允许清单默认空，并在注释里写明只有 antd 确有而本移植无法派生时才可加入 | TokenParity 仍 536/536 × 10 组 PASS |
| 6 | 组件数量当作能力完成度 | `progress.md` 新增「能力完成度」一节：基础版本 69/71，完整能力另行校准，并列出业务最常踩的 7 个缺口 | 文档 |
| 7 | 文档状态互相冲突 | README 的 50/71、92 个控件类型更正为 69/71、97 个；M4 状态与实现一致；README 明确以 `progress.md` 为唯一状态来源 | 文档 |

断言：SmokeTests 233 → **255**，FeedbackTests 62，TokenParity 536 × 10 组；双 TFM 全过。
