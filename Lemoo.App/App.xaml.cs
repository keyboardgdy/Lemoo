using System.Windows;
using Lemoo.App.ViewModels;
using Lemoo.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lemoo.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder(e.Args)
            .ConfigureServices(ConfigureServices)
            .Build();

        _host.Start();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // 服务
        services.AddSingleton<Services.PageRegistry>(sp =>
        {
            var registry = new Services.PageRegistry();
            registry.Initialize(); // 自动发现并注册所有页面
            return registry;
        });
        services.AddSingleton<Services.NavigationService>(sp =>
        {
            var pageRegistry = sp.GetRequiredService<Services.PageRegistry>();
            return new Services.NavigationService(pageRegistry);
        });
        services.AddSingleton<Services.PageFactory>(sp =>
        {
            var navigationService = sp.GetRequiredService<Services.NavigationService>();
            var pageRegistry = sp.GetRequiredService<Services.PageRegistry>();
            return new Services.PageFactory(navigationService, pageRegistry);
        });

        // ViewModels
        services.AddSingleton<MainViewModel>(sp =>
        {
            var pageFactory = sp.GetRequiredService<Services.PageFactory>();
            return new MainViewModel(pageFactory);
        });

        // Views
        services.AddSingleton<MainWindow>();

    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}

