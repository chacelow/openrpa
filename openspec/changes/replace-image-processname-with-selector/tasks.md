## 1. GetElement 属性修改

- [x] 1.1 在 `GetElement.cs` 中新增 `InArgument<string> Selector` 属性
- [x] 1.2 移除 `Processname` 属性和 `Limit` 属性
- [x] 1.3 修改 `getBatch` 方法签名，移除 `Processname` 和 `limit` 参数，新增 `selector` 参数
- [x] 1.4 在 `getBatch` 中将 Selector 字符串传入 `ImageEvent.waitFor`
- [x] 1.5 修改 `StartLoop` 读取 Selector 并传给 `getBatch`
- [x] 1.6 在 `CacheMetadata` 中移除 Processname/Limit，注册 Selector

## 2. 设计器 UX 替换

- [x] 2.1 在 `GetElementDesigner.xaml` 中移除 "Process限制" 和 "Clear Process Limit" 按钮
- [x] 2.2 新增 "Open Selector" 按钮（参考 Windows GetElementDesigner 的实现）
- [x] 2.3 实现 `Open_Selector` 方法：调起 `SelectorWindow` 弹框，回写 Selector JSON
- [x] 2.4 修改 `Highlight_Click` 读取 Selector 并传入，移除 Processname/Limit 引用

## 3. ImageEvent 搜索逻辑替换

- [x] 3.1 修改 `ImageEvent.waitFor` 签名，移除 `Processname` 和 `Limit` 参数
- [x] 3.2 修改 `ImageEvent.findMatch`：当 Selector 不为空时，用 Selector 解析窗口 Rectangle 截图搜索
- [x] 3.3 移除 `findMatch` 中 Processname 分支和 `WindowRects` 调用
- [x] 3.4 保留无 Selector 时的全屏搜索分支

## 4. 清理与验证

- [x] 4.1 移除 `ImageEvent` 中的 Processname、limit 相关字段
- [x] 4.2 构建验证 0 errors
- [ ] 4.3 手动测试：无 Selector → 全屏搜索正常
- [ ] 4.4 手动测试：有 Selector → 仅窗口内搜索正常
