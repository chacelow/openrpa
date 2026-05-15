## 1. Recording Defaults

- [x] 1.1 In `MainWindow.xaml.cs` `OnUserAction`, add `Focus = true` to the recorded `ClickElement` activity construction.
- [x] 1.2 In `MainWindow.xaml.cs` `OnUserAction`, change `VirtualClick` from `Config.local.use_virtual_click` to hardcoded `false` for recorded activities.

## 2. Verification

- [x] 2.1 Build and verify no compile errors.
- [ ] 2.2 Manually verify a recorded click produces a `ClickElement` with `Focus = true` and `VirtualClick = false`.
- [ ] 2.3 Manually verify a replayed click focuses the element before clicking.
