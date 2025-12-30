using System;

namespace Lemoo.App.Helper.Attributes;

/// <summary>
/// 页面特性：用于标记页面的元数据，支持自动发现和注册
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class PageAttribute : Attribute
{
    /// <summary>
    /// 页面键（唯一标识符）
    /// </summary>
    public string PageKey { get; }

    /// <summary>
    /// 页面标题（显示名称）
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 页面图标（Segoe MDL2 Assets 字符）
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 页面所属模块（用于组织和管理）
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// 是否默认启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 页面描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    public PageAttribute(string pageKey)
    {
        PageKey = pageKey ?? throw new ArgumentNullException(nameof(pageKey));
    }
}

