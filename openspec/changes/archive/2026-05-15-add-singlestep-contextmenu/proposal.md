## Why

WFDesigner 已有 F10/F11 单步执行键盘快捷键，代码中预留了右键菜单项 `runthis`（运行此活动）但被注释掉。需要启用右键菜单方式触发单步执行。

## What Changes

- 取消注释 `runthis` 菜单项声明、初始化和点击事件绑定
- 实现 `OnRunthis` 方法：设置 `Singlestep = true`，调用 `Run()`
- 移除 `#if DEBUG` 守卫，让 Release 版本也可用
- `runFromHere` 保持不变（`从某步开始执行` 因 WF 引擎限制无法实现）

## Capabilities

### New Capabilities

- `singlestep-contextmenu`: 右键活动菜单中点击"运行此活动"触发单步调试执行

### Modified Capabilities

None.

## Impact

- `OpenRPA/Views/WFDesigner.xaml.cs`：取消注释 + 实现 OnRunthis
