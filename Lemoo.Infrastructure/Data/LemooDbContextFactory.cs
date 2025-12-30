using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lemoo.Infrastructure.Data;

/// <summary>
/// DbContext 设计时工厂（用于 EF Core 工具，如 migrations）
/// </summary>
public class LemooDbContextFactory : IDesignTimeDbContextFactory<LemooDbContext>
{
    /// <summary>
    /// 创建 DbContext 实例（用于设计时）
    /// </summary>
    public LemooDbContext CreateDbContext(string[] args)
    {
        // 设计时连接字符串（用于生成迁移）
        // 优先从环境变量读取，如果没有则使用默认值
        var userId = Environment.GetEnvironmentVariable("LEMOO_DB_USERID") ?? "sa";
        var password = Environment.GetEnvironmentVariable("LEMOO_DB_PASSWORD") ?? "newu";
        var connectionString = $"Server=localhost;Database=LemooDb;User Id={userId};Password={password};TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<LemooDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new LemooDbContext(optionsBuilder.Options);
    }
}

