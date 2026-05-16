## 1. Splash 窗口创建

- [ ] 1.1 新建 `OpenRPA/SplashWindow.xaml` — 无边框窗口 + logo + 状态文字
- [ ] 1.2 新建 `OpenRPA/SplashWindow.xaml.cs` — 公开 `SetStatus(string)` 方法
- [ ] 1.3 在 `OpenRPA.csproj` 中注册新文件

## 2. 启动流程集成

- [ ] 2.1 修改 `App.xaml.cs`：启动时先创建并显示 Splash
- [ ] 2.2 在加载插件循环中调用 `SplashWindow.SetStatus()`
- [ ] 2.3 在 `MainWindow.Loaded` 时关闭 Splash

## 3. 视觉效果

- [ ] 3.1 使用 `OpenRPA-logo.png` 作为 splash logo
- [ ] 3.2 添加圆角边框 + 半透明阴影
- [ ] 3.3 添加简单淡入效果

## 4. 构建验证

- [ ] 4.1 编译 0 errors
- [ ] 4.2 手动启动验证 splash 显示 → 加载状态更新 → 自动关闭
