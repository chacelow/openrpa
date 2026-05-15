## Why

OpenRPA currently records Windows/UIA mouse clicks as absolute pixel offsets from the captured element rectangle. This works when the element size is stable, but becomes fragile when dialogs, embedded WebView panes, or fused UI containers resize between recording and replay.

The recorder needs an explicit capture mode and a size-independent UIA click position model so recorded clicks remain aligned to the intended visual region when the target element bounds change.

## What Changes

- Add recording capture options modeled after Power Automate:
  - UIA as the default capture mode.
  - MSAA as a visible option, marked unavailable until a real MSAA recorder is added.
  - Image recording as an explicit capture mode.
- Change UIA click recording to store the click position as a relative ratio inside the clicked component region, not only as fixed pixel offsets.
- During replay of UIA clicks, resolve the current component rectangle and compute the actual click point from the stored ratio.
- Keep image recording explicit instead of implicitly overriding UIA for broad `Pane` elements.
- Surface the selected capture mode in the recorder UI so users can choose the intended engine before recording.

## Capabilities

### New Capabilities

- `recording-capture-options`: Covers recorder capture mode selection and size-independent UIA click position recording/replay.

### Modified Capabilities

None.

## Impact

- `OpenRPA.Windows` recording flow, especially UIA mouse move and mouse up event creation.
- `OpenRPA.Interfaces.IRecordEvent` and concrete record event data passed from plugins to `MainWindow`.
- `OpenRPA.Activities.ClickElement` and `IElement.Click` behavior for ratio-based UIA click replay.
- Recorder UI resources and views where capture mode selection is exposed.
- Existing image recording plugin behavior where it currently converts some broad UIA panes to image-based activities implicitly.
- MSAA is not implemented in this change; selecting it must not silently produce UIA output.
