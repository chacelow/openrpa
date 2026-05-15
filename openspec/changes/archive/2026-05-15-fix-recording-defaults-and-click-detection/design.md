## Context

The `add-recording-capture-options` change added a capture mode dropdown (`UIA`/`MSAA`/`Image`) in the recorder ribbon, relative click position recording, and mode filter for record parsers. However, three defects surfaced during testing:

1. The `RibbonComboBox` in `MainWindow.xaml` shows an empty selection box despite having three items and a valid default (`UIA`). The binding tree (`RibbonGallery.SelectedItem` → `defaultrecordingcapturemode` → `recordingCaptureModes` → `DisplayMemberPath="Name"`) is structurally correct but the `capturemode` class lacks value equality, and the `RibbonComboBox` outer control does not receive a text/display binding to render the selected item name in its collapsed state.

2. The Windows record plugin's `OnMouseUp` handler silently returns when `sel.Count < 2` (insufficient selector depth), and all exceptions inside the background recording thread are caught without user feedback. The user clicks but nothing happens, with no indication of whether recording is active or why the click was ignored.

3. `StartRecordPlugins` unconditionally starts the Windows (UIA) plugin regardless of `Config.local.recording_capture_mode`. The capture mode only affects which *parser* plugins are allowed via `IsRecordParserAllowed`, but the primary event source is always Windows. This is incompatible with the Image capture mode intent.

## Goals / Non-Goals

**Goals:**

- Fix the `RibbonComboBox` to display the currently selected capture mode name (default: "UIA") in its collapsed state.
- Ensure the capture mode selector reflects the persisted `recording_capture_mode` configuration on application startup.
- Give user-visible feedback when a click cannot be captured (selector generation failure) so the user understands why no activity was recorded.
- Wire `StartRecordPlugins` to respect the configured capture mode when deciding which record plugin to start.
- Add a health check that verifies `InputDriver` low-level hooks are installed before recording starts, and show an error if hooks are unavailable.
- Add clear visual indicators that recording is active (status bar text, button state change).

**Non-Goals:**

- Do not implement a full MSAA record plugin.
- Do not change the `InputDriver` hook mechanism or low-level hook installation logic beyond adding a health check.
- Do not change selector generation logic.
- Do not change the image recording plugin itself.
- Do not add new capture modes beyond UIA, MSAA, Image.

## Decisions

### Bind RibbonComboBox Text to display selected mode

The `RibbonComboBox` wraps a `RibbonGallery` but does not automatically reflect the gallery's selected item display text in its collapsed state. To fix this, add a `Text` binding on the `RibbonComboBox` that points to the computed display name of the current capture mode.

Rationale: The `RibbonComboBox.Text` property is writable and directly controls what text the user sees in the collapsed control. Binding it to the resolved mode name ensures the control always shows the current selection. This approach is consistent with how other WPF comboboxes display selection text.

Alternative considered: Override `Equals`/`GetHashCode` on `capturemode` so `RibbonGallery.SelectedItem` matching is value-based. Rejected because this alone does not guarantee the `RibbonComboBox` outer chrome will render the name — WPF Ribbon's gallery selection synchronization is known to have edge cases with the outer combo box display.

Alternative considered: Use `RibbonComboBox.SelectionBoxItem` binding. Rejected because `SelectionBoxItem` expects the selected object itself, not just its display string, and the outer combo box would need an `ItemTemplate` to render it.

### Add captureModeDisplayName computed property

Add a `captureModeDisplayName` property to `MainWindow` that returns the display name of the currently selected capture mode. The `RibbonComboBox.Text` binding will use this property. The setter of `defaultrecordingcapturemode` will call `NotifyPropertyChanged("captureModeDisplayName")` to keep the text in sync.

Rationale: Keeps the binding simple (one-way text binding) without requiring changes to the `capturemode` class. The property acts as a computed projection from the current mode to its display string.

### Wire StartRecordPlugins to respect capture mode

Refactor `StartRecordPlugins` to check `Config.local.recording_capture_mode` and start the appropriate plugin:
- `UIA` → start "Windows" plugin (current behavior, unchanged)
- `Image` → start "Image" plugin if available
- `MSAA` → start "MSAA" plugin if available, otherwise show unavailable message

The function signature changes to accept no `all` parameter (the `all` flag was only used to control SAP plugin startup, which is preserved via explicit check).

Rationale: The capture mode is supposed to control which recording engine is used. Starting only the appropriate plugin aligns the runtime behavior with the user's explicit mode selection.

Alternative considered: Always start all plugins and filter at the event level. Rejected because:
- It would waste resources running unused plugins.
- The Image plugin may have side effects even when not supposed to be active.
- The MSAA plugin doesn't exist yet, and starting it would fail.

### Add hook health check before recording

Before starting any record plugin, verify that `InputDriver.Instance.IsInitialized` is `true`. If hooks are not installed, show an error dialog and abort recording start.

Rationale: The `InputDriver` hooks require low-level Windows hook installation, which can fail due to permissions, security software, or unexpected process state (e.g., hook limit reached). Checking before recording provides a clear error message rather than silently failing.

### Add click capture failure feedback

In the Windows Plugin's `OnMouseUp`, when `sel.Count < 2` or an exception occurs during selector creation, log the failure at warning level and invoke a new `OnCaptureFailed` event on `IRecordPlugin`. `MainWindow` subscribes to this event and shows a brief transient message in the status bar (e.g., "Click not captured — insufficient selector information").

Rationale: The current silent failure is the most directly user-visible bug. Users click elements during recording and see nothing happen, with no indication of whether recording is active or why their action was ignored. A status bar message provides immediate feedback without being intrusive.

Alternative considered: Show a message box for each failed click. Rejected because it would be too disruptive during a recording session where the user may click many non-capturable elements.

Alternative considered: Flash the overlay window red. Rejected because the overlay uses `System.Drawing` (WinForms interop) and adding an asynchronous color animation on a transparent overlay window is fragile and complex.

### Add recording visual state indicators

When recording starts:
- Set a `IsRecording` property on `MainWindow` that the XAML can bind to for button state changes.
- Update the status bar text to indicate recording is active (e.g., "Recording…").
- Optionally change the Record button's image or label to indicate the active state.

When recording stops, restore the original states.

Rationale: Users need clear confirmation that recording is active, especially since the OpenRPA window is typically minimized during recording (`GenericTools.Minimize()`).

## Risks / Trade-offs

- [Risk] Changing `StartRecordPlugins` to respect capture mode could break existing workflows that depend on implicit UIA fallback. → Mitigation: UIA is the default mode, so existing users with default config are unaffected. Users who explicitly selected Image mode will now get the correct (Image) plugin instead of Windows+Image parser.
- [Risk] `RibbonComboBox.Text` binding may introduce a brief flicker or binding error on initial load if the computed property returns null before the collection populates. → Mitigation: The `captureModeDisplayName` property defaults to "UIA" (hardcoded fallback) if the collection is empty or the mode is not found.
- [Risk] The `OnCaptureFailed` event adds a new member to `IRecordPlugin`, which is a public interface in `OpenRPA.Interfaces`. External plugin implementations would need to implement this member. → Mitigation: Make `OnCaptureFailed` a nullable event (like a callback) rather than a required interface member. Or use a simpler approach: log a message that `MainWindow` can observe through the existing logging infrastructure rather than adding a new interface member.
- [Risk] Hook health check may report false negatives if called too early (before `InputDriver.Initialize()` completes). → Mitigation: `InputDriver.Initialize()` is called from `RobotInstance.MainWindowReadyForAction()`, which fires after the main window is ready. Recording cannot start before the window is ready, so hooks will always be installed by the time the user clicks Record.

## Open Questions

- Should we add a distinct `OnCaptureFailed` event to `IRecordPlugin` (breaking change for external plugins), or use the existing logging infrastructure to communicate failures? Current leaning: use the existing logging + status bar update pattern from `MainWindow` without modifying the interface.
