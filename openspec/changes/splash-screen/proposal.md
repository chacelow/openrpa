## Why

OpenRPA 启动时从双击到主窗口出现有 3-5 秒空白等待，用户无感知——不知道正在加载插件还是卡死了。PS/AE 等专业软件有 splash screen 展示 logo + 加载进度，既提升体验又能诊断启动问题。已有注释掉的 `splash.BusyContent` 代码，只需实现完整 splash 窗口。

## What Changes

- 新增 `SplashWindow` WPF 窗口，显示 OpenRPA logo + 加载状态文字
- 启动时先显示 splash，加载插件/初始化过程中更新状态
- 所有模块加载完成后自动关闭 splash，显示主窗口
- **BREAKING**: 无，纯新增功能

## Capabilities

### New Capabilities
- `splash-screen`: 启动画面，显示 logo 和加载进度

## Impact

- `OpenRPA/SplashWindow.xaml` + `.xaml.cs`：新增 splash 窗口
- `OpenRPA/App.xaml.cs`：修改启动流程，先显示 splash 再初始化
- `OpenRPA/OpenRPA.csproj`：新增编译文件
- `OpenRPA/Resources/`：可能需要新增大尺寸 logo 图片
