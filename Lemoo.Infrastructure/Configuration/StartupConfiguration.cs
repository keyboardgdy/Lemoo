using System.Diagnostics;

namespace Lemoo.Infrastructure.Configuration;

/// <summary>
/// 启动模式枚举
/// </summary>
public enum StartupMode
{
    /// <summary>
    /// 快速启动模式：不初始化数据库，保证稳定运行和快速启动
    /// </summary>
    FastStart,

    /// <summary>
    /// 初始化模式：初始化数据库并更新种子数据
    /// </summary>
    Initialize
}

/// <summary>
/// 启动配置
/// </summary>
public class StartupConfiguration
{
    /// <summary>
    /// 启动模式
    /// </summary>
    public StartupMode Mode { get; set; } = GetDefaultStartupMode();

    /// <summary>
    /// 是否启用数据库初始化
    /// </summary>
    public bool EnableDatabaseInitialization => Mode == StartupMode.Initialize;

    /// <summary>
    /// 获取默认启动模式
    /// </summary>
    /// <returns>默认启动模式</returns>
    /// <remarks>
    /// 在DEBUG模式下或调试器附加时，默认使用Initialize模式（方便开发）
    /// 在RELEASE模式下，默认使用FastStart模式（保证性能）
    /// </remarks>
    private static StartupMode GetDefaultStartupMode()
    {
#if DEBUG
        // DEBUG模式下默认使用初始化模式，方便开发时自动更新种子数据
        return StartupMode.Initialize;
#else
        // RELEASE模式下默认使用快速启动模式
        return StartupMode.FastStart;
#endif
    }

    /// <summary>
    /// 检查是否在开发环境中
    /// </summary>
    /// <returns>如果在开发环境中返回true</returns>
    private static bool IsDevelopmentEnvironment()
    {
        // 检查是否有调试器附加（Visual Studio调试时）
        if (Debugger.IsAttached)
        {
            return true;
        }

        // 检查是否在DEBUG模式下编译
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// 从命令行参数解析启动模式
    /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>启动配置</returns>
    public static StartupConfiguration FromCommandLineArgs(string[] args)
    {
        var config = new StartupConfiguration();

        // 检查命令行参数
        if (args != null && args.Length > 0)
        {
            foreach (var arg in args)
            {
                // 支持 --init 或 --initialize 参数来启用初始化模式
                if (arg.Equals("--init", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--initialize", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("/init", StringComparison.OrdinalIgnoreCase))
                {
                    config.Mode = StartupMode.Initialize;
                    break;
                }
                // 支持 --fast 或 --fast-start 参数来启用快速启动模式（默认）
                else if (arg.Equals("--fast", StringComparison.OrdinalIgnoreCase) ||
                         arg.Equals("--fast-start", StringComparison.OrdinalIgnoreCase) ||
                         arg.Equals("/fast", StringComparison.OrdinalIgnoreCase))
                {
                    config.Mode = StartupMode.FastStart;
                    break;
                }
            }
        }

        // 也可以从环境变量读取
        var envMode = Environment.GetEnvironmentVariable("LEMOO_STARTUP_MODE");
        if (!string.IsNullOrWhiteSpace(envMode))
        {
            if (Enum.TryParse<StartupMode>(envMode, true, out var parsedMode))
            {
                config.Mode = parsedMode;
            }
        }

        return config;
    }
}

