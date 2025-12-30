using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Lemoo.App.Helper.Attributes;

namespace Lemoo.App.Services;

/// <summary>
/// 页面注册表：自动发现和注册所有页面
/// </summary>
public class PageRegistry
{
    private readonly Dictionary<string, PageMetadata> _pages = new();
    private readonly Dictionary<string, Type> _pageTypes = new();

    /// <summary>
    /// 所有已注册的页面元数据
    /// </summary>
    public IReadOnlyDictionary<string, PageMetadata> Pages => _pages;

    /// <summary>
    /// 初始化页面注册表（自动发现所有页面）
    /// </summary>
    public void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var pageTypes = assembly.GetTypes()
            .Where(t => typeof(Page).IsAssignableFrom(t) && 
                       !t.IsAbstract && 
                       t.GetCustomAttribute<PageAttribute>() != null)
            .ToList();

        foreach (var pageType in pageTypes)
        {
            var attribute = pageType.GetCustomAttribute<PageAttribute>();
            if (attribute == null) continue;

            var metadata = new PageMetadata
            {
                PageKey = attribute.PageKey,
                Title = string.IsNullOrWhiteSpace(attribute.Title) ? pageType.Name.Replace("Page", "") : attribute.Title,
                Icon = attribute.Icon,
                Module = attribute.Module,
                IsEnabled = attribute.IsEnabled,
                Description = attribute.Description,
                PageType = pageType
            };

            _pages[attribute.PageKey] = metadata;
            _pageTypes[attribute.PageKey] = pageType;
        }
    }

    /// <summary>
    /// 根据 PageKey 获取页面类型
    /// </summary>
    public Type? GetPageType(string pageKey)
    {
        return _pageTypes.TryGetValue(pageKey, out var type) ? type : null;
    }

    /// <summary>
    /// 根据 PageKey 获取页面元数据
    /// </summary>
    public PageMetadata? GetPageMetadata(string pageKey)
    {
        return _pages.TryGetValue(pageKey, out var metadata) ? metadata : null;
    }

    /// <summary>
    /// 根据模块获取所有页面
    /// </summary>
    public IEnumerable<PageMetadata> GetPagesByModule(string module)
    {
        return _pages.Values.Where(p => p.Module == module);
    }

    /// <summary>
    /// 获取所有启用的页面
    /// </summary>
    public IEnumerable<PageMetadata> GetEnabledPages()
    {
        return _pages.Values.Where(p => p.IsEnabled);
    }
}

/// <summary>
/// 页面元数据
/// </summary>
public class PageMetadata
{
    public string PageKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public Type PageType { get; set; } = null!;
}

