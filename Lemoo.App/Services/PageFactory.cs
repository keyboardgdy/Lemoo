using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Lemoo.App.Models;

namespace Lemoo.App.Services;

/// <summary>
/// 页面工厂：根据 PageKey 或 PageType 动态创建页面实例
/// </summary>
public class PageFactory
{
    private readonly Dictionary<string, Type> _pageTypeCache = new();
    private readonly Dictionary<string, Page> _pageInstanceCache = new(); // 页面实例缓存
    private readonly Dictionary<string, Type> _typeNameCache = new(); // 类型名称查找缓存
    private readonly NavigationService _navigationService;
    private readonly PageRegistry _pageRegistry;

    public PageFactory(NavigationService navigationService, PageRegistry pageRegistry)
    {
        _navigationService = navigationService;
        _pageRegistry = pageRegistry;
        InitializePageTypeCache();
    }

    /// <summary>
    /// 初始化页面类型缓存（优先从页面注册表，然后从导航项）
    /// </summary>
    private void InitializePageTypeCache()
    {
        // 首先从页面注册表加载所有页面类型（自动发现）
        foreach (var pageMetadata in _pageRegistry.GetEnabledPages())
        {
            if (pageMetadata.PageType != null)
            {
                _pageTypeCache[pageMetadata.PageKey] = pageMetadata.PageType;
            }
        }

        // 然后从导航项中补充（兼容 XML 配置）
        ExtractPageTypes(_navigationService.NavigationItems);
        ExtractPageTypes(_navigationService.BottomNavigationItems);
    }

    /// <summary>
    /// 递归提取页面类型
    /// </summary>
    private void ExtractPageTypes(System.Collections.ObjectModel.ObservableCollection<NavigationItem> items)
    {
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.PageKey) && !string.IsNullOrEmpty(item.PageType))
            {
                try
                {
                    var pageType = GetPageType(item.PageType);
                    if (pageType != null && typeof(Page).IsAssignableFrom(pageType))
                    {
                        _pageTypeCache[item.PageKey] = pageType;
                    }
                }
                catch
                {
                    // 忽略无效的类型
                }
            }

            // 递归处理子项
            if (item.HasChildren)
            {
                ExtractPageTypes(item.Children);
            }
        }
    }

    /// <summary>
    /// 获取页面类型（支持多种查找方式，使用缓存优化性能）
    /// </summary>
    private Type? GetPageType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        // 首先检查缓存
        if (_typeNameCache.TryGetValue(typeName, out var cachedType))
        {
            return cachedType;
        }

        // 首先尝试直接获取类型
        var type = Type.GetType(typeName);
        if (type != null)
        {
            _typeNameCache[typeName] = type;
            return type;
        }

        // 如果失败，尝试从当前程序集中查找
        var assembly = Assembly.GetExecutingAssembly();
        type = assembly.GetType(typeName);
        if (type != null)
        {
            _typeNameCache[typeName] = type;
            return type;
        }

        // 尝试在所有已加载的程序集中查找（使用缓存）
        foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // 跳过系统程序集以提高性能
            var assemblyName = loadedAssembly.GetName().Name;
            if (assemblyName != null && 
                (assemblyName.StartsWith("System", StringComparison.Ordinal) ||
                 assemblyName.StartsWith("Microsoft", StringComparison.Ordinal) ||
                 assemblyName.StartsWith("mscorlib", StringComparison.Ordinal)))
            {
                continue;
            }

            type = loadedAssembly.GetType(typeName);
            if (type != null)
            {
                _typeNameCache[typeName] = type;
                return type;
            }
        }

        // 缓存 null 结果，避免重复查找
        _typeNameCache[typeName] = null!;
        return null;
    }

    /// <summary>
    /// 根据 PageKey 创建页面实例（使用缓存优化性能）
    /// </summary>
    public Page? CreatePage(string pageKey, bool useCache = true)
    {
        if (string.IsNullOrEmpty(pageKey))
        {
            return null;
        }

        // 如果启用缓存且缓存中有实例，直接返回（注意：对于需要状态的页面，可能需要每次都创建新实例）
        if (useCache && _pageInstanceCache.TryGetValue(pageKey, out var cachedPage))
        {
            return cachedPage;
        }

        Page? page = null;
        Type? pageType = null;

        // 优先从页面注册表获取
        pageType = _pageRegistry.GetPageType(pageKey);
        if (pageType != null)
        {
            try
            {
                page = Activator.CreateInstance(pageType) as Page;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建页面失败 {pageKey}: {ex.Message}");
                return null;
            }
        }
        else
        {
            // 从缓存中获取页面类型
            if (_pageTypeCache.TryGetValue(pageKey, out pageType))
            {
                try
                {
                    page = Activator.CreateInstance(pageType) as Page;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"创建页面失败 {pageKey}: {ex.Message}");
                    return null;
                }
            }
            else
            {
                // 如果缓存中没有，尝试从导航项中查找
                var navItem = FindNavigationItemByPageKey(pageKey);
                if (navItem != null && !string.IsNullOrEmpty(navItem.PageType))
                {
                    try
                    {
                        var type = GetPageType(navItem.PageType);
                        if (type != null && typeof(Page).IsAssignableFrom(type))
                        {
                            _pageTypeCache[pageKey] = type;
                            page = Activator.CreateInstance(type) as Page;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"创建页面失败 {pageKey}: {ex.Message}");
                    }
                }
            }
        }

        // 如果启用缓存且成功创建页面，缓存实例
        if (useCache && page != null)
        {
            _pageInstanceCache[pageKey] = page;
        }

        return page;
    }

    /// <summary>
    /// 清除页面实例缓存（当需要强制重新创建页面时调用）
    /// </summary>
    public void ClearPageInstanceCache()
    {
        _pageInstanceCache.Clear();
    }

    /// <summary>
    /// 从缓存中移除特定页面实例
    /// </summary>
    public void RemovePageInstanceFromCache(string pageKey)
    {
        _pageInstanceCache.Remove(pageKey);
    }

    /// <summary>
    /// 根据 PageKey 查找导航项
    /// </summary>
    private NavigationItem? FindNavigationItemByPageKey(string pageKey)
    {
        return FindNavigationItemByPageKey(_navigationService.NavigationItems, pageKey) ??
               FindNavigationItemByPageKey(_navigationService.BottomNavigationItems, pageKey);
    }

    /// <summary>
    /// 递归查找导航项
    /// </summary>
    private NavigationItem? FindNavigationItemByPageKey(System.Collections.ObjectModel.ObservableCollection<NavigationItem> items, string pageKey)
    {
        foreach (var item in items)
        {
            if (item.PageKey == pageKey)
            {
                return item;
            }

            if (item.HasChildren)
            {
                var found = FindNavigationItemByPageKey(item.Children, pageKey);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }
}

