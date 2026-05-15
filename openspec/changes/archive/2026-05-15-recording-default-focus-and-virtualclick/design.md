## Context

`ClickElement` 活动本身有两个默认值：`Focus = false`、`VirtualClick = true`。录制时 `OnUserAction` 只传递了 `VirtualClick`（从配置读取）和 `AnimateMouse`，未传 `Focus`，导致使用活动类的默认值 `false`。

## Goals / Non-Goals

**Goals:**
- 录制点击时 `ClickElement.Focus` 默认设为 `true`
- 录制点击时 `ClickElement.VirtualClick` 默认设为 `false`

**Non-Goals:**
- 不改变 `ClickElement` 活动类本身的默认值（影响拖拽创建的活动）
- 不改变设置面板中的 `use_virtual_click` 配置项

## Decisions

### 在 `OnUserAction` 中显式设值

不修改 `ClickElement` 类默认值（那会影响手拖活动），只在录制路径中显式传参。

```csharp
Focus = true,
VirtualClick = false,
```

Rationale: 录制场景和手拖场景默认值需求不同。录制时 Focus 有助于回放可靠性。

## Risks / Trade-offs

- [Risk] 某些不支持 Focus 的元素（如纯图形区域）可能因 Focus 调用产生额外延迟 → Mitigation: 低风险，UIA Focus 调用轻量级
