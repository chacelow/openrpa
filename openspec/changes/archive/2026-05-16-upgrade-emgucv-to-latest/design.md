## Context

OpenRPA.Image 项目当前使用 Emgu.CV 4.1.1.3497（`World.dll` 统一包模型）。Emgu.CV 从 4.2 开始拆分为 meta 包 + 平台 runtime 包。最新稳定版（4.9+）对 AKAZE 特征检测器有完整支持，且 `Bitmap.ToImage<>()` 扩展方法可用于替代已移除的 `Image(Bitmap)` 构造函数。

## Goals / Non-Goals

**Goals:**
- 升级到 Emgu.CV 最新稳定版（优先 4.9.0，回退 4.8.1）
- 保持所有现有功能（模板匹配、OCR、轮廓检测、截图）正常工作
- FeatureMatching 切换到 AKAZE 浮点描述符

**Non-Goals:**
- 不修改 `Matches.FindMatch(Mat, Mat, ...)` 旧版 KAZE 重载（保持向后兼容）
- 不升级其他 NuGet 依赖

## Decisions

### 1. 目标版本：4.9.0（优先），4.8.1（回退）

4.9.0 是最新稳定版。如遇 API 兼容问题则降至 4.8.1（已验证 nuget 缓存中有此版本）。

### 2. 包模型迁移

```
Before: Emgu.CV 4.1.1 (World.dll 统一模型)
After:  Emgu.CV 4.9.0 + Emgu.CV.runtime.windows (meta + runtime)
```

`Emgu.CV.runtime.windows` 提供 Windows 原生运行时（`cvextern.dll`），`Emgu.CV` 提供托管封装。

### 3. Image 构造 API 迁移

| 旧 (4.1.1) | 新 (4.9) |
|------------|---------|
| `new Image<Bgr, byte>(bitmap)` | `bitmap.ToImage<Bgr, byte>()` |
| `new Image<Gray, byte>(bitmap)` | `bitmap.ToImage<Gray, byte>()` |
| `new Image<Bgr, Byte>(bitmap)` | `bitmap.ToImage<Bgr, Byte>()` |

扩展方法在 `Emgu.CV` 命名空间下（`BitmapExtension` 类），现有 `using Emgu.CV;` 已涵盖。

### 4. AKAZE 配置

```csharp
var featureDetector = new AKAZE(AKAZE.DescriptorType.KAZE);
```

使用 `DescriptorType.KAZE` 生成浮点描述符，与 FLANN（KdTree）匹配器兼容。旧版 `FindMatch` 方法保留 KAZE 不变。

### 5. Tesseract OCR 兼容性

OCR 功能使用 `Emgu.CV.OCR.Tesseract`，在新版中 API 可能略有变化。需验证 `SetImage`、`Recognize`、`GetCharacters` 等方法签名。

## Risks / Trade-offs

- **包模型变化影响全局**: 任何引用 `Emgu.CV.World.dll` 的项目需同步升级 → Mitigation: 构建整个解决方案，逐个修复
- **Tesseract API 变化**: 4.9 中 Tesseract API 可能有签名变化 → Mitigation: 优先编译，按错误调整
- **运行时 DLL 缺失**: `Emgu.CV.runtime.windows` 需确保 `cvextern.dll` 正确部署 → Mitigation: NuGet 自动处理复制到输出目录
