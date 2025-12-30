using Lemoo.Domain.Entities;
using Lemoo.Domain.Repositories;
using Lemoo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lemoo.Infrastructure.Repositories;

/// <summary>
/// 用户仓储实现
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly LemooDbContext _dbContext;

    public UserRepository(LemooDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 根据用户名查找用户
    /// </summary>
    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// 根据用户名和启用状态查找用户
    /// </summary>
    public async Task<User?> FindByUsernameAndEnabledAsync(string username, bool isEnabled = true)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsEnabled == isEnabled);
    }

    /// <summary>
    /// 添加用户
    /// </summary>
    public async Task AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    public async Task UpdateAsync(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Username == username);
    }

    /// <summary>
    /// 检查是否有任何用户
    /// </summary>
    public async Task<bool> AnyAsync()
    {
        return await _dbContext.Users.AnyAsync();
    }

    /// <summary>
    /// 根据邮箱查找用户
    /// </summary>
    public async Task<User?> FindByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email != null && u.Email == email);
    }

    /// <summary>
    /// 根据手机号查找用户
    /// </summary>
    public async Task<User?> FindByPhoneNumberAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber != null && u.PhoneNumber == phoneNumber);
    }

    /// <summary>
    /// 检查邮箱是否存在
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await _dbContext.Users
            .AnyAsync(u => u.Email != null && u.Email == email);
    }

    /// <summary>
    /// 检查手机号是否存在
    /// </summary>
    public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        return await _dbContext.Users
            .AnyAsync(u => u.PhoneNumber != null && u.PhoneNumber == phoneNumber);
    }
}

