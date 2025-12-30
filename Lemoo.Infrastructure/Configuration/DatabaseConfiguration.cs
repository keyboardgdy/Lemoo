namespace Lemoo.Infrastructure.Configuration;

/// <summary>
/// 数据库配置
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// 服务器地址
    /// </summary>
    public string Server { get; set; } = "localhost";

    /// <summary>
    /// 数据库名称
    /// </summary>
    public string Database { get; set; } = "LemooDb";

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserId { get; set; } = "sa";

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = "newu";

    /// <summary>
    /// 是否信任服务器证书
    /// </summary>
    public bool TrustServerCertificate { get; set; } = true;

    /// <summary>
    /// 获取连接字符串
    /// </summary>
    public string GetConnectionString()
    {
        return $"Server={Server};Database={Database};User Id={UserId};Password={Password};TrustServerCertificate={TrustServerCertificate};";
    }
}

