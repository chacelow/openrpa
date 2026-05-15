# recording-capture-options Specification

## Purpose
TBD - created by archiving change fix-recording-defaults-and-click-detection. Update Purpose after archive.
## Requirements
### Requirement: Recorder capture mode selection

The recorder SHALL provide a capture mode option with `UIA`, `MSAA`, and `Image` choices, and `UIA` MUST be the default mode for new recording sessions. The capture mode dropdown SHALL always display the currently selected mode name, including on application startup before any user interaction.

#### Scenario: Default capture mode

- **WHEN** the user opens or starts a new recording session without changing capture settings
- **THEN** the recorder uses `UIA` as the active capture mode and the dropdown displays "UIA"

#### Scenario: Dropdown displays current selection on startup

- **WHEN** the application starts with a previously saved capture mode configuration
- **THEN** the capture mode dropdown displays the name of the saved mode (e.g., "Image") without requiring the user to open the dropdown

#### Scenario: User selects image recording

- **WHEN** the user selects `Image` as the capture mode before recording a click
- **THEN** the recorder creates image-based recording output for the captured click and the dropdown displays "Image"

#### Scenario: User selects MSAA recording

- **WHEN** the user selects `MSAA` as the capture mode before recording a click
- **THEN** the recorder indicates that MSAA capture is unavailable in the current build and MUST NOT emit UIA recording output for that click

### Requirement: Recording start path respects capture mode

When recording starts, the system SHALL activate only the record plugin corresponding to the currently selected capture mode, rather than unconditionally starting the Windows (UIA) plugin.

#### Scenario: UIA mode starts Windows plugin

- **WHEN** the capture mode is `UIA` and the user clicks Record
- **THEN** only the Windows record plugin is started for capturing input events

#### Scenario: Image mode starts Image plugin

- **WHEN** the capture mode is `Image`, the Image record plugin is available, and the user clicks Record
- **THEN** only the Image record plugin is started for capturing input events

#### Scenario: Image mode with no Image plugin

- **WHEN** the capture mode is `Image`, no Image record plugin is available, and the user clicks Record
- **THEN** recording does not start and an error message indicates that the Image capture mode is unavailable

#### Scenario: MSAA mode starts MSAA plugin

- **WHEN** the capture mode is `MSAA`, the MSAA record plugin is available, and the user clicks Record
- **THEN** only the MSAA record plugin is started for capturing input events

#### Scenario: MSAA mode with no MSAA plugin

- **WHEN** the capture mode is `MSAA`, no MSAA record plugin is available, and the user clicks Record
- **THEN** recording does not start and an error message indicates that MSAA capture mode is unavailable

### Requirement: UIA clicks record relative click position

When recording a click in `UIA` mode, the recorder SHALL store the click position as a relative ratio within the captured UIA element rectangle, and SHALL set the recorded `ClickElement` activity defaults to `Focus = true` and `VirtualClick = false`.

#### Scenario: UIA click inside a valid element rectangle

- **WHEN** the user clicks a UIA element whose rectangle has positive width and height
- **THEN** the recorded click includes `ClickRatioX` and `ClickRatioY` values derived from the clicked point inside that rectangle, and the generated `ClickElement` activity has `Focus = true` and `VirtualClick = false`

#### Scenario: UIA click ratios are bounded

- **WHEN** the clicked point is converted to relative ratio values
- **THEN** each recorded ratio value MUST be constrained to the inclusive range from `0` to `1`

#### Scenario: Recorded click focuses element before clicking

- **WHEN** a recorded click is replayed
- **THEN** the target element receives focus before the click point is resolved, improving click reliability for elements that require focus

