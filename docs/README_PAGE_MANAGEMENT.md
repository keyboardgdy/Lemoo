# 页面管理方案

## 概述

本方案提供了模块化的页面管理机制，支持：
- **自动发现**：使用特性（Attribute）标记页面，自动发现和注册
- **模块化组织**：按功能模块组织页面到不同文件夹
- **集中管理**：通过 `PageRegistry` 统一管理所有页面
- **灵活配置**：支持特性标记和 XML 配置两种方式

## 使用方法

### 1. 使用特性标记页面

在页面类上添加 `[Page]` 特性：

```csharp
using System.Windows.Controls;
using Lemoo.App.Attributes;

namespace Lemoo.App.Views.Pages;

[Page("Home", Title = "首页", Icon = "\uE80F", Module = "文件管理")]
public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }
}
```

### 2. 特性参数说明

- `PageKey`（必需）：页面的唯一标识符
- `Title`（可选）：页面显示标题，默认使用类名（去掉 "Page" 后缀）
- `Icon`（可选）：页面图标（Segoe MDL2 Assets 字符）
- `Module`（可选）：页面所属模块，用于组织管理
- `IsEnabled`（可选）：是否默认启用，默认为 `true`
- `Description`（可选）：页面描述

### 3. 模块化文件夹结构

建议按模块组织页面：

```
Views/
  Pages/
    FileManagement/          # 文件管理模块
      HomePage.xaml
      HomePage.xaml.cs
      DocumentPage.xaml
      DocumentPage.xaml.cs
      ProjectPage.xaml
      ProjectPage.xaml.cs
      ...
    Collaboration/          # 协作模块
      TeamPage.xaml
      TeamPage.xaml.cs
      ...
    Analytics/              # 分析模块
      AnalyticsPage.xaml
      AnalyticsPage.xaml.cs
      ...
    Settings/               # 设置模块
      SettingsPage.xaml
      SettingsPage.xaml.cs
      ...
```

### 4. 命名空间建议

使用模块化的命名空间：

```csharp
namespace Lemoo.App.Views.Pages.FileManagement;

[Page("Home", Title = "首页", Module = "文件管理")]
public partial class HomePage : Page
{
    // ...
}
```

### 5. 访问页面注册表

在需要的地方注入 `PageRegistry` 服务：

```csharp
public class SomeService
{
    private readonly PageRegistry _pageRegistry;

    public SomeService(PageRegistry pageRegistry)
    {
        _pageRegistry = pageRegistry;
    }

    public void DoSomething()
    {
        // 获取所有启用的页面
        var enabledPages = _pageRegistry.GetEnabledPages();
        
        // 按模块获取页面
        var fileManagementPages = _pageRegistry.GetPagesByModule("文件管理");
        
        // 获取特定页面的元数据
        var homePageMetadata = _pageRegistry.GetPageMetadata("Home");
    }
}
```

## 自动生成导航结构

`NavigationService` 现在支持从 `PageRegistry` 自动生成导航结构：

- **自动按模块组织**：根据页面的 `Module` 属性自动分组
- **智能布局**：
  - 单个页面的模块：直接添加到导航项
  - 多个页面的模块：创建父级项，包含所有子页面
  - 系统模块：自动添加到底部导航
- **XML 优先**：如果 XML 配置文件存在，优先使用 XML 配置
- **自动回退**：如果 XML 不存在或加载失败，自动从 `PageRegistry` 生成

### 使用自动生成

只需确保所有页面都标记了 `[Page]` 特性，`NavigationService` 会在以下情况自动生成导航结构：

1. XML 配置文件不存在
2. XML 配置文件为空
3. XML 配置文件加载失败

## 优势

1. **自动发现**：无需手动注册，添加页面特性即可自动发现
2. **自动生成导航**：根据页面特性自动生成导航结构，无需手动配置 XML
3. **模块化管理**：通过 `Module` 参数和文件夹结构组织页面
4. **类型安全**：编译时检查，避免运行时错误
5. **易于维护**：页面信息集中在特性中，易于查找和修改
6. **向后兼容**：仍然支持 XML 配置方式，XML 优先
7. **智能回退**：XML 不可用时自动从特性生成

## 迁移指南

### 从硬编码迁移

**之前：**
```csharp
page = pageKey switch
{
    "Home" => new Views.Pages.HomePage(),
    "Document" => new Views.Pages.DocumentPage(),
    // ...
};
```

**现在：**
```csharp
// 只需在页面类上添加特性，PageFactory 会自动处理
[Page("Home", Title = "首页", Module = "文件管理")]
public partial class HomePage : Page { }
```

### 从 XML 配置迁移

XML 配置仍然有效，但建议逐步迁移到特性方式：

**XML 方式（仍然支持）：**
```xml
<Item Title="首页" Icon="&#xE80F;" PageKey="Home" PageType="Lemoo.App.Views.Pages.HomePage" />
```

**特性方式（推荐）：**
```csharp
[Page("Home", Title = "首页", Icon = "\uE80F")]
public partial class HomePage : Page { }
```

## 最佳实践

1. **使用有意义的 PageKey**：使用清晰、唯一的标识符
2. **按模块组织**：将相关页面放在同一文件夹和模块中
3. **提供完整元数据**：尽量填写 Title、Icon、Module 等信息
4. **保持一致性**：同一模块的页面使用相同的命名规范
5. **文档化**：使用 Description 参数记录页面用途
6. **优先使用特性**：新页面使用特性标记，减少 XML 配置维护工作
7. **模块命名规范**：使用统一的模块名称（如"文件管理"、"协作"、"系统"等）

## 模块化文件夹结构示例

虽然当前所有页面仍在 `Views/Pages/` 目录下，但建议按以下结构组织：

```
Views/
  Pages/
    FileManagement/          # 文件管理模块
      HomePage.xaml
      HomePage.xaml.cs       # [Page("Home", Module = "文件管理")]
      DocumentPage.xaml
      DocumentPage.xaml.cs   # [Page("Document", Module = "文件管理")]
      ProjectPage.xaml
      ProjectPage.xaml.cs    # [Page("Project", Module = "文件管理")]
      ...
    Collaboration/           # 协作模块
      TeamPage.xaml
      TeamPage.xaml.cs      # [Page("Team", Module = "协作")]
      ...
    Analytics/              # 分析模块
      AnalyticsPage.xaml
      AnalyticsPage.xaml.cs # [Page("Analytics", Module = "视图分析")]
      ...
    Settings/               # 设置模块
      SettingsPage.xaml
      SettingsPage.xaml.cs  # [Page("Settings", Module = "系统")]
      ...
```

### 迁移到模块化结构

1. 创建模块文件夹（如 `FileManagement/`）
2. 移动相关页面文件到对应文件夹
3. 更新命名空间（可选，但推荐）：
   ```csharp
   namespace Lemoo.App.Views.Pages.FileManagement;
   ```
4. 更新 XAML 中的命名空间引用（如果更改了命名空间）
5. 确保 `[Page]` 特性中的 `Module` 参数正确

## 示例

### 完整示例

```csharp
using System.Windows.Controls;
using Lemoo.App.Attributes;

namespace Lemoo.App.Views.Pages.FileManagement;

[Page(
    "Document", 
    Title = "文档管理", 
    Icon = "\uE8A5", 
    Module = "文件管理",
    Description = "管理和编辑文档",
    IsEnabled = true
)]
public partial class DocumentPage : Page
{
    public DocumentPage()
    {
        InitializeComponent();
    }
}
```

