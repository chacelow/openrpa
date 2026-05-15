# singlestep-contextmenu Specification

## Purpose
TBD - created by archiving change add-singlestep-contextmenu. Update Purpose after archive.
## Requirements
### Requirement: Right-click single-step execution

The workflow designer SHALL provide a right-click context menu option "Run this activity" that triggers single-step debug execution of the workflow.

#### Scenario: User triggers single-step from context menu

- **WHEN** user right-clicks an activity in the workflow designer and selects "Run this activity"
- **THEN** the workflow starts executing in single-step mode, pausing at each activity for user inspection

#### Scenario: Single-step pauses at each activity

- **WHEN** single-step mode is active and the workflow engine reaches the next activity
- **THEN** the designer highlights the current activity, shows current variable values, and waits for user to continue

