## Context

`GetElement`（Image）目前用 `Processname`（进程名）+ `Limit`（搜索区域限制）限定图像搜索范围。底层通过 `MyEnumWindows.WindowRects` 枚举进程所有窗口句柄（可达 1960 个），经大小过滤和去重后得到搜索矩形。该机制存在去重 bug（会错误删除主窗口）且交互体验差。

OpenRPA 已有成熟的 Selector 选择器体系：`SelectorWindow` 弹框 → 鼠标选中窗口/组件 → 自动捕获 `Selector` JSON 字符串。`WindowsSelector(selector)` 可解析该字符串并返回目标元素的 `Rectangle`。现有 `ClickElement`、`Windows.GetElement` 等十余个活动均依赖此机制。

本设计将 Selector 引入 Image.GetElement，用 Selector 解析出的窗口矩形替代 Processname + WindowRects 枚举。

## Goals / Non-Goals

**Goals:**
- 为 Image.GetElement 新增 `Selector` 属性，复用标准 Selector 弹框选择目标窗口
- 运行时通过 Selector 解析窗口矩形作为图像搜索区域
- 移除 `Processname`、`Limit` 属性及相关按钮
- 保留无 Selector 时的全屏搜索行为

**Non-Goals:**
- 不修改 Selector/SelectorWindow 本身
- 不修改 ImageEvent 的轮询/匹配逻辑（仅改搜索区域获取方式）
- 不修改录制流程 (`Plugin.cs`)

## Decisions

### 1. Selector 属性格式

使用 `InArgument<string>`（与 Windows.GetElement 一致），存储 Selector JSON 字符串。

```csharp
public InArgument<string> Selector { get; set; }
```

### 2. Selector 选择器按钮

复用 `SelectorWindow` 弹框，传入 `"Windows"` 插件名（与 Windows.GetElement 的 Open_Selector 逻辑一致）。用户点击后弹出标准选择器，鼠标悬停高亮窗口，点击确认后 Selector JSON 写入属性。

### 3. 运行时搜索区域解析

在 `findMatch` 中，当 `Selector` 不为空时：

```
Selector JSON → new WindowsSelector(selector) → SelectorItem[] → 取第一个元素的 Rectangle
```

替代原有的 `WindowRects` 枚举 → 多个矩形 → 逐个搜索 的流程。

解析后的 `Rectangle` 直接作为截图搜索区域。

### 4. 原有 Processname/Limit 路径完全删除

- 移除 `GetElement.Processname` 属性
- 移除 `GetElement.Limit` 属性
- 移除 `ImageEvent.findMatch` 中的 Processname 分支
- 移除设计师中的 Processname 按钮

### 5. 向后兼容

- 无 Selector 时 `findMatch` 回退到全屏搜索（行为不变）
- 已有工作流中的 Processname 和 Limit 属性将被忽略（不崩溃，但不起作用）

## Risks / Trade-offs

- **BREAKING**: 现有使用 Processname/Limit 的工作流需人工迁移 → Mitigation: 迁移简单——删除旧属性，打开 Selector 弹框点一下目标窗口即可
- **依赖 Windows 插件**: Selector 解析依赖 OpenRPA.Windows 的 `WindowsSelector` → 已存在的依赖（Image 项目引用 Interfaces，Interfaces 包含 Selector 基础设施）
