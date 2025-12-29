using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Lemoo.App.Models;

namespace Lemoo.App.Services;

/// <summary>
/// 导航 XML 保存器：将导航结构保存到 XML 文件
/// </summary>
public class NavigationXmlSaver
{
    /// <summary>
    /// 保存导航项到 XML 文件
    /// </summary>
    public static void SaveToXml(
        string xmlPath,
        ObservableCollection<NavigationItem> navigationItems,
        ObservableCollection<NavigationItem> bottomNavigationItems)
    {
        var root = new XElement("Navigation");

        // 保存主导航项
        var navItemsElement = new XElement("NavigationItems");
        foreach (var item in navigationItems)
        {
            navItemsElement.Add(CreateItemElement(item));
        }
        root.Add(navItemsElement);

        // 保存底部导航项
        var bottomNavItemsElement = new XElement("BottomNavigationItems");
        foreach (var item in bottomNavigationItems)
        {
            bottomNavItemsElement.Add(CreateItemElement(item));
        }
        root.Add(bottomNavItemsElement);

        // 创建 XML 文档
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);

        // 确保目录存在
        var directory = Path.GetDirectoryName(xmlPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 保存文件
        doc.Save(xmlPath);
    }

    /// <summary>
    /// 创建导航项的 XML 元素（递归处理子项）
    /// </summary>
    private static XElement CreateItemElement(NavigationItem item)
    {
        var element = new XElement("Item");
        element.SetAttributeValue("Title", item.Title);
        
        if (!string.IsNullOrEmpty(item.Icon))
        {
            // 将字符编码为 HTML 实体
            var iconValue = EncodeIcon(item.Icon);
            element.SetAttributeValue("Icon", iconValue);
        }

        if (!string.IsNullOrEmpty(item.PageKey))
        {
            element.SetAttributeValue("PageKey", item.PageKey);
        }

        if (!string.IsNullOrEmpty(item.PageType))
        {
            element.SetAttributeValue("PageType", item.PageType);
        }

        if (item.IsExpanded)
        {
            element.SetAttributeValue("IsExpanded", "true");
        }

        if (!item.IsEnabled)
        {
            element.SetAttributeValue("IsEnabled", "false");
        }

        // 递归处理子项
        if (item.HasChildren)
        {
            var childrenElement = new XElement("Children");
            foreach (var child in item.Children)
            {
                childrenElement.Add(CreateItemElement(child));
            }
            element.Add(childrenElement);
        }

        return element;
    }

    /// <summary>
    /// 将图标字符编码为 HTML 实体
    /// </summary>
    private static string EncodeIcon(string icon)
    {
        if (string.IsNullOrEmpty(icon))
        {
            return string.Empty;
        }

        // 如果已经是 HTML 实体格式，直接返回
        if (icon.StartsWith("&#x") && icon.EndsWith(";"))
        {
            return icon;
        }

        // 将字符转换为 Unicode 实体
        if (icon.Length > 0)
        {
            var charCode = (int)icon[0];
            return $"&#x{charCode:X};";
        }

        return icon;
    }
}

