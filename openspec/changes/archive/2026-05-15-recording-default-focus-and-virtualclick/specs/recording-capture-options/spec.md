## ADDED Requirements

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
