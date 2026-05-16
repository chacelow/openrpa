## Why

OpenRPA 的 `GetElement`（获取元素）活动目前硬编码使用模板匹配（CcoeffNormed），但 `Matches.cs` 中早已实现了基于 KAZE + FLANN 的特征点匹配算法，只是从未暴露给用户。用户在录制和回放时需要根据场景选择合适的算法：模板匹配适合固定尺寸的 UI 元素，特征点匹配适合缩放、旋转或透视变化的场景。现在用户无法做这个选择，且默认行为与业界主流工具（如 Power Automate）不一致——后者默认使用特征点匹配。

## What Changes

- 为 `GetElement` 活动新增 `MatchMode` 属性，提供匹配算法选择列表
- 支持两种匹配模式：**特征点匹配**（KAZE + FLANN，默认）和 **模板匹配**（CcoeffNormed）
- `MatchMode` 在属性面板中以下拉列表展示，与 `Threshold`、`Timeout` 等属性并列
- **BREAKING**: 默认算法从模板匹配改为特征点匹配，现有使用 `GetElement` 且依赖模板匹配行为的工作流需要显式设置 `MatchMode = TemplateMatching`

## Capabilities

### New Capabilities
- `image-match-mode`: 为图像识别活动提供匹配算法选择能力，用户可在属性面板中切换特征点匹配或模板匹配

### Modified Capabilities
<!-- No existing specs are modified. This is a new capability. -->

## Impact

- `OpenRPA.Image/Activities/GetElement.cs`: 新增 `MatchMode` 属性
- `OpenRPA.Image/Matches.cs`: 暴露特征点匹配 `FindMatch` 的重载，使其返回 `Rectangle`（当前只返回 `out` 参数，不便于 Activity 调用）
- `OpenRPA.Image/GetElementDesigner.xaml` + `.xaml.cs`: 属性面板需要展示 `MatchMode` 下拉选择
- `OpenRPA.Interfaces`: 可能需要定义 `ImageMatchMode` 枚举（或放在 `OpenRPA.Image` 中）
- 现有工作流兼容性：默认值变更需要文档说明
