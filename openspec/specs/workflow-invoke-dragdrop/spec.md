# workflow-invoke-dragdrop Specification

## Purpose
TBD - created by archiving change add-workflow-invoke-dragdrop. Update Purpose after archive.
## Requirements
### Requirement: Drag workflow from project tree to designer

The system SHALL allow dragging a workflow from the OpenProject tree onto the WFDesigner surface, creating an InvokeWorkflow activity that references the dropped workflow.

#### Scenario: User drags workflow to designer

- **WHEN** user drags a workflow from the project tree and drops it on the workflow designer
- **THEN** an InvokeWorkflow activity is inserted at the drop position, referencing the dropped workflow

#### Scenario: Drop rejected for non-workflow items

- **WHEN** user drags a non-workflow item (e.g., Detector) onto the designer
- **THEN** the drop is rejected with no action

### Requirement: Snippets panel shows user project workflows

The Snippets panel SHALL display the user's project workflows alongside built-in snippets, with distinct visual grouping for user projects.

#### Scenario: User project workflows appear in Snippets

- **WHEN** the Snippets panel is loaded
- **THEN** a "My Projects" section displays the user's projects and their workflows as draggable items

#### Scenario: Drag user workflow from Snippets to designer

- **WHEN** user drags a workflow from the Snippets "My Projects" section onto the designer
- **THEN** an InvokeWorkflow activity is created referencing the dropped workflow

### Requirement: Auto-map sub-workflow arguments

When an InvokeWorkflow activity is created via drag-drop, the system SHALL read the target workflow's argument definitions and auto-populate the parameter mappings.

#### Scenario: In arguments left empty for user input

- **WHEN** the target workflow has In arguments (e.g., username, password)
- **THEN** the InvokeWorkflow's In argument slots are created but left empty for manual mapping

#### Scenario: Out arguments auto-create parent variables

- **WHEN** the target workflow has Out arguments (e.g., token, result)
- **THEN** corresponding variables are automatically created in the parent workflow with prefix "var_"

