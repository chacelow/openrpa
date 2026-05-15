## Context

WFDesigner 中 `runthis` / `runFromHere` 菜单项已被声明但注释掉，隐藏在 `#if DEBUG` 守卫后。`Singlestep` 模式已通过 F10/F11 键盘快捷键实现并可用。`OnVisualTracking` 检测 `Singlestep` 标志后暂停执行等待用户继续。

## Goals / Non-Goals

**Goals:**
- 右键活动 → "运行此活动" → `Singlestep = true` → `Run()`
- 移除 `#if DEBUG` 限制

**Non-Goals:**
- 不实现 "从某步开始执行"（WF 引擎限制）

## Decisions

### 复用现有 Singlestep 机制

直接设置 `Singlestep = true` 后调用 `Run()`，无需新逻辑。

### 不实现 runFromHere

`从某步开始执行` 需要跳过前面活动的执行，WF 引擎不支持。该菜单项保持注释状态。

## Risks / Trade-offs

- 无风险。只启用已有代码路径。
