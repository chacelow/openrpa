## Context

`GetElement` 活动目前硬编码使用模板匹配 (`CcoeffNormed`)，但 `Matches.cs` 中已有基于 KAZE 特征检测 + FLANN 匹配的特征点匹配实现，该实现未被任何 Activity 调用。用户需要根据场景选择匹配算法：

- **特征点匹配**：抗缩放、旋转、透视变换，适合 UI 在不同分辨率/缩放下的场景
- **模板匹配**：精确像素对比，适合固定尺寸、无变形的 UI 元素

当前 `OpenRPA.Interfaces` 已有 `RecordingCaptureMode` 枚举作为模式选择先例，`GetElement` 已有 `Threshold`、`CompareGray` 等多参数设计，新增一个匹配模式属性符合现有架构模式。

## Goals / Non-Goals

**Goals:**
- 为 `GetElement` 添加 `MatchMode` 属性，支持 TemplateMatching 和 FeatureMatching 两种模式
- 默认值设为 FeatureMatching
- 特征点匹配结果返回 `Rectangle[]`（与模板匹配一致），便于 Activity 统一处理
- `GetElementDesigner` 的 Highlight 按钮遵循当前 MatchMode 设置
- `ImageEvent.waitFor` 支持 MatchMode 参数，轮询时使用正确算法

**Non-Goals:**
- 不添加其他特征检测器（SIFT、SURF、ORB 等）——本次只暴露已有的 KAZE
- 不修改录制流程 (`Plugin.cs`) 的自动匹配逻辑（录制时始终用模板匹配是合理的）
- 不修改 `GetText`、`GetImage`、`GetColor` 等其他活动

## Decisions

### 1. 枚举位置：定义在 `OpenRPA.Interfaces`

遵循 `RecordingCaptureMode` 的先例，在 `OpenRPA.Interfaces` 命名空间下定义 `ImageMatchMode` 枚举。`OpenRPA.Image` 项目已引用 `OpenRPA.Interfaces`，无需新增依赖。

```csharp
namespace OpenRPA.Interfaces
{
    public enum ImageMatchMode
    {
        FeatureMatching = 0,   // KAZE + FLANN，默认
        TemplateMatching = 1   // CcoeffNormed
    }
}
```

### 2. 特征点匹配结果转换

现有 `Matches.FindMatch` (KAZE 版本) 通过 `out` 参数返回 `homography` 矩阵。需新增一个包装方法：
1. 用 homography 变换模板四角坐标 → 得到在源图中的四边形
2. 取四边形外接矩形作为匹配结果 `Rectangle`
3. 返回 `Rectangle[]`（与模板匹配保持一致）

使用 `CvInvoke.PerspectiveTransform` 将模板四角映射到源图坐标。

### 3. MatchMode 与现有属性的交互

| MatchMode | Threshold 作用 | CompareGray 作用 |
|-----------|---------------|-----------------|
| FeatureMatching | 用作匹配点数阈值（归一化到 0~1），实际匹配点数 / 最小特征点数 | 忽略（KAZE 在彩色/灰度均可工作） |
| TemplateMatching | 现有行为：相关系数阈值 | 现有行为：控制是否转灰度再匹配 |

特性点匹配模式下 `CompareGray` 属性可隐藏或置灰。

### 4. `getBatch` 方法的路由

`GetElement.getBatch()` 是核心匹配方法，根据 `MatchMode` 分发：

```
getBatch() → if FeatureMatching → Matches.FindFeatureMatches(source, template, threshold, maxresults)
           → if TemplateMatching → Matches.FindMatches(source, template, threshold, maxresults, comparegray)
```

## Risks / Trade-offs

- **默认值变更影响现有工作流**: 已有 `GetElement` 的工作流若未显式设置 MatchMode，行为会从模板匹配变为特征点匹配 → Mitigation: 在 release notes 中标明 **BREAKING**，用户需显式设置为 TemplateMatching 恢复旧行为
- **KAZE 性能**: 特征点检测计算量比模板匹配大，每帧约 50-200ms → Mitigation: 轮询间隔保持 100ms 上限，第一次匹配后缓存结果；模板匹配仍可选，高频场景用户可切换
- **特征点匹配对小图效果差**: 模板太小（<50x50px）特征点不足 → Mitigation: 当匹配点数不足时返回空结果，日志提示用户切换模板匹配

## Open Questions

- 是否需要为特征点匹配单独配置 `uniquenessThreshold`（当前硬编码 0.80）？初始版本保持硬编码，后续可加属性。
