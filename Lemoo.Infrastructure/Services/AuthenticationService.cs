using Lemoo.Domain.Repositories;
using Lemoo.Domain.Services;

namespace Lemoo.Infrastructure.Services;

/// <summary>
/// 认证领域服务实现
/// </summary>
public class AuthenticationService : Domain.Services.IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 验证用户登录凭据
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var user = await _userRepository.FindByUsernameAndEnabledAsync(username, true);

        if (user == null)
        {
            return false;
        }

        // 使用领域服务验证密码
        return _passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    /// <summary>
    /// 更新用户最后登录时间
    /// </summary>
    public async Task UpdateLastLoginTimeAsync(string username)
    {
        var user = await _userRepository.FindByUsernameAsync(username);

        if (user != null)
        {
            user.LastLoginAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);
        }
    }
}

