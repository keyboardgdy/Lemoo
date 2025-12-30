using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Lemoo.Infrastructure.Configuration;

/// <summary>
/// 配置加载器：从配置文件和环境变量加载数据库配置
/// </summary>
public static class ConfigurationLoader
{
    /// <summary>
    /// 从配置加载数据库配置
    /// </summary>
    /// <param name="configuration">配置对象</param>
    /// <returns>数据库配置</returns>
    public static DatabaseConfiguration LoadDatabaseConfiguration(IConfiguration configuration)
    {
        var dbConfig = new DatabaseConfiguration();

        // 从配置文件读取非敏感信息
        var dbSection = configuration.GetSection("Database");
        dbConfig.Server = dbSection["Server"] ?? "localhost";
        dbConfig.Database = dbSection["Database"] ?? "LemooDb";
        
        // 读取 TrustServerCertificate（支持字符串和布尔值）
        var trustCertValue = dbSection["TrustServerCertificate"];
        if (bool.TryParse(trustCertValue, out var trustCert))
        {
            dbConfig.TrustServerCertificate = trustCert;
        }
        else
        {
            dbConfig.TrustServerCertificate = true; // 默认值
        }

        // 优先从环境变量读取敏感信息（用户名和密码）
        // 如果环境变量不存在，则从配置文件读取（开发环境）
        dbConfig.UserId = Environment.GetEnvironmentVariable("LEMOO_DB_USERID") 
                         ?? dbSection["UserId"] 
                         ?? "sa";
        
        dbConfig.Password = Environment.GetEnvironmentVariable("LEMOO_DB_PASSWORD") 
                           ?? dbSection["Password"] 
                           ?? "newu";

        return dbConfig;
    }

    /// <summary>
    /// 构建配置对象
    /// </summary>
    /// <param name="basePath">应用程序基路径</param>
    /// <param name="environmentName">环境名称（Development, Production等）</param>
    /// <returns>配置对象</returns>
    public static IConfiguration BuildConfiguration(string basePath, string? environmentName = null)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // 根据环境加载不同的配置文件
        if (!string.IsNullOrWhiteSpace(environmentName))
        {
            builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
        }

        // 环境变量优先级最高（会覆盖配置文件中的值）
        builder.AddEnvironmentVariables();

        return builder.Build();
    }
}

