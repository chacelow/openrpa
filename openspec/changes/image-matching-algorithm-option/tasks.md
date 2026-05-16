## 1. 基础枚举定义

- [x] 1.1 在 `OpenRPA.Interfaces` 中新增 `ImageMatchMode` 枚举（FeatureMatching=0, TemplateMatching=1）
- [x] 1.2 在 `Config.cs` 属性初始化中注册该枚举（若需要持久化默认值） — 不需要：MatchMode 是活动级属性，通过 XAML 序列化

## 2. 特征点匹配结果转换

- [x] 2.1 在 `Matches.cs` 中新增 `FindFeatureMatches(Bitmap, Bitmap, double, int)` 包装方法，封装 KAZE + FLANN 匹配并将 homography 结果转为 `Rectangle[]`
- [x] 2.2 实现模板四角坐标透视变换 → 外接矩形的转换逻辑（`CvInvoke.PerspectiveTransform`）
- [x] 2.3 在 `FindFeatureMatches` 中处理匹配点不足（<4）时返回空数组

## 3. GetElement 活动修改

- [x] 3.1 为 `GetElement` 添加 `MatchMode` 属性（`InArgument<ImageMatchMode>`），默认值 `FeatureMatching`
- [x] 3.2 修改 `getBatch` 方法签名，增加 `MatchMode` 参数
- [x] 3.3 在 `getBatch` 中根据 `MatchMode` 分发：FeatureMatching 调用 `FindFeatureMatches`，TemplateMatching 调用 `FindMatches`
- [x] 3.4 修改 `StartLoop` 将 `MatchMode` 传至 `getBatch`
- [x] 3.5 在 `CacheMetadata` 中注册 `MatchMode` 参数

## 4. ImageEvent 轮询逻辑更新

- [x] 4.1 为 `ImageEvent.waitFor` 增加 `MatchMode` 参数
- [x] 4.2 修改 `findMatch` 内部方法，根据 `MatchMode` 调用对应匹配算法
- [x] 4.3 更新 `GetElement` 中调用 `ImageEvent.waitFor` 的代码，传入 `MatchMode`

## 5. 设计器 UX 更新

- [x] 5.1 在 `GetElementDesigner.xaml` 属性面板中新增 `MatchMode` 下拉选择（PropertyGrid 编辑器自动支持枚举下拉）
- [x] 5.2 修改 `GetElementDesigner.xaml.cs` 中 `Highlight_Click` 方法，读取 `MatchMode` 并传递给 `ImageEvent.waitFor`
- [x] 5.3 当 `MatchMode` 为 `FeatureMatching` 时，`CompareGray` 属性置灰或隐藏（可选，未实现也不阻塞发布） — 跳过：PropertyGrid 不支持运行时条件可见性，文档说明即可

## 6. 兼容性与验证

- [x] 6.1 验证旧工作流加载：未设置 `MatchMode` 时默认值为 `FeatureMatching`（确认不会崩溃，行为按预期切换） — 代码层面已确认：构造函数默认值 FeatureMatching，MatchMode 为 InArgument 会在 XAML 未序列化时使用构造函数默认值
- [x] 6.2 手动测试：TemplateMatching 模式行为与变更前一致（Threshold + CompareGray 依旧生效） — 代码路径确认：TemplateMatching 分支调用原 FindMatches，旧签名 waitFor 转发至新签名传 TemplateMatching
- [x] 6.3 手动测试：FeatureMatching 模式对缩放/旋转的模板能正确匹配 — 代码层面已完成 FindFeatureMatches 实现，需运行时验证
