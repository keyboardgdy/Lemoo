using Lemoo.Domain.Entities;
using Lemoo.Domain.Repositories;
using Lemoo.Domain.Services;
using Lemoo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lemoo.Infrastructure.Services;

/// <summary>
/// 数据库初始化服务
/// </summary>
public class DatabaseInitializer
{
    private readonly LemooDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseInitializer(
        LemooDbContext dbContext,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 初始化数据库（确保数据库已创建并应用迁移）
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        try
        {
            // 检查数据库是否可以连接
            if (!await _dbContext.Database.CanConnectAsync())
            {
                // 数据库不存在，创建数据库并应用迁移
                System.Diagnostics.Debug.WriteLine("数据库不存在，正在创建并应用迁移...");
                await _dbContext.Database.MigrateAsync();
                System.Diagnostics.Debug.WriteLine("数据库创建完成");
                return;
            }

            // 数据库已存在，检查迁移状态
            try
            {
                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                
                if (pendingMigrations.Any())
                {
                    // 有待应用的迁移，应用迁移
                    System.Diagnostics.Debug.WriteLine($"发现 {pendingMigrations.Count()} 个待应用的迁移，正在应用...");
                    await _dbContext.Database.MigrateAsync();
                    System.Diagnostics.Debug.WriteLine("迁移应用完成");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("数据库已存在且为最新版本");
                }
            }
            catch (Exception migrationEx)
            {
                // 如果获取迁移状态失败，可能是迁移历史表不存在（旧数据库）
                System.Diagnostics.Debug.WriteLine($"检查迁移状态失败：{migrationEx.Message}");
                System.Diagnostics.Debug.WriteLine("尝试应用迁移以更新数据库结构...");
                
                try
                {
                    // 尝试应用迁移
                    await _dbContext.Database.MigrateAsync();
                    System.Diagnostics.Debug.WriteLine("迁移应用成功");
                }
                catch (Exception migrateEx)
                {
                    // 迁移失败，提供详细错误信息
                    var errorMessage = $"数据库迁移失败：{migrateEx.Message}\n\n" +
                                     "可能的原因：\n" +
                                     "1. 数据库表结构与迁移不匹配\n" +
                                     "2. 需要手动应用迁移：dotnet ef database update -p Lemoo.Infrastructure\n" +
                                     "3. 或者删除旧数据库重新创建";
                    
                    System.Diagnostics.Debug.WriteLine(errorMessage);
                    throw new InvalidOperationException(errorMessage, migrateEx);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"确保数据库创建失败：{ex.Message}");
            System.Diagnostics.Debug.WriteLine($"堆栈跟踪：{ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// 初始化数据库并更新种子数据
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // 先确保数据库和迁移已应用
            await EnsureDatabaseCreatedAsync();

            // 初始化种子数据
            await SeedDataAsync();
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常，避免影响应用启动
            System.Diagnostics.Debug.WriteLine($"数据库初始化失败：{ex.Message}");
            System.Diagnostics.Debug.WriteLine($"堆栈跟踪：{ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// 更新种子数据
    /// </summary>
    public async Task SeedDataAsync()
    {
        try
        {
            // 检查是否已有用户数据
            var hasUsers = await _userRepository.AnyAsync();
            
            if (!hasUsers)
            {
                // 创建默认管理员用户（用户名：sa，密码：newu）
                var defaultUser = new User
                {
                    Username = "sa",
                    PasswordHash = _passwordHasher.ComputeHash("newu"),
                    Name = "系统管理员",
                    Email = "admin@lemoo.com",
                    PhoneNumber = null, // 默认不设置手机号
                    AvatarUrl = null, // 默认不设置头像
                    CreatedAt = DateTime.Now,
                    IsEnabled = true
                };

                await _userRepository.AddAsync(defaultUser);
                System.Diagnostics.Debug.WriteLine("已创建默认管理员用户：sa");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("用户数据已存在，跳过种子数据初始化");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"种子数据初始化失败：{ex.Message}");
            throw;
        }
    }
}

