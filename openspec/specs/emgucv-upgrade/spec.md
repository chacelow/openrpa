# emgucv-upgrade Specification

## Purpose
TBD - created by archiving change upgrade-emgucv-to-latest. Update Purpose after archive.
## Requirements
### Requirement: Emgu.CV package upgraded to latest

项目 SHALL 引用 Emgu.CV 最新稳定版（4.9.0+）及对应的 Windows 运行时包。编译和运行时 MUST 无崩溃。

#### Scenario: Build succeeds with new package

- **WHEN** 执行 `dotnet build OpenRPA.Image`
- **THEN** 编译 0 errors，所有 Emgu.CV 类型解析正常

#### Scenario: Runtime image operations work

- **WHEN** 运行时执行截图、模板匹配、轮廓检测
- **THEN** 功能与升级前一致，无异常

### Requirement: Image construction uses Bitmap.ToImage extension

所有 `new Image<TColor, TDepth>(Bitmap)` 构造调用 SHALL 替换为 `bitmap.ToImage<TColor, TDepth>()` 扩展方法。

#### Scenario: Template matching with Bitmap input

- **WHEN** 调用 `Matches.FindMatches(Bitmap, Bitmap, double, int, bool)`
- **THEN** 内部 `Bitmap → Image<>` 转换成功，匹配结果正确

### Requirement: FeatureMatching uses AKAZE float descriptors

当 `MatchMode = FeatureMatching` 时，系统 SHALL 使用 `AKAZE(AKAZE.DescriptorType.KAZE)` 生成浮点描述符，配合 FLANN KdTree 匹配器。

#### Scenario: AKAZE matches scaled UI element

- **WHEN** 模板在源图中以不同比例存在
- **THEN** 特征点匹配率比 KAZE 提升 20% 以上，能正确返回匹配矩形

