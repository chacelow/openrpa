# image-match-mode Specification

## Purpose
TBD - created by archiving change image-matching-algorithm-option. Update Purpose after archive.
## Requirements
### Requirement: GetElement exposes MatchMode property

`GetElement` 活动 SHALL 暴露 `MatchMode` 属性，类型为 `ImageMatchMode` 枚举，默认值 MUST 为 `FeatureMatching`。该属性 SHALL 在活动设计器的属性面板中以下拉列表形式展示。

#### Scenario: Default is FeatureMatching

- **WHEN** 用户新拖入一个 `GetElement` 活动
- **THEN** 其 `MatchMode` 属性默认值为 `FeatureMatching`

#### Scenario: User selects TemplateMatching in property panel

- **WHEN** 用户在属性面板中将 `MatchMode` 改为 `TemplateMatching`
- **THEN** 活动执行时使用模板匹配算法（CcoeffNormed）

#### Scenario: Property panel shows dropdown with both modes

- **WHEN** 用户在属性面板中查看 `MatchMode` 属性
- **THEN** 该属性以下拉列表展示，包含 `FeatureMatching` 和 `TemplateMatching` 两个选项

### Requirement: FeatureMatching mode uses KAZE detector

当 `MatchMode` 为 `FeatureMatching` 时，系统 SHALL 使用 KAZE 特征检测器和 FLANN 匹配器在源图像中定位模板图像。匹配结果 SHALL 通过单应性矩阵变换模板四角坐标得到外接矩形。

#### Scenario: Feature matching finds a matching region

- **WHEN** `MatchMode` 为 `FeatureMatching`，模板图像在源图像中有对应的缩放/旋转版本
- **THEN** 系统 SHALL 返回一个 `Rectangle` 表示模板在源图中的外接矩形位置

#### Scenario: Feature matching with insufficient keypoints

- **WHEN** `MatchMode` 为 `FeatureMatching`，但匹配的有效点少于 4 个
- **THEN** 系统 SHALL 返回空结果（`Rectangle[]` 长度为 0）

### Requirement: TemplateMatching mode uses CcoeffNormed

当 `MatchMode` 为 `TemplateMatching` 时，系统 SHALL 使用归一化相关系数匹配（CcoeffNormed），行为与变更前一致。`Threshold` 和 `CompareGray` 属性在此模式下 SHALL 继续按现有逻辑生效。

#### Scenario: Template matching with threshold

- **WHEN** `MatchMode` 为 `TemplateMatching`，`Threshold` 为 0.8，`CompareGray` 为 true
- **THEN** 系统使用灰度 CcoeffNormed 匹配，相关系数 ≥0.8 的位置视为匹配

### Requirement: ImageEvent.waitFor respects MatchMode

`ImageEvent.waitFor` 静态方法 SHALL 新增 `MatchMode` 参数。轮询过程中每次检测 SHALL 使用指定的匹配算法。

#### Scenario: waitFor with FeatureMatching finds image after delay

- **WHEN** 调用 `ImageEvent.waitFor` 且 `MatchMode = FeatureMatching`，模板图像在超时前出现在屏幕上
- **THEN** 系统 SHALL 在检测到时立即返回匹配的 `Rectangle[]`，不再等待剩余超时

### Requirement: GetElementDesigner Highlight button respects MatchMode

`GetElementDesigner` 的高亮（Highlight）按钮 SHALL 读取当前 `MatchMode` 设置，并使用对应算法进行匹配测试。

#### Scenario: Highlight uses current MatchMode

- **WHEN** `MatchMode` 为 `FeatureMatching`，用户点击高亮按钮
- **THEN** 系统使用 KAZE 特征点匹配进行检测，并用绿色框标记匹配结果

