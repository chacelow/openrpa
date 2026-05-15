# recording-ux-feedback Specification

## Purpose
TBD - created by archiving change fix-recording-defaults-and-click-detection. Update Purpose after archive.
## Requirements
### Requirement: Recording active visual state

The application SHALL visually indicate when recording is active by updating the status bar text to a recording-specific message and toggling the Record button appearance.

#### Scenario: Recording starts

- **WHEN** the user clicks the Record button and recording begins successfully
- **THEN** the status bar displays "Recording…" (or localized equivalent) and the Record button reflects its active state

#### Scenario: Recording stops

- **WHEN** recording is stopped or cancelled
- **THEN** the status bar returns to its previous message and the Record button returns to its inactive state

### Requirement: Input hook health verification

Before starting recording, the system SHALL verify that the `InputDriver` low-level mouse and keyboard hooks are installed. If hooks are not installed, the system MUST display an error message and MUST NOT start recording.

#### Scenario: Hooks are installed

- **WHEN** the user clicks Record and the InputDriver hooks are active
- **THEN** recording proceeds normally

#### Scenario: Hooks are not installed

- **WHEN** the user clicks Record and the InputDriver hooks are not installed
- **THEN** the system displays an error message indicating that input hooks could not be installed and recording does not start

### Requirement: Click capture failure feedback

When a mouse click is detected during recording but cannot be captured as a recorded activity (e.g., insufficient selector depth), the system SHALL report the failure to the user through the status bar or logging output.

#### Scenario: Click produces insufficient selector

- **WHEN** the user clicks an element during UIA recording that yields fewer than 2 selector items
- **THEN** a warning is logged indicating the click was not captured, and a transient status message informs the user

#### Scenario: Click capture throws an exception

- **WHEN** an exception occurs during selector creation or event processing in the recording thread
- **THEN** the error is logged and a status message indicates that the click could not be captured, without terminating the recording session

### Requirement: Recording failure does not terminate session

When a single click fails to capture, the recording session SHALL remain active so the user can continue recording subsequent clicks.

#### Scenario: Click fails, session continues

- **WHEN** a click fails to capture during an active recording session
- **THEN** the recording session remains active and subsequent clicks are processed normally

