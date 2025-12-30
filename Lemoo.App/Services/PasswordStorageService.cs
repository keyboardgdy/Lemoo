using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Lemoo.App.Services;

/// <summary>
/// 密码存储服务：使用 Windows DPAPI 加密存储密码
/// </summary>
public class PasswordStorageService
{
    private const string SettingsFileName = "login_settings.dat";
    private readonly string _settingsFilePath;

    public PasswordStorageService()
    {
        // 使用应用程序数据目录存储设置文件
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Lemoo");
        
        // 确保目录存在
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _settingsFilePath = Path.Combine(appDataPath, SettingsFileName);
    }

    /// <summary>
    /// 保存登录凭据
    /// </summary>
    public void SaveCredentials(string username, string password, bool rememberPassword)
    {
        try
        {
            if (!rememberPassword)
            {
                // 如果不记住密码，删除保存的凭据
                if (File.Exists(_settingsFilePath))
                {
                    File.Delete(_settingsFilePath);
                }
                return;
            }

            // 使用 DPAPI 加密密码
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var encryptedPassword = ProtectedData.Protect(
                passwordBytes,
                Encoding.UTF8.GetBytes(username), // 使用用户名作为额外熵
                DataProtectionScope.CurrentUser);

            // 保存到文件（简单格式：用户名|加密密码的Base64）
            var content = $"{username}|{Convert.ToBase64String(encryptedPassword)}";
            File.WriteAllText(_settingsFilePath, content, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常，避免影响登录流程
            System.Diagnostics.Debug.WriteLine($"保存登录凭据失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 加载保存的登录凭据
    /// </summary>
    public (string? Username, string? Password) LoadCredentials()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return (null, null);
            }

            var content = File.ReadAllText(_settingsFilePath, Encoding.UTF8);
            var parts = content.Split('|');
            
            if (parts.Length != 2)
            {
                return (null, null);
            }

            var username = parts[0];
            var encryptedPassword = Convert.FromBase64String(parts[1]);

            // 使用 DPAPI 解密密码
            var passwordBytes = ProtectedData.Unprotect(
                encryptedPassword,
                Encoding.UTF8.GetBytes(username), // 使用用户名作为额外熵
                DataProtectionScope.CurrentUser);

            var password = Encoding.UTF8.GetString(passwordBytes);
            return (username, password);
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常
            System.Diagnostics.Debug.WriteLine($"加载登录凭据失败：{ex.Message}");
            return (null, null);
        }
    }

    /// <summary>
    /// 清除保存的登录凭据
    /// </summary>
    public void ClearCredentials()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                File.Delete(_settingsFilePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"清除登录凭据失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 检查是否有保存的凭据
    /// </summary>
    public bool HasSavedCredentials()
    {
        return File.Exists(_settingsFilePath);
    }
}

