using System.Security.Cryptography;
using System.Text;

namespace Lemoo.Domain.Services;

/// <summary>
/// 密码哈希服务实现（领域服务）
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// 计算密码哈希值（SHA256）
    /// </summary>
    public string ComputeHash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("密码不能为空", nameof(password));
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// 验证密码是否匹配哈希值
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var computedHash = ComputeHash(password);
        return computedHash == passwordHash;
    }
}

