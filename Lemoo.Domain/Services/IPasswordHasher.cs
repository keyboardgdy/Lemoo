namespace Lemoo.Domain.Services;

/// <summary>
/// 密码哈希服务接口（领域服务）
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 计算密码哈希值
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>密码哈希值</returns>
    string ComputeHash(string password);

    /// <summary>
    /// 验证密码是否匹配哈希值
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="passwordHash">密码哈希值</param>
    /// <returns>如果匹配返回true，否则返回false</returns>
    bool VerifyPassword(string password, string passwordHash);
}

