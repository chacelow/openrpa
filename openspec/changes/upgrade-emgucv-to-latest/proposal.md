## Why

Emgu.CV 4.1.1 中 AKAZE 特征检测器因二进制描述符与 FLANN 匹配器不兼容而无法使用，KAZE 对 UI 缩放场景的特征点匹配率从 1:1 的 80%+ 骤降到缩放后的 40% 以下。升级到 Emgu.CV 最新版（4.9+）可完整支持 AKAZE 浮点描述符 + FLANN 匹配，显著提升缩放不变性。同时 4.x 后续版本在矩阵运算性能上有约 20% 的提升。

## What Changes

- **升级** Emgu.CV NuGet 包从 4.1.1 到最新稳定版（4.9.0+）
- **适配** 新包模型：`Emgu.CV` 变为 meta 包，需额外引入 `Emgu.CV.runtime.windows`
- **替换** 所有 `new Image<>(Bitmap)` 构造函数调用为 `Bitmap.ToImage<>()` 扩展方法（14 处）
- **启用** AKAZE 浮点描述符（`AKAZE.DescriptorType.KAZE`）用于 FeatureMatching
- **BREAKING**: Emgu.CV 包结构变化，依赖此包的其他项目需同步升级

## Capabilities

### New Capabilities
- `emgucv-upgrade`: 升级 Emgu.CV 到最新版本，启用 AKAZE 浮点描述符特征匹配

### Modified Capabilities
- `image-match-mode`: FeatureMatching 模式从 KAZE 切换到 AKAZE（浮点描述符），缩放匹配率提升

## Impact

- `OpenRPA.Image/OpenRPA.Image.csproj`: Emgu.CV 版本号 + 新增 runtime 包引用
- `OpenRPA.Image/Matches.cs`: 8 处 `new Image<>()` → `.ToImage<>()`
- `OpenRPA.Image/getrectangle.cs`: 3 处 `new Image<>()` → `.ToImage<>()`
- `OpenRPA.Image/ImageElement.cs`: 1 处 Emgu.CV Image 构造
- `OpenRPA.Image/Activities/GetText.cs`: 2 处 Emgu.CV Image 构造
- 可能影响其他引用 `Emgu.CV.World.dll` 的项目（需全局 restore 验证）
