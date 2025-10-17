using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DeskSharper.Localization;
using DeskSharper.Services;
using DeskSharper.ViewModels;
using DeskSharper.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DeskSharper;

public class App : Application
{
    public static Window? MainWindow { get; private set; }
    private static IServiceProvider? Services { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Strings.InitializeFromCulture();
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "<Pending>")]
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Configure logging
            ConfigureLogging();

            // Setup Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            DisableAvaloniaDataAnnotationValidation();

            // Create main window
            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            desktop.MainWindow = MainWindow;

            // Handle auto-run on startup
            desktop.Startup += async (_, _) => { await HandleAutoRunAsync(); };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureLogging()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logPath = Path.Combine(appDataPath, "DeskSharper", "logs", "desksharper.log");
        var logDir = Path.GetDirectoryName(logPath);

        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder => { builder.AddSerilog(dispose: true); });

        // Services
        services.AddSingleton<ConfigService>();
        services.AddSingleton<CleaningService>();
        services.AddSingleton<NotificationService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
    }

    private async Task HandleAutoRunAsync()
    {
        try
        {
            var configService = Services?.GetRequiredService<ConfigService>();
            var cleaningService = Services?.GetRequiredService<CleaningService>();
            var notificationService = Services?.GetRequiredService<NotificationService>();

            if (configService == null || cleaningService == null || notificationService == null)
                return;

            var autoRunConfigs = await configService.GetAutoRunConfigsAsync();

            foreach (var config in autoRunConfigs)
            {
                try
                {
                    var tasks = await cleaningService.ScanDirectoryAsync(config);

                    if (tasks.Count > 0)
                    {
                        var result = await cleaningService.ExecuteCleaningAsync(
                            tasks,
                            config.DestinationPath);

                        await notificationService.ShowNotificationAsync(
                            $"Auto-Run: {config.Name}",
                            $"{result.MovedItems.Count} items processed");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during auto-run for config: {ConfigName}", config.Name);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during auto-run");
        }
    }

    [RequiresUnreferencedCode("Calls Avalonia.Data.Core.Plugins.BindingPlugins.DataValidators")]
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}