using System.Windows;

namespace Lemoo.App.Helper.Extensions;

/// <summary>
/// Window 扩展方法
/// </summary>
public static class WindowExtensions
{
    /// <summary>
    /// 居中显示窗口
    /// </summary>
    public static void CenterOnScreen(this Window window)
    {
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    /// <summary>
    /// 居中显示窗口（相对于父窗口）
    /// </summary>
    public static void CenterOnOwner(this Window window)
    {
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}

