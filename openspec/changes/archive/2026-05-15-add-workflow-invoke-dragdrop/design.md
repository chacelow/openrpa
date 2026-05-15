## Context

Snippets 面板使用 `DynamicActivityGenerator` 将 XAML 编译为 CLR Type，再包装为 `ToolboxItemWrapper` 添加到 ToolboxControl。工具箱原生支持拖拽到 WFDesigner。用户项目工作流同样保存为 XAML，可复用同一编译流程。

## Goals / Non-Goals

**Goals:**
- 用户项目工作流自动出现在 Snippets 面板
- 可直接拖拽到设计器中嵌入为子流程

**Non-Goals:**
- 不改变内置 ISnippet 插件加载逻辑
- 不在 OpenProject 页面添加拖拽（AvalonDock 跨面板限制）

## Decisions

### 复用 DynamicActivityGenerator 编译

每个用户工作流的 XAML 通过 `AppendSubWorkflowTemplate` 编译为 Type，然后包装进 ToolboxItemWrapper。

### 不覆盖已有分类

使用 `toolbox.Categories.Any(x => x.CategoryName == c.Key)` 检查，避免重复添加。

### 移除 Count > 0 守卫

原来的 `if (toolbox.Categories.Count > 0) return;` 阻止了刷新，改为每次重新加载。

## Risks / Trade-offs

- [Risk] 工作流 XAML 编译失败 → 跳过该工作流，记录错误日志
