using System;

namespace Lemoo.App.Helper.Extensions;

/// <summary>
/// 字符串扩展方法
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 检查字符串是否为空或空白
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// 如果字符串为空，返回默认值
    /// </summary>
    public static string DefaultIfEmpty(this string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    /// <summary>
    /// 安全截取字符串
    /// </summary>
    public static string SafeSubstring(this string value, int startIndex, int length)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (startIndex < 0)
            startIndex = 0;

        if (startIndex >= value.Length)
            return string.Empty;

        if (startIndex + length > value.Length)
            length = value.Length - startIndex;

        return value.Substring(startIndex, length);
    }
}

