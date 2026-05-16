## 1. NuGet 包升级

- [ ] 1.1 修改 `OpenRPA.Image.csproj`：Emgu.CV 版本 → `4.9.0.*`，新增 `Emgu.CV.runtime.windows` 包引用
- [ ] 1.2 `dotnet restore` 验证包下载成功
- [ ] 1.3 编译检查，记录所有 CS 错误

## 2. API 迁移 — Image 构造

- [ ] 2.1 `Matches.cs`：8 处 `new Image<>(Bitmap)` → `Bitmap.ToImage<>()`
- [ ] 2.2 `getrectangle.cs`：3 处 `new Image<>(Bitmap)` → `Bitmap.ToImage<>()`
- [ ] 2.3 `ImageElement.cs`：Emgu.CV Image 构造适配（如有）
- [ ] 2.4 `GetText.cs` / `ocr.cs`：OCR 相关 Image 构造适配（如有）

## 3. AKAZE 启用

- [ ] 3.1 `Matches.cs` `FindFeatureMatches`：KAZE → AKAZE + 浮点描述符
- [ ] 3.2 验证 FLANN KdTree 匹配器与 AKAZE 浮点描述符兼容
- [ ] 3.3 缩放场景手动测试：匹配率应比 KAZE 提升

## 4. Tesseract OCR 验证

- [ ] 4.1 编译检查 OCR 相关 API 是否变化
- [ ] 4.2 按需调整 Tesseract 构造函数或方法调用

## 5. 全局构建验证

- [ ] 5.1 构建整个 Solution，修复其他项目中对 Emgu.CV 的引用问题（如有）
- [ ] 5.2 确认运行时 `cvextern.dll` 正确部署
- [ ] 5.3 手动测试：模板匹配、轮廓检测、截图功能正常
