## ADDED Requirements

### Requirement: Recorder capture mode selection

The recorder SHALL provide a capture mode option with `UIA`, `MSAA`, and `Image` choices, and `UIA` MUST be the default mode for new recording sessions.

#### Scenario: Default capture mode

- **WHEN** the user opens or starts a new recording session without changing capture settings
- **THEN** the recorder uses `UIA` as the active capture mode

#### Scenario: User selects image recording

- **WHEN** the user selects `Image` as the capture mode before recording a click
- **THEN** the recorder creates image-based recording output for the captured click

#### Scenario: User selects MSAA recording

- **WHEN** the user selects `MSAA` as the capture mode before recording a click
- **THEN** the recorder indicates that MSAA capture is unavailable in the current build and MUST NOT emit UIA recording output for that click

### Requirement: UIA clicks record relative click position

When recording a click in `UIA` mode, the recorder SHALL store the click position as a relative ratio within the captured UIA element rectangle.

#### Scenario: UIA click inside a valid element rectangle

- **WHEN** the user clicks a UIA element whose rectangle has positive width and height
- **THEN** the recorded click includes `ClickRatioX` and `ClickRatioY` values derived from the clicked point inside that rectangle

#### Scenario: UIA click ratios are bounded

- **WHEN** the clicked point is converted to relative ratio values
- **THEN** each recorded ratio value MUST be constrained to the inclusive range from `0` to `1`

### Requirement: UIA clicks replay from current element size

When replaying a UIA click that contains relative click ratios, the runtime SHALL compute the click offset from the resolved element's current rectangle size.

#### Scenario: Element width changes after recording

- **WHEN** a UIA click was recorded at a horizontal ratio inside an element and the element width changes before replay
- **THEN** the replayed click uses the same horizontal ratio inside the current element width

#### Scenario: Element height changes after recording

- **WHEN** a UIA click was recorded at a vertical ratio inside an element and the element height changes before replay
- **THEN** the replayed click uses the same vertical ratio inside the current element height

### Requirement: Capture modes do not implicitly replace each other

The recorder MUST NOT silently replace a click captured in `UIA` mode with image recording or MSAA recording output.

#### Scenario: UIA mode captures a broad pane

- **WHEN** the active capture mode is `UIA` and the clicked UIA element is a broad `Pane`
- **THEN** the recorder emits UIA recording output with relative click position data instead of automatically converting the output to image recording

#### Scenario: Image mode is explicit

- **WHEN** the user wants image-based recording
- **THEN** the user selects `Image` capture mode before recording the click

#### Scenario: MSAA mode is unavailable

- **WHEN** the active capture mode is `MSAA` and no MSAA record plugin is available
- **THEN** the recorder displays an unavailable message and does not create a recorded click activity
