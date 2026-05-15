## 1. Capture Mode Dropdown Display Fix

- [x] 1.1 Add `captureModeDisplayName` computed property to `MainWindow` that returns the display name of the current capture mode (default "UIA"), and wire `NotifyPropertyChanged("captureModeDisplayName")` in the `defaultrecordingcapturemode` setter.
- [x] 1.2 Add `Text="{Binding captureModeDisplayName, Mode=OneWay}"` to the `RibbonComboBox` in `MainWindow.xaml` at the capture mode selector (around line 120).
- [x] 1.3 Add `IsRecording` property to `MainWindow` with `NotifyPropertyChanged` and bind Record button visual state (e.g., `IsEnabled` or label change) to reflect active recording.

## 2. Hook Health Check and Capture Mode Wiring

- [x] 2.1 Add `IsInitialized` public property to `InputDriver` that returns `true` when keyboard and mouse hooks are installed.
- [x] 2.2 Refactor `StartRecordPlugins` in `MainWindow.xaml.cs` to check `Config.local.recording_capture_mode` and start only the corresponding plugin (Windows for UIA, Image for Image, MSAA for MSAA), instead of always starting Windows.
- [x] 2.3 Add hook health check in `OnRecord`: before calling `StartRecordPlugins`, verify `InputDriver.Instance.IsInitialized`. If false, show error message box and abort recording start.
- [x] 2.4 Remove or adapt the `all` parameter from `StartRecordPlugins` so the call site doesn't pass a misleading flag.
- [x] 2.5 Handle the case where the selected mode's plugin is not available (show error, abort recording start).

## 3. Click Capture Failure Feedback

- [x] 3.1 In `OpenRPA.Windows.Plugin.OnMouseUp`, when `sel.Count < 2`, add a `Log.Warning` message indicating the click was not captured due to insufficient selector, before the silent return.
- [x] 3.2 In `OpenRPA.Windows.Plugin.OnMouseUp`, improve the catch block to log a more specific error message when selector creation fails, rather than the generic `Log.Error(ex.ToString())`.
- [x] 3.3 Add a `RecordingStatusMessage` property to `MainWindow` with `NotifyPropertyChanged`, and bind the status bar to it. Set it to "Recording…" when recording starts and clear it when recording stops.
- [x] 3.4 After a click capture failure, set `RecordingStatusMessage` to a transient warning (e.g., "Click not captured — try a different element") that auto-clears after 3 seconds using a dispatcher timer.

## 4. Resources and Localization

- [x] 4.1 Add English resource strings to `OpenRPA/Resources/strings.resx` for recording active status, hook failure, and click capture failure messages.
- [x] 4.2 Add Chinese (zh) resource strings to `OpenRPA/Resources/strings.zh.resx` for the same messages.

## 5. Verification

- [x] 5.1 Build the affected projects (`OpenRPA`, `OpenRPA.Windows`, `OpenRPA.Interfaces`) and fix any compile errors.
- [ ] 5.2 Manually verify the capture mode dropdown shows "UIA" on first launch and updates when switching modes.
- [ ] 5.3 Manually verify recording starts with visual status indicators, and clicking a capturable element produces a recorded activity.
- [ ] 5.4 Manually verify clicking a non-capturable element shows a transient warning in the status bar and does not crash the recording session.
- [ ] 5.5 Manually verify selecting Image mode starts the Image plugin (if available) and produces image-based activities.
