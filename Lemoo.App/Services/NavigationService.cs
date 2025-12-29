using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Lemoo.App.Models;

namespace Lemoo.App.Services;

/// <summary>
/// 导航服务：管理导航项和菜单项数据
/// </summary>
public class NavigationService
{
    private readonly ObservableCollection<NavigationItem> _navigationItems = new();
    private readonly ObservableCollection<NavigationItem> _bottomNavigationItems = new();
    private readonly ObservableCollection<MenuItemData> _menuItems = new();
    private readonly PageRegistry? _pageRegistry;

    /// <summary>
    /// 导航项集合
    /// </summary>
    public ObservableCollection<NavigationItem> NavigationItems => _navigationItems;

    /// <summary>
    /// 底部导航项集合
    /// </summary>
    public ObservableCollection<NavigationItem> BottomNavigationItems => _bottomNavigationItems;

    /// <summary>
    /// 菜单项集合
    /// </summary>
    public ObservableCollection<MenuItemData> MenuItems => _menuItems;

    public NavigationService(PageRegistry? pageRegistry = null)
    {
        _pageRegistry = pageRegistry;
        LoadNavigationFromXml();
        InitializeMenuItems();
    }

    /// <summary>
    /// 从 PageRegistry 自动生成导航结构（按模块组织）
    /// </summary>
    public void GenerateNavigationFromPageRegistry()
    {
        if (_pageRegistry == null) return;

        _navigationItems.Clear();
        _bottomNavigationItems.Clear();

        // 按模块分组页面
        var pagesByModule = _pageRegistry.GetEnabledPages()
            .GroupBy(p => p.Module)
            .OrderBy(g => g.Key);

        foreach (var moduleGroup in pagesByModule)
        {
            var moduleName = string.IsNullOrWhiteSpace(moduleGroup.Key) ? "其他" : moduleGroup.Key;
            var modulePages = moduleGroup.OrderBy(p => p.Title).ToList();

            // 如果模块只有一个页面，直接添加到导航项
            if (modulePages.Count == 1)
            {
                var page = modulePages[0];
                var navItem = CreateNavigationItemFromPageMetadata(page);
                if (navItem != null)
                {
                    // 系统模块的页面添加到底部导航
                    if (moduleName == "系统")
                    {
                        _bottomNavigationItems.Add(navItem);
                    }
                    else
                    {
                        _navigationItems.Add(navItem);
                    }
                }
            }
            else
            {
                // 多个页面，创建父级项
                var parentItem = new NavigationItem(moduleName, GetModuleIcon(moduleName))
                {
                    IsExpanded = true,
                    IsEnabled = true
                };

                foreach (var page in modulePages)
                {
                    var navItem = CreateNavigationItemFromPageMetadata(page);
                    if (navItem != null)
                    {
                        parentItem.Children.Add(navItem);
                    }
                }

                if (parentItem.HasChildren)
                {
                    // 系统模块的页面添加到底部导航
                    if (moduleName == "系统")
                    {
                        _bottomNavigationItems.Add(parentItem);
                    }
                    else
                    {
                        _navigationItems.Add(parentItem);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 从页面元数据创建导航项
    /// </summary>
    private NavigationItem? CreateNavigationItemFromPageMetadata(PageMetadata metadata)
    {
        if (metadata == null || !metadata.IsEnabled) return null;

        var navItem = new NavigationItem(metadata.Title, metadata.Icon, metadata.PageKey, metadata.PageType.FullName ?? string.Empty)
        {
            IsEnabled = metadata.IsEnabled
        };

        return navItem;
    }

    /// <summary>
    /// 获取模块图标
    /// </summary>
    private string GetModuleIcon(string moduleName)
    {
        return moduleName switch
        {
            "文件管理" => "\uE8A5",
            "协作" => "\uE716",
            "视图分析" => "\uE9D2",
            "系统" => "\uE713",
            _ => "\uE8A5"
        };
    }

    /// <summary>
    /// 从 XML 文件加载导航结构
    /// </summary>
    private void LoadNavigationFromXml()
    {
        try
        {
            // 获取 XML 文件路径
            var xmlPath = GetNavigationConfigPath();
            
            // 如果 XML 文件不存在，尝试从 PageRegistry 自动生成
            if (!File.Exists(xmlPath) && _pageRegistry != null)
            {
                System.Diagnostics.Debug.WriteLine("XML 配置文件不存在，从 PageRegistry 自动生成导航结构");
                GenerateNavigationFromPageRegistry();
                return;
            }
            
            // 从 XML 加载导航项
            var (navigationItems, bottomNavigationItems) = NavigationXmlLoader.LoadFromXml(xmlPath);
            
            // 批量填充集合（减少通知次数，提高性能）
            _navigationItems.Clear();
            foreach (var item in navigationItems)
            {
                _navigationItems.Add(item);
            }
            
            _bottomNavigationItems.Clear();
            foreach (var item in bottomNavigationItems)
            {
                _bottomNavigationItems.Add(item);
            }

            // 如果 XML 中没有配置，但 PageRegistry 中有页面，则补充
            if (_pageRegistry != null && _navigationItems.Count == 0 && _bottomNavigationItems.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("XML 配置为空，从 PageRegistry 自动生成导航结构");
                GenerateNavigationFromPageRegistry();
            }
        }
        catch (Exception ex)
        {
            // 如果加载失败，尝试从 PageRegistry 生成
            System.Diagnostics.Debug.WriteLine($"加载导航配置失败: {ex.Message}");
            if (_pageRegistry != null)
            {
                System.Diagnostics.Debug.WriteLine("尝试从 PageRegistry 自动生成导航结构");
                GenerateNavigationFromPageRegistry();
            }
            else
            {
                // 如果 PageRegistry 也不可用，使用默认配置
                InitializeNavigationItemsFallback();
                InitializeBottomNavigationItemsFallback();
            }
        }
    }


    /// <summary>
    /// 保存导航结构到 XML 文件
    /// </summary>
    public void SaveNavigationToXml()
    {
        try
        {
            var xmlPath = GetNavigationConfigPath();
            NavigationXmlSaver.SaveToXml(xmlPath, _navigationItems, _bottomNavigationItems);
            // 重新加载菜单项以反映更改
            RefreshMenuItems();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存导航配置失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 获取导航配置文件的路径
    /// </summary>
    private string GetNavigationConfigPath()
    {
        // 优先从应用程序基目录查找（运行时，XML 文件会被复制到输出目录）
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var configPath = Path.Combine(baseDirectory, "Config", "NavigationConfig.xml");
        if (File.Exists(configPath))
        {
            return configPath;
        }

        // 尝试从程序集所在目录查找
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            if (assemblyDirectory != null)
            {
                configPath = Path.Combine(assemblyDirectory, "Config", "NavigationConfig.xml");
                if (File.Exists(configPath))
                {
                    return configPath;
                }
            }
        }

        // 尝试从项目源目录查找（开发时）
        var currentDir = new DirectoryInfo(baseDirectory);
        while (currentDir != null && currentDir.Parent != null)
        {
            configPath = Path.Combine(currentDir.FullName, "Config", "NavigationConfig.xml");
            if (File.Exists(configPath))
            {
                return configPath;
            }
            currentDir = currentDir.Parent;
        }

        // 如果都找不到，返回默认路径（会触发回退方案）
        return Path.Combine(baseDirectory, "Config", "NavigationConfig.xml");
    }

    /// <summary>
    /// 初始化导航项（回退方案，当 XML 加载失败时使用）
    /// </summary>
    private void InitializeNavigationItemsFallback()
    {
        // 文件管理组
        var fileGroup = new NavigationItem("文件管理", "\uE8A5");
        fileGroup.Children.Add(new NavigationItem("首页", "\uE80F", "Home"));
        fileGroup.Children.Add(new NavigationItem("文档", "\uE8A5", "Document"));
        fileGroup.Children.Add(new NavigationItem("项目", "\uE8B7", "Project"));
        fileGroup.Children.Add(new NavigationItem("任务", "\uE7C3", "Task"));
        fileGroup.Children.Add(new NavigationItem("日历", "\uE787", "Calendar"));
        fileGroup.Children.Add(new NavigationItem("笔记", "\uE8A5", "Note"));
        fileGroup.Children.Add(new NavigationItem("归档", "\uE7B8", "Archive"));
        fileGroup.IsExpanded = true;
        _navigationItems.Add(fileGroup);

        // 协作组
        var collaborateGroup = new NavigationItem("协作", "\uE716");
        collaborateGroup.Children.Add(new NavigationItem("团队", "\uE716", "Team"));
        collaborateGroup.IsExpanded = true;
        _navigationItems.Add(collaborateGroup);

        // 视图分析组
        var viewGroup = new NavigationItem("视图分析", "\uE9D2");
        viewGroup.Children.Add(new NavigationItem("分析", "\uE9D2", "Analytics"));
        viewGroup.IsExpanded = true;
        _navigationItems.Add(viewGroup);
    }

    /// <summary>
    /// 初始化底部导航项（回退方案）
    /// </summary>
    private void InitializeBottomNavigationItemsFallback()
    {
        _bottomNavigationItems.Add(new NavigationItem("设置", "\uE713", "Settings"));
    }

    /// <summary>
    /// 初始化菜单项（从导航项自动生成，保持一致）
    /// </summary>
    private void InitializeMenuItems()
    {
        // 从导航项生成菜单项，保持数据一致（只包含启用的项）
        foreach (var navItem in NavigationItems)
        {
            if (navItem.IsEnabled)
            {
                var menuItem = ConvertNavigationItemToMenuItemData(navItem);
                if (menuItem != null)
                {
                    _menuItems.Add(menuItem);
                }
            }
        }
    }

    /// <summary>
    /// 重新加载菜单项（当导航项更新时调用）
    /// </summary>
    public void RefreshMenuItems()
    {
        _menuItems.Clear();
        InitializeMenuItems();
    }

    /// <summary>
    /// 将 NavigationItem 转换为 MenuItemData（只包含启用的项）
    /// </summary>
    private MenuItemData? ConvertNavigationItemToMenuItemData(NavigationItem navItem)
    {
        // 只处理启用的项
        if (!navItem.IsEnabled)
        {
            return null;
        }

        var menuItem = new MenuItemData(navItem.Title, navItem.Icon, navItem.PageKey);
        // 注意：MenuItemData 不需要 PageType，因为它只用于菜单显示
        
        // 递归转换子项（只包含启用的子项）
        foreach (var child in navItem.Children)
        {
            var childMenuItem = ConvertNavigationItemToMenuItemData(child);
            if (childMenuItem != null)
            {
                menuItem.Children.Add(childMenuItem);
            }
        }
        
        return menuItem;
    }
}

