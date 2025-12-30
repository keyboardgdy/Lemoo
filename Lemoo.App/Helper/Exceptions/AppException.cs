using System;

namespace Lemoo.App.Helper.Exceptions;

/// <summary>
/// 应用程序基础异常类
/// </summary>
public class AppException : Exception
{
    public AppException()
    {
    }

    public AppException(string message) : base(message)
    {
    }

    public AppException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// 页面未找到异常
/// </summary>
public class PageNotFoundException : AppException
{
    public string PageKey { get; }

    public PageNotFoundException(string pageKey) : base($"页面未找到: {pageKey}")
    {
        PageKey = pageKey;
    }
}

/// <summary>
/// 配置加载异常
/// </summary>
public class ConfigurationException : AppException
{
    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

