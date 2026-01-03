using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HevyHeartConsole.Config;
using HevyHeartConsole.Services;
using HevyHeartGui.ViewModels;

namespace HevyHeartGui;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public App()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var appConfig = configuration.Get<AppConfig>() ?? new AppConfig();
        services.AddSingleton(appConfig);
        services.AddSingleton(appConfig.Strava);
        services.AddSingleton(appConfig.Hevy);
        services.AddSingleton(appConfig.Server);

        // Register HTTP clients
        services.AddHttpClient<StravaService>();
        services.AddHttpClient<HevyService>();

        // Register services
        services.AddTransient<StravaService>();
        services.AddTransient<HevyService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();

        // Register MainWindow
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider?.GetRequiredService<MainWindow>();
        mainWindow?.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
