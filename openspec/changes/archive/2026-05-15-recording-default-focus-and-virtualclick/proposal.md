## Why

录制点击时生成的 `ClickElement` 活动，`Focus` 默认为 `false`、`VirtualClick` 默认为 `true`。这导致回放时可能因为元素未获得焦点而点击失败，且虚拟点击在某些场景下不可靠。需要将录制默认值改为 `Focus=true`、`VirtualClick=false`，提高回放成功率。

## What Changes

- 录制生成的 `ClickElement` 活动显式设置 `Focus = true`
- 录制生成的 `ClickElement` 活动显式设置 `VirtualClick = false`
- `use_virtual_click` 和 `use_animate_mouse` 配置项不变（仅在设置 UI 中控制）

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `recording-capture-options`: 录制点击默认值从 `Focus=false, VirtualClick=true` 改为 `Focus=true, VirtualClick=false`

## Impact

- `OpenRPA.MainWindow.xaml.cs` `OnUserAction` 方法：修改 `ClickElement` 构造参数
