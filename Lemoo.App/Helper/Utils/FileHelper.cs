using System.IO;

namespace Lemoo.App.Helper.Utils;

/// <summary>
/// 文件操作辅助类
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 确保目录存在，如果不存在则创建
    /// </summary>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 获取文件大小（格式化字符串）
    /// </summary>
    public static string GetFileSizeString(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// 检查文件是否存在且可读
    /// </summary>
    public static bool IsFileReadable(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            using var stream = File.OpenRead(filePath);
            return stream.CanRead;
        }
        catch
        {
            return false;
        }
    }
}

