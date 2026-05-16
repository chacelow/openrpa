# splash-screen Specification

## Purpose

启动时显示带 logo 和加载状态的 Splash 窗口，加载完成后自动关闭。

## ADDED Requirements

### Requirement: Splash window displays on startup

应用启动时 SHALL 先显示 SplashWindow，包含 OpenRPA logo 和 "正在加载..." 文字。窗口 MUST 无边框、居中、置顶、不在任务栏显示。

#### Scenario: Splash appears before main window

- **WHEN** 用户双击启动 OpenRPA
- **THEN** Splash 窗口先出现，主窗口在加载完成后才显示

### Requirement: Loading status updates during initialization

加载插件和初始化过程中，Splash SHALL 显示当前加载的模块名称。状态更新 MUST 通过 `Dispatcher.Invoke` 保证线程安全。

#### Scenario: Module name shown during plugin load

- **WHEN** 系统正在加载插件 "OpenRPA.Image"
- **THEN** Splash 显示文字 "加载插件: OpenRPA.Image"

### Requirement: Splash auto-closes on completion

所有模块加载完成后，Splash SHALL 自动关闭并显示主窗口。

#### Scenario: Splash disappears when loading done

- **WHEN** `MainWindow.Loaded` 事件触发
- **THEN** Splash 窗口关闭
