## Why

`GetElement`（Image）目前用 `Processname` + `Limit` 来限定图像搜索范围，底层通过 `MyEnumWindows.WindowRects` 枚举进程的 1960 个窗口句柄再过滤去重。这个机制有两大致命问题：1）`WindowRects` 去重逻辑有 bug，会错误删除主窗口矩形导致搜索区域为 0；2）UI 交互体验差——用户需要手动填进程名、点两个按钮、在蓝色遮罩上框选，整个流程割裂。OpenRPA 已有成熟的 Selector 选择器机制（ClickElement、Windows GetElement 等都在用），可以直接复用：弹框→选中窗口→自动捕获进程名和窗口矩形（Rectangle），一步到位。

## What Changes

- **新增** `Selector` 属性（`InArgument<string>`），与 Windows GetElement 一致
- **新增** "Open Selector" 按钮，复用 `SelectorWindow` 弹框选择目标窗口
- **移除** `Processname` 属性 — **BREAKING**
- **移除** `Limit` 属性 — **BREAKING**
- **移除** "Process限制" / "Clear Process Limit" 按钮
- `findMatch` 中用 Selector 解析出的窗口 `Rectangle` 替代 `WindowRects` 枚举
- 保留无 Selector 时的全屏搜索行为（向后兼容）

## Capabilities

### New Capabilities
- `image-selector-search-scope`: 图像识别活动通过 Selector 机制限定搜索范围，替代 Processname + Limit

### Modified Capabilities
<!-- No existing specs modified -->

## Impact

- `OpenRPA.Image/Activities/GetElement.cs`: 移除 `Processname`、`Limit`，新增 `Selector`
- `OpenRPA.Image/Activities/GetElementDesigner.xaml` + `.xaml.cs`: 移除 Processname 相关按钮，新增 Selector 按钮
- `OpenRPA.Image/ImageEvent.cs`: `findMatch` 中移除 `WindowRects` 调用，改用 Selector 解析
- **BREAKING**: 现有使用 `Processname` 或 `Limit` 的工作流需迁移到 `Selector`
