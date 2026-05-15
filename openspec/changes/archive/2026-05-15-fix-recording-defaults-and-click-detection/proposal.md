## Why

The previously implemented `add-recording-capture-options` change introduced recording capture mode selection but contains three defects that prevent users from effectively using the recording feature: the capture mode dropdown displays blank instead of showing the current selection, the recorder silently ignores clicks when it cannot build a sufficient UIA selector, and no visual feedback confirms that recording is actively capturing input. Additionally, the start/stop plugin logic is hardcoded to the Windows plugin regardless of the selected capture mode, causing inconsistent recorder behavior. These issues must be fixed to make the recording feature usable.

## What Changes

- Fix the RibbonComboBox capture mode display so the currently selected mode (default: UIA) is always shown in the collapsed dropdown.
- Add user-visible feedback when recording is active (status indicator, overlay, or UI state change) so users can confirm recording has started.
- Add logging and visual notification when a click is recorded but the selector generation fails, so the user understands why an action was not captured instead of silently failing.
- Wire the recording start path to respect the selected capture mode instead of unconditionally starting the Windows (UIA) plugin.
- Ensure the input driver low-level hooks are verified as active before starting recording, and surface an error message if hooks cannot be installed.

## Capabilities

### New Capabilities

- `recording-ux-feedback`: Covers visual/audible confirmation of recording state, click capture success/failure feedback, and hook installation health checks.

### Modified Capabilities

- `recording-capture-options`: The capture mode selection UI now correctly displays the current mode; the recording start path respects mode selection instead of hardcoding the Windows plugin.

## Impact

- `OpenRPA.MainWindow.xaml` and `MainWindow.xaml.cs`: RibbonComboBox capture mode binding fix, recording start/stop UI state management, hook health verification.
- `OpenRPA.Windows.Plugin.cs`: Selector generation failure logging and optional user feedback when a click cannot be captured.
- `OpenRPA.Interfaces.Input.InputDriver.cs`: Optional hook health check method to verify hooks are installed before recording.
- `OpenRPA.Resources.strings.resx` and `strings.zh.resx`: New resource strings for recording feedback and error messages.
