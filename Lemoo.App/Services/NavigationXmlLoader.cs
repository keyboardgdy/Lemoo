using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Lemoo.App.Models;

namespace Lemoo.App.Services;

/// <summary>
/// 导航 XML 加载器：从 XML 文件加载导航结构
/// </summary>
public class NavigationXmlLoader
{
    /// <summary>
    /// 从 XML 文件加载导航项
    /// </summary>
    public static (ObservableCollection<NavigationItem> navigationItems, ObservableCollection<NavigationItem> bottomNavigationItems) LoadFromXml(string xmlPath)
    {
        if (!File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"导航配置文件未找到: {xmlPath}");
        }

        var doc = XDocument.Load(xmlPath);
        var root = doc.Root ?? throw new InvalidOperationException("XML 文件格式错误：缺少根元素");

        var navigationItems = new ObservableCollection<NavigationItem>();
        var bottomNavigationItems = new ObservableCollection<NavigationItem>();

        // 加载主导航项
        var navItemsElement = root.Element("NavigationItems");
        if (navItemsElement != null)
        {
            foreach (var itemElement in navItemsElement.Elements("Item"))
            {
                var navItem = ParseNavigationItem(itemElement);
                if (navItem != null)
                {
                    navigationItems.Add(navItem);
                }
            }
        }

        // 加载底部导航项
        var bottomNavItemsElement = root.Element("BottomNavigationItems");
        if (bottomNavItemsElement != null)
        {
            foreach (var itemElement in bottomNavItemsElement.Elements("Item"))
            {
                var navItem = ParseNavigationItem(itemElement);
                if (navItem != null)
                {
                    bottomNavigationItems.Add(navItem);
                }
            }
        }

        return (navigationItems, bottomNavigationItems);
    }

    /// <summary>
    /// 解析单个导航项（递归处理子项）
    /// </summary>
    private static NavigationItem? ParseNavigationItem(XElement element)
    {
        var title = element.Attribute("Title")?.Value;
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var icon = element.Attribute("Icon")?.Value ?? string.Empty;
        // 处理 HTML 实体编码（如 &#xE8A5; 转换为实际字符）
        icon = System.Net.WebUtility.HtmlDecode(icon);
        
        var pageKey = element.Attribute("PageKey")?.Value ?? string.Empty;
        var pageType = element.Attribute("PageType")?.Value ?? string.Empty;
        var isExpanded = element.Attribute("IsExpanded")?.Value == "true";
        var isEnabled = element.Attribute("IsEnabled")?.Value != "false"; // 默认为 true

        var navItem = new NavigationItem(title, icon, pageKey, pageType)
        {
            IsExpanded = isExpanded,
            IsEnabled = isEnabled
        };

        // 递归解析子项
        var childrenElement = element.Element("Children");
        if (childrenElement != null)
        {
            foreach (var childElement in childrenElement.Elements("Item"))
            {
                var childItem = ParseNavigationItem(childElement);
                if (childItem != null)
                {
                    navItem.Children.Add(childItem);
                }
            }
        }

        return navItem;
    }
}

