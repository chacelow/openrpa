## 1. Capture Mode Model

- [x] 1.1 Add a recorder capture mode enum with `UIA`, `MSAA`, and `Image` values.
- [x] 1.2 Add local configuration state for the selected recorder capture mode with `UIA` as the default.
- [x] 1.3 Expose the capture mode selector in the recording UI and bind it to the selected mode.
- [x] 1.4 Add English and Chinese resource strings for the capture mode UI labels.

## 2. UIA Ratio Click Recording

- [x] 2.1 Extend `IRecordEvent` and `OpenRPA.Windows.RecordEvent` with nullable click ratio fields.
- [x] 2.2 Compute bounded `ClickRatioX` and `ClickRatioY` from the UIA element rectangle during Windows/UIA mouse-up recording.
- [x] 2.3 Ensure UIA mode emits UIA recording output directly and does not invoke image contour replacement.
- [x] 2.4 Keep text input and select-list recording behavior unchanged when ratio click data is present.

## 3. Ratio Click Replay

- [x] 3.1 Extend `ClickElement` with nullable click ratio arguments.
- [x] 3.2 Pass click ratio arguments from `MainWindow.OnUserAction` into recorded `ClickElement` activities.
- [x] 3.3 Extend `IElement.Click` and `UIElement.Click` to compute current pixel offsets from click ratios when ratio values are supplied.
- [x] 3.4 Preserve existing pixel offset behavior for activities that do not contain ratio values.

## 4. Explicit Image and MSAA Modes

- [x] 4.1 Route image capture mode to the image recording path without relying on implicit `Pane` conversion.
- [x] 4.2 Add the MSAA capture mode boundary and wire the selected mode through the recorder start/action path.
- [x] 4.3 Mark MSAA capture as unavailable when no MSAA record plugin exists.
- [x] 4.4 Prevent cross-mode fallback so UIA, MSAA, and Image output remain predictable.

## 5. Verification

- [x] 5.1 Add focused unit tests for ratio calculation and replay offset calculation.
- [ ] 5.2 Manually verify a UIA click inside a resized dialog replays at the same relative component region.
- [ ] 5.3 Manually verify a broad WebView or `Pane` capture in UIA mode records UIA output, while image mode records image output.
- [x] 5.4 Build the affected projects and fix compile or binding errors.
