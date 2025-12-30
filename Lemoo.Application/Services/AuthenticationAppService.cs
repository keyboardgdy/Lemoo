using Lemoo.Domain.Services;

namespace Lemoo.Application.Services;

/// <summary>
/// 认证应用服务实现
/// </summary>
public class AuthenticationAppService : IAuthenticationAppService
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationAppService(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// 验证用户登录凭据
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        return await _authenticationService.ValidateCredentialsAsync(username, password);
    }

    /// <summary>
    /// 更新用户最后登录时间
    /// </summary>
    public async Task UpdateLastLoginTimeAsync(string username)
    {
        await _authenticationService.UpdateLastLoginTimeAsync(username);
    }
}

