using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HevyHeartConsole.Config;
using HevyHeartConsole.Services;
using HevyHeartGui.ViewModels;
using HevyHeartGui.Services;

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

        // Register configuration service
        services.AddSingleton<ConfigurationService>();

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

        // Validate configuration
        var configService = _serviceProvider?.GetRequiredService<ConfigurationService>();
        var appConfig = _serviceProvider?.GetRequiredService<AppConfig>();

        if (configService != null && appConfig != null)
        {
            if (!configService.ValidateConfiguration(appConfig, out var missingSettings))
            {
                System.Diagnostics.Debug.WriteLine($"Missing {missingSettings.Count} settings:");
                foreach (var setting in missingSettings)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {setting}");
                }

                // Show settings dialog
                var settingsViewModel = new SettingsViewModel(appConfig);
                var settingsWindow = new SettingsWindow(settingsViewModel);
                
                var result = settingsWindow.ShowDialog();
                
                System.Diagnostics.Debug.WriteLine($"Settings dialog result: {result}");
                
                if (result == true)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("User clicked Save - about to save configuration");
                        System.Diagnostics.Debug.WriteLine($"Strava ClientId: '{appConfig.Strava.ClientId}'");
                        System.Diagnostics.Debug.WriteLine($"Hevy ApiKey length: {appConfig.Hevy.ApiKey?.Length ?? 0}");
                        
                        // Save the updated configuration
                        configService.SaveConfiguration(appConfig);
                        
                        System.Diagnostics.Debug.WriteLine("Configuration saved successfully");
                        
                        // Rebuild the service provider with the updated configuration
                        _serviceProvider?.Dispose();
                        var serviceCollection = new ServiceCollection();
                        ConfigureServices(serviceCollection);
                        _serviceProvider = serviceCollection.BuildServiceProvider();
                        
                        System.Diagnostics.Debug.WriteLine("Service provider rebuilt");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex}");
                        MessageBox.Show(
                            $"Failed to save configuration:\n\n{ex.Message}\n\nThe application will now close.",
                            "Configuration Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Shutdown();
                        return;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User cancelled settings dialog");
                    // User cancelled - show message and exit
                    MessageBox.Show(
                        "Application settings are required to continue.\n\n" +
                        "The application will now close.",
                        "Settings Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    Shutdown();
                    return;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("All required settings are present");
            }
        }

        var mainWindow = _serviceProvider?.GetRequiredService<MainWindow>();
        mainWindow?.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
