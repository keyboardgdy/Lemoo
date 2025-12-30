using Lemoo.App.Services;
using Lemoo.App.ViewModels;
using Lemoo.App.Views;
using Lemoo.App.Views.Windows;
using Lemoo.Application.Services;
using Lemoo.Domain.Repositories;
using Lemoo.Domain.Services;
using Lemoo.Infrastructure.Configuration;
using Lemoo.Infrastructure.Data;
using Lemoo.Infrastructure.Repositories;
using Lemoo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lemoo.App.Helper.Extensions;

/// <summary>
/// 服务集合扩展方法：用于配置应用程序服务
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册数据库服务
    /// </summary>
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services, 
        StartupConfiguration? startupConfig = null,
        IConfiguration? configuration = null)
    {
        // 注册启动配置（单例）
        if (startupConfig == null)
        {
            startupConfig = new StartupConfiguration { Mode = StartupMode.FastStart };
        }
        services.AddSingleton(startupConfig);

        // 从配置加载数据库配置（优先使用环境变量）
        DatabaseConfiguration dbConfig;
        if (configuration != null)
        {
            dbConfig = ConfigurationLoader.LoadDatabaseConfiguration(configuration);
        }
        else
        {
            // 如果没有配置对象，使用默认值（向后兼容）
            dbConfig = new DatabaseConfiguration
            {
                Server = "localhost",
                Database = "LemooDb",
                UserId = Environment.GetEnvironmentVariable("LEMOO_DB_USERID") ?? "sa",
                Password = Environment.GetEnvironmentVariable("LEMOO_DB_PASSWORD") ?? "newu",
                TrustServerCertificate = true
            };
        }
        services.AddSingleton(dbConfig);

        // 注册DbContext
        services.AddDbContext<LemooDbContext>(options =>
            options.UseSqlServer(dbConfig.GetConnectionString()));

        // 注册仓储（DDD模式）
        services.AddScoped<IUserRepository, UserRepository>();

        // 注册领域服务（DDD模式）
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<Domain.Services.IAuthenticationService, Infrastructure.Services.AuthenticationService>();

        // 注册应用服务（DDD模式）
        services.AddScoped<IAuthenticationAppService, AuthenticationAppService>();

        // 注册数据库初始化服务
        services.AddScoped<DatabaseInitializer>();

        return services;
    }

    /// <summary>
    /// 初始化数据库（在应用启动时调用，根据启动模式决定是否初始化）
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        var startupConfig = serviceProvider.GetRequiredService<StartupConfiguration>();
        
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();

        if (startupConfig.EnableDatabaseInitialization)
        {
            // 初始化模式：初始化数据库并更新种子数据
            System.Diagnostics.Debug.WriteLine("启动模式：初始化模式 - 正在初始化数据库和种子数据...");
            await initializer.InitializeAsync();
            System.Diagnostics.Debug.WriteLine("数据库初始化完成");
        }
        else
        {
            // 快速启动模式：只确保数据库已创建，不初始化种子数据
            System.Diagnostics.Debug.WriteLine("启动模式：快速启动模式 - 仅确保数据库已创建");
            await initializer.EnsureDatabaseCreatedAsync();
            System.Diagnostics.Debug.WriteLine("数据库检查完成");
        }
    }

    /// <summary>
    /// 注册应用程序核心服务
    /// </summary>
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // 注册密码存储服务
        services.AddSingleton<PasswordStorageService>();

        // 注册页面注册表（自动发现并注册所有页面）
        services.AddSingleton<PageRegistry>(sp =>
        {
            var registry = new PageRegistry();
            registry.Initialize();
            return registry;
        });

        // 注册导航服务
        services.AddSingleton<NavigationService>(sp =>
        {
            var pageRegistry = sp.GetRequiredService<PageRegistry>();
            return new NavigationService(pageRegistry);
        });

        // 注册页面工厂
        services.AddSingleton<PageFactory>(sp =>
        {
            var navigationService = sp.GetRequiredService<NavigationService>();
            var pageRegistry = sp.GetRequiredService<PageRegistry>();
            return new PageFactory(navigationService, pageRegistry);
        });

        return services;
    }

    /// <summary>
    /// 注册应用程序视图模型
    /// </summary>
    public static IServiceCollection AddAppViewModels(this IServiceCollection services)
    {
        // 登录视图模型（每次创建新实例）
        services.AddTransient<LoginViewModel>(sp =>
        {
            var passwordStorageService = sp.GetRequiredService<PasswordStorageService>();
            var authenticationAppService = sp.GetRequiredService<IAuthenticationAppService>();
            return new LoginViewModel(passwordStorageService, authenticationAppService);
        });

        // 主视图模型
        services.AddSingleton<MainViewModel>(sp =>
        {
            var pageFactory = sp.GetRequiredService<PageFactory>();
            return new MainViewModel(pageFactory);
        });

        return services;
    }

    /// <summary>
    /// 注册应用程序视图
    /// </summary>
    public static IServiceCollection AddAppViews(this IServiceCollection services)
    {
        // 登录窗口（每次创建新实例）
        services.AddTransient<LoginWindow>(sp =>
        {
            var viewModel = sp.GetRequiredService<LoginViewModel>();
            return new LoginWindow(viewModel);
        });

        // 主窗口
        services.AddSingleton<MainWindow>();

        return services;
    }

    /// <summary>
    /// 注册所有应用程序服务、视图模型和视图
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, 
        StartupConfiguration? startupConfig = null,
        IConfiguration? configuration = null)
    {
        return services
            .AddDatabaseServices(startupConfig, configuration)
            .AddAppServices()
            .AddAppViewModels()
            .AddAppViews();
    }
}

