## Context

OpenRPA 是 WPF 应用，启动流程：`App.xaml.cs` → `LoadPlugins` → `MainWindow.xaml`。当前 `App.xaml.cs:274-293` 有注释掉的 splash 代码引用 `splash.BusyContent` 但变量 `splash` 不存在。已有 `OpenRPA-logo.png` 可用于 splash。

## Goals / Non-Goals

**Goals:**
- 启动时先显示 SplashWindow（无边框、居中、置顶）
- 显示 OpenRPA logo + "正在加载..." + 模块名称
- 加载完成后自动关闭
- 启动速度快，splash 不阻塞主线程

**Non-Goals:**
- 不添加启动动画视频
- 不添加启动参数（/nosplash 等）
- 不修改加载流程本身（仅加状态回调）

## Decisions

### 1. 窗口实现

```xml
<Window WindowStyle="None" AllowsTransparency="True" 
        Background="Transparent" Topmost="True"
        WindowStartupLocation="CenterScreen"
        ShowInTaskbar="False">
    <Border CornerRadius="12" Background="#F0F0F0" Margin="20">
        <StackPanel Width="420">
            <Image Source="logo" Height="80" Margin="0,30,0,10"/>
            <TextBlock Text="OpenRPA" FontSize="24" HorizontalAlignment="Center"/>
            <TextBlock Name="StatusText" Text="正在加载..." HorizontalAlignment="Center" 
                       Foreground="#666" Margin="0,10,0,30"/>
        </StackPanel>
    </Border>
</Window>
```

### 2. 加载流程

```
App.OnStartup()
  → splash = new SplashWindow()
  → splash.Show()
  → LoadPlugins() { splash.Status = "加载插件: xxx" }
  → MainWindow.Loaded { splash.Close() }
```

### 3. 线程安全

Splash 必须在 UI 线程显示，状态更新通过 `Dispatcher.Invoke`。
