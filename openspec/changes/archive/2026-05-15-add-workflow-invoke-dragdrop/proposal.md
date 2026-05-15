## Why

用户需要将已有的工作流作为子流程嵌入当前工作流中。Snippets 面板仅显示内置 ISnippet 插件，不包含用户自己的项目和工作流。

## What Changes

- Snippets 面板在加载内置片段的同时，扫描用户项目并将其工作流作为 ToolboxItemWrapper 添加到工具箱中
- 每个项目作为一个 ToolboxCategory，子工作流作为可拖拽的 ToolboxItem
- 移除 `toolbox.Categories.Count > 0` 守卫，允许刷新
- 不覆盖已有分类，只追加新分类

## Capabilities

### New Capabilities

- `workflow-invoke-dragdrop`: 用户项目工作流自动出现在 Snippets 面板，拖拽到设计器嵌入子流程

### Modified Capabilities

None.

## Impact

- `OpenRPA/Views/Snippets.xaml.cs`：`InitializeSnippets` 方法增加用户项目遍历逻辑
