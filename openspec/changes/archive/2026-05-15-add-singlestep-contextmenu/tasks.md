## 1. Enable Context Menu

- [x] 1.1 Uncomment `runthis` MenuItem declaration, initialization, and click handler wiring in WFDesigner constructor.
- [x] 1.2 Implement `OnRunthis` method: set `Singlestep = true`, then call `Run(VisualTracking, SlowMotion, null)`.
- [x] 1.3 Remove `#if DEBUG` guard from `WorkflowDesigner.ContextMenu.Items.Add(runthis)`.

## 2. Verification

- [x] 2.1 Build and verify no compile errors.
- [ ] 2.2 Manually verify right-click → "运行此活动" starts single-step execution and pauses at each activity.
