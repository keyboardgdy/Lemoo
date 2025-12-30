namespace Lemoo.Domain.Services;

/// <summary>
/// 认证领域服务接口
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// 验证用户登录凭据
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>验证成功返回true，否则返回false</returns>
    Task<bool> ValidateCredentialsAsync(string username, string password);

    /// <summary>
    /// 更新用户最后登录时间
    /// </summary>
    /// <param name="username">用户名</param>
    Task UpdateLastLoginTimeAsync(string username);
}

