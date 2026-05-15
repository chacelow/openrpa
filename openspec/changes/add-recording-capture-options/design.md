## Context

The current recorder starts the Windows record plugin as the primary input source. `OpenRPA.Windows.Plugin` captures UIA elements from the mouse position, builds a `WindowsSelector`, and records `OffsetX` / `OffsetY` as pixel distances from the element rectangle. `MainWindow.OnUserAction` then adds a `ClickElement` activity with those offsets. At replay time, `UIElement.Click` clicks `Rectangle.X + OffsetX` and `Rectangle.Y + OffsetY` when virtual click is not used.

This means the click target is stable only when the recorded element rectangle keeps the same size. In WebView or custom panes, UIA may expose one large `Pane` while the visual target is only a sub-region. If that pane resizes, fixed offsets drift away from the intended visual area.

The image plugin already tries to reinterpret some broad `Pane` captures by contour detection, but that behavior is implicit. It helps in some cases, yet it also hides the user's intended capture engine and makes recording behavior harder to reason about.

## Goals / Non-Goals

**Goals:**

- Provide explicit recording capture modes: UIA, MSAA, and image recording.
- Keep UIA as the default mode.
- Show MSAA as an unavailable option in this change rather than implementing a partial MSAA recorder.
- Store UIA click positions as ratios within the clicked element/component rectangle.
- Compute replay click coordinates from the current rectangle and stored ratios.
- Make image recording an explicit user choice rather than an implicit replacement for UIA.
- Keep the implementation scoped to recorder selection and click coordinate semantics.

**Non-Goals:**

- Do not redesign selector generation.
- Do not add heuristic fallback chains between UIA, MSAA, and image recording.
- Do not change text input or select-list recording semantics.
- Do not implement broad computer vision recognition beyond the existing image recording behavior.
- Do not implement the MSAA element model, selector model, or recorded activity path in this change.

## Decisions

### Use Explicit Capture Mode Selection

The recorder will expose a capture mode option with `UIA`, `MSAA`, and `Image` values. `UIA` is selected by default.

Rationale: the same screen point can be interpreted differently by UIA, MSAA, and image recognition. Making the mode explicit gives the user control and keeps the generated workflow predictable.

Alternative considered: keep UIA as the only direct recorder and let other plugins parse or replace the event automatically. This is rejected because implicit conversion makes WebView and large pane cases unpredictable and hard to debug.

### Store UIA Click Position as Ratios

For UIA click recording, compute:

- `ClickRatioX = OffsetX / Element.Rectangle.Width`
- `ClickRatioY = OffsetY / Element.Rectangle.Height`

The ratios are clamped to the `[0, 1]` range at recording time and replayed against the current element rectangle.

Rationale: ratios preserve the user's intended location within the element when the element grows or shrinks. This directly addresses dialog resizing and fused WebView UI regions without requiring selector changes.

Alternative considered: store offsets relative to the top-level window. This is rejected because embedded WebView panes and nested containers can move independently from the window content area.

### Add Ratio-Aware Click Activity Semantics

`ClickElement` will carry ratio click data for recorded UIA clicks and pass it to the element click operation. The click implementation will compute pixel offsets at execution time using the current rectangle size.

Rationale: replay-time computation must happen as close as possible to the actual click because the element rectangle may only be known after selector resolution and refresh.

Alternative considered: convert ratios back to pixels in `MainWindow.OnUserAction` before creating the activity. This is rejected because it would still freeze the recorded rectangle size into the workflow.

### Keep Image Recording Explicit

When the selected capture mode is `Image`, recording should produce image-based activities through the image plugin. When the selected capture mode is `UIA`, the recorder should not silently replace a UIA click with image contour detection.

Rationale: the user's core goal is stable recording. Hidden mode switching creates a different kind of instability because the same action can produce different activity types depending on heuristics.

Alternative considered: prefer image recording automatically for `Pane` / WebView elements. This is rejected because `Pane` is too broad and includes legitimate UIA targets.

### Show MSAA Without Emitting Fake MSAA Output

MSAA is introduced as a selectable option, but this change does not implement a real MSAA recorder. If selected in a build without an `MSAA` record plugin, recording is blocked with a clear unavailable message.

Rationale: MSAA has different element identity and bounds semantics from UIA. Treating UIA `LegacyIAccessible` or a Windows/UIA event as MSAA would create incorrect workflow semantics.

## Risks / Trade-offs

- [Risk] Existing workflows created before this change may only contain pixel offsets. -> Mitigation: new recordings use ratio data; existing workflow behavior is not reinterpreted by this proposal.
- [Risk] Very small or zero-sized rectangles cannot produce meaningful ratios. -> Mitigation: ratio recording requires a valid non-empty rectangle before emitting a UIA click activity.
- [Risk] Ratio clicks still depend on the chosen UIA element being the correct component region. -> Mitigation: expose capture mode selection and keep image mode explicit for cases where UIA cannot identify useful regions.
- [Risk] Users may select MSAA before a real MSAA recorder exists. -> Mitigation: show a clear unavailable message and do not emit UIA output under the MSAA option.
