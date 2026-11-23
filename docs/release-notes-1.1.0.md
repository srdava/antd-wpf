# Antd.Wpf 1.1.0

在 1.0.0 基础上补齐了四类业务缺口，**全部是新增 API，没有破坏性变更**：升级只需改版本号，已有代码无需修改。

## 安装

```bash
dotnet add package Antd.Wpf.Controls --version 1.1.0
```

## 本版新增

### Select：搜索与多选

```xml
<!-- 本地搜索 -->
<antd:Select Filterable="True" ItemsSource="{Binding States}" />

<!-- 多选 + 标签折叠 -->
<antd:Select IsMultiple="True" MaxTagCount="3" />

<!-- 远程搜索：不再本地过滤，查询词交给宿主 -->
<antd:Select Filterable="True" FilterLocally="False" SearchTextChanged="OnSearch" />
```

- `Filterable`：下拉顶部搜索框，按显示文本（或 `TextSearch.TextPath`）过滤，↑↓ 移动、Enter 收起、Esc 清空
- `IsMultiple` / `MaxTagCount` / `SelectedItems`：多选、超出折叠成 `+N`、标签可点 × 移除
- `FilterLocally` + `SearchTextChanged`：远程搜索
- 列表改为 `VirtualizingStackPanel`，万级选项不再一次性实例化
- 下拉面板补上阴影（16px 阴影槽，视觉间距仍是 4px）

### DatePicker：区间与禁用日期

```xml
<antd:DatePicker IsRange="True" RangeStart="{Binding From}" RangeEnd="{Binding To}" />
```

- `IsRange` / `RangeStart` / `RangeEnd`：双月面板，第一次点起点、第二次点终点并收起
- 终点早于起点自动改起点；改 `RangeStart` 超过现有终点会自动清掉终点
- `DisabledDate`（`Func<DateTime,bool>`）：禁用格置灰且键盘也进不去
- 新增 `Clear()`；`Today` 文案接入 `AntdLocale`（原来写死英文）

### Table：树形行与展开行

```xml
<!-- 树形行：指定子集合属性即可 -->
<antd:Table ItemsSource="{Binding Nodes}" TreeChildrenPath="Children" TreeTextPath="Name" />

<!-- 展开行 -->
<antd:Table ShowRowDetails="True" RowDetailsTemplate="{StaticResource Details}" />
```

- `TreeChildrenPath` / `TreeTextPath` / `TreeIndent`：自动插入树列，层级缩进
- `ToggleNode()` / `ExpandAllNodes()` / `CollapseAllNodes()`
- 展开器是**真正的按钮**：可 Tab 聚焦、空格/回车切换、朗读名为"展开/收起"
- 固定列沿用 DataGrid 的 `FrozenColumnCount`（1.0.0 起即可用，本版补了断言与文档）

### Form：异步校验

```csharp
item.AsyncValidator = async v => await api.IsNameTakenAsync((string)v) ? "这个名字已被占用" : null;

if (!await form.ValidateAsync()) return;
```

- 同步规则先跑（必填 / `Validator`），通过后才发异步请求
- `FormItem.IsValidating` 可绑定转圈
- 内部竞态保护：新一次校验会作废旧的 in-flight 结果，快速输入不会留下过期报错

### Modal：命令式弹窗

```csharp
Modal.Confirm("删除确认", "确定要删除这条记录吗？", () => Delete(), () => { });
Modal.Info("提示", "保存成功");
```

- `Confirm / Info / Success / Warning / Error`，可指定 owner window
- 自动挂载到窗口、关闭时自动移除，调用方不需要持有引用
- `ShowCancel`：信息类弹窗只保留 OK

### Upload：队列、并发、进度、取消

```csharp
upload.MaxConcurrent = 3;
upload.Uploader = async (file, progress, token) => await api.UploadAsync(file.Path, progress, token);
```

- `Uploader` / `MaxConcurrent` / `AutoUpload` / `StartUpload()` / `Cancel()` / `CancelAll()`
- `UploadFile` 改为 INotifyPropertyChanged，行内显示 2px 进度条与状态文字
- 移除某一行会自动取消它正在跑的请求
- 传输本身仍由宿主提供：**库不碰网络，只负责队列**

### 其它

- `Calendar.DisabledDate`（DatePicker 的禁用规则靠它）
- 新增文案：`Search` / `Today` / `Uploading` / `UploadDone` / `UploadFailed` / `UploadCancelled` / `Expand` / `Collapse`，中英双语

## 文档

- **新增中英文使用手册**：`docs/manual.zh.md` / `docs/manual.en.md`，74 个组件的用法与全部参数表（参数表由程序集反射生成）
- README 重写为双语（`README.md` / `README.en.md`）
- 组件文档站新增上述属性

## 验证

| 套件 | net8 | net48 |
|---|---|---|
| TokenParity（536 token × 10 组） | PASS | PASS |
| SmokeTests | 349 | 349 |
| FeedbackTests | 76 | 76 |
| Integration 自检 | 28 | 28 |

包可用性：清空缓存后从 nuget.org 或本地包源还原，`net48` 与 `net8.0-windows` 两个消费者工程均 `VERIFY: PASS`。

## 附件校验和（SHA-256）

| 文件 | 大小 | SHA-256 |
|---|---|---|
| Antd.Wpf.Controls.1.1.0.nupkg | 257 KB | `710f0425d289316f067ea32f17d1dd5f6a814fe61bd03e066298b29fc0470dbc` |
| Antd.Wpf.Icons.1.1.0.nupkg | 483 KB | `9af161f6f49aa5f1b53e47a2b541c3a7c48439526dc851dfa8d15621ec29a7ad` |
| Antd.Wpf.Theme.1.1.0.nupkg | 21 KB | `684f4d268d3832be4bec12e591c20cfdb7f366fa6f7fce3913103b6bc657e438` |
| Antd.Wpf.Tokens.1.1.0.nupkg | 25 KB | `e04b389be26dab533f5eb92d32eb023417dbb0b05dc22849be0353cfd87fe0cb` |

## 已知边界（未在本版处理）

Table 可编辑单元格、Form 跨字段联动、Select 分组、DatePicker 周/月/季/年视图、RTL、Windows 高对比度、twotone 双色渲染、DatePicker/TimePicker/Cascader 下拉面板阴影。详见 `docs/progress.md`。

## 许可证

MIT。
