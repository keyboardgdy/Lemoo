using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Lemoo.App.Helper.Extensions;
using Lemoo.App.Views.Windows;
using Lemoo.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lemoo.App;

/// <summary>
/// 应用程序入口类
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    /// <summary>
    /// 应用程序启动时调用
    /// </summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // 配置应用程序性能优化
            ConfigurePerformanceOptimizations();

            // 设置关闭模式：只有在显式调用 Shutdown 时才关闭应用程序
            // 这样可以防止主窗口关闭时应用程序立即退出
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 构建并启动主机
            _host = BuildHost(e.Args);
            _host.Start();

            // 初始化数据库
            await _host.Services.InitializeDatabaseAsync();

            // 显示登录窗口，登录成功后显示主窗口
            ShowLoginWindow();
        }
        catch (Exception ex)
        {
            // 记录启动错误并显示给用户
            HandleStartupError(ex);
        }
    }

    /// <summary>
    /// 配置性能优化设置
    /// </summary>
    private static void ConfigurePerformanceOptimizations()
    {
        // 启用硬件加速和渲染优化
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

        // 优化文本渲染（针对 FrameworkElement）
        TextOptions.TextFormattingModeProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(TextFormattingMode.Display));
    }

    /// <summary>
    /// 构建应用程序主机
    /// </summary>
    private static IHost BuildHost(string[] args)
    {
        // 从命令行参数解析启动配置
        var startupConfig = StartupConfiguration.FromCommandLineArgs(args);

        // 确定环境名称
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                            ?? (System.Diagnostics.Debugger.IsAttached ? "Development" : "Production");

        // 构建配置
        var appBasePath = AppDomain.CurrentDomain.BaseDirectory;
        var configuration = ConfigurationLoader.BuildConfiguration(appBasePath, environmentName);

        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // 配置已通过 ConfigurationLoader 构建，这里可以添加额外配置
            })
            .ConfigureServices((context, services) =>
            {
                // 注册配置对象
                services.AddSingleton<IConfiguration>(configuration);

                // 使用扩展方法注册所有应用程序服务，传入启动配置和配置对象
                services.AddApplicationServices(startupConfig, configuration);
            })
            .Build();
    }

    /// <summary>
    /// 显示登录窗口
    /// </summary>
    private void ShowLoginWindow()
    {
        if (_host == null)
        {
            throw new InvalidOperationException("主机未初始化，无法获取登录窗口。");
        }

        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        
        // 显示登录窗口（模态对话框）
        bool? dialogResult = loginWindow.ShowDialog();

        // 检查登录结果
        if (dialogResult == true)
        {
            // 登录成功，显示主窗口
            ShowMainWindow();
        }
        else
        {
            // 登录失败或取消，关闭应用程序
            Shutdown();
        }
    }

    /// <summary>
    /// 显示主窗口
    /// </summary>
    private void ShowMainWindow()
    {
        if (_host == null)
        {
            throw new InvalidOperationException("主机未初始化，无法获取主窗口。");
        }

        var mainWindow = _host.Services.GetRequiredService<Views.MainWindow>();
        
        // 订阅主窗口关闭事件
        mainWindow.Closed += (sender, e) =>
        {
            // 主窗口关闭时，关闭应用程序
            Shutdown();
        };

        // 显示主窗口
        mainWindow.Show();
        
        // 主窗口显示后，将关闭模式改为 OnMainWindowClose
        // 这样如果主窗口被关闭，应用程序也会关闭
        ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    /// <summary>
    /// 处理启动错误
    /// </summary>
    private static void HandleStartupError(Exception ex)
    {
        // 显示错误消息框
        MessageBox.Show(
            $"应用程序启动失败：{ex.Message}\n\n详细信息：{ex}",
            "启动错误",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // 退出应用程序
        Current.Shutdown();
    }

    /// <summary>
    /// 应用程序退出时调用
    /// </summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            // 停止并释放主机资源
            if (_host != null)
            {
                await _host.StopAsync().ConfigureAwait(false);
                _host.Dispose();
                _host = null;
            }
        }
        catch (Exception ex)
        {
            // 记录退出错误（可选：可以添加日志记录）
            System.Diagnostics.Debug.WriteLine($"应用程序退出时发生错误：{ex}");
        }
        finally
        {
            base.OnExit(e);
        }
    }
}

