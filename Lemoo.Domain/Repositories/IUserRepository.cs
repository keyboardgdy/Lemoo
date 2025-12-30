using Lemoo.Domain.Entities;

namespace Lemoo.Domain.Repositories;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 根据用户名查找用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>用户实体，如果不存在返回null</returns>
    Task<User?> FindByUsernameAsync(string username);

    /// <summary>
    /// 根据用户名和启用状态查找用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="isEnabled">是否启用</param>
    /// <returns>用户实体，如果不存在返回null</returns>
    Task<User?> FindByUsernameAndEnabledAsync(string username, bool isEnabled = true);

    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="user">用户实体</param>
    Task AddAsync(User user);

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="user">用户实体</param>
    Task UpdateAsync(User user);

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    Task<bool> ExistsByUsernameAsync(string username);

    /// <summary>
    /// 检查是否有任何用户
    /// </summary>
    /// <returns>如果有用户返回true，否则返回false</returns>
    Task<bool> AnyAsync();

    /// <summary>
    /// 根据邮箱查找用户
    /// </summary>
    /// <param name="email">邮箱</param>
    /// <returns>用户实体，如果不存在返回null</returns>
    Task<User?> FindByEmailAsync(string email);

    /// <summary>
    /// 根据手机号查找用户
    /// </summary>
    /// <param name="phoneNumber">手机号</param>
    /// <returns>用户实体，如果不存在返回null</returns>
    Task<User?> FindByPhoneNumberAsync(string phoneNumber);

    /// <summary>
    /// 检查邮箱是否存在
    /// </summary>
    /// <param name="email">邮箱</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// 检查手机号是否存在
    /// </summary>
    /// <param name="phoneNumber">手机号</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
}

