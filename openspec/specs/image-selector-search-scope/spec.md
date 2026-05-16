# image-selector-search-scope Specification

## Purpose
TBD - created by archiving change replace-image-processname-with-selector. Update Purpose after archive.
## Requirements
### Requirement: GetElement exposes Selector property

`GetElement` 活动 SHALL 暴露 `Selector` 属性（`InArgument<string>`），存储 JSON 格式的 Selector 字符串。该属性 SHALL 在设计器中通过 "Open Selector" 按钮触发 `SelectorWindow` 弹框进行选择。

#### Scenario: Selector property renders as text with selector button

- **WHEN** 用户在属性面板中查看 `Selector` 属性
- **THEN** 该属性显示为可编辑文本框，且活动设计器提供 "Open Selector" 按钮以弹出选择器

#### Scenario: Selector button opens SelectorWindow

- **WHEN** 用户点击 "Open Selector" 按钮
- **THEN** 系统打开标准 `SelectorWindow` 弹框，用户可鼠标悬停选择目标窗口

### Requirement: findMatch uses Selector-resolved rectangle as search area

当 `Selector` 不为空时，`ImageEvent.findMatch` SHALL 使用 Selector 解析出的窗口 `Rectangle` 作为图像搜索截图的区域。系统 MUST 不再枚举 `MyEnumWindows.WindowRects`。

#### Scenario: Selector resolves to a window rectangle

- **WHEN** `Selector` 非空且解析成功得到目标窗口
- **THEN** 系统仅在该窗口矩形区域内截图并搜索模板图像

#### Scenario: No Selector falls back to full-screen search

- **WHEN** `Selector` 为空或 null
- **THEN** 系统进行全屏截图搜索（行为与不设搜索范围一致）

#### Scenario: Selector resolves but window not found

- **WHEN** `Selector` 解析成功但在当前桌面找不到对应窗口
- **THEN** 系统 SHALL 返回空结果

### Requirement: Processname and Limit are removed

`GetElement` 活动 MUST 移除 `Processname` 和 `Limit` 属性。设计器中对应的 "Process限制" 和 "Clear Process Limit" 按钮 MUST 移除。

#### Scenario: Old workflows with Processname do not crash

- **WHEN** 加载包含已废弃 `Processname` 属性的旧工作流
- **THEN** 系统 SHALL 忽略该属性值，不抛出异常

### Requirement: Highlight button respects Selector

设计器的 "高亮" 按钮 SHALL 将当前 `Selector` 传入 `ImageEvent.waitFor`，搜索区域限定在 Selector 解析的窗口矩形内。

#### Scenario: Highlight with Selector searches within window

- **WHEN** `Selector` 已设定且用户点击高亮按钮
- **THEN** 系统仅在 Selector 指定的窗口矩形内进行图像匹配并高亮结果

