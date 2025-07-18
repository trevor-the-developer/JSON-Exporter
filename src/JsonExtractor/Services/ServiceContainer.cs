using JsonExtractor.Commands;
using JsonExtractor.Helpers;
using JsonExtractor.Interfaces;
using JsonExtractor.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Services;

public static class ServiceContainer
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Register core services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IJsonExtractorService, JsonExtractorService>();
        services.AddSingleton<IJsonQueryService, JsonQueryService>();
        services.AddSingleton<ICommandProcessor, CommandProcessor>();
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<ProgramHelper>();

        // Register commands
        services.AddSingleton<ICommand, ParseCommand>();
        services.AddSingleton<ICommand, QueryCommand>();
        services.AddSingleton<ICommand, FormatCommand>();
        services.AddSingleton<ICommand, ExportCommand>();
        services.AddSingleton<ICommand, HelpCommand>();

        return services.BuildServiceProvider();
    }

    public static IServiceProvider ConfigureServicesWithConfiguration(JsonExtractorConfiguration configuration)
    {
        var services = new ServiceCollection();

        // Configure logging based on configuration
        services.AddLogging(builder =>
        {
            if (configuration.Logging.EnableConsoleLogging)
                builder.AddConsole();

            var logLevel = Enum.TryParse<LogLevel>(configuration.Logging.LogLevel, out var level)
                ? level
                : LogLevel.Information;
            builder.SetMinimumLevel(logLevel);
        });

        // Register configuration
        services.AddSingleton(configuration);

        // Register core services
        services.AddSingleton<IConfigurationService, ConfigurationService>(provider =>
            new ConfigurationService(provider.GetRequiredService<ILogger<ConfigurationService>>()));
        services.AddSingleton<IJsonExtractorService, JsonExtractorService>();
        services.AddSingleton<IJsonQueryService, JsonQueryService>();
        services.AddSingleton<ICommandProcessor, CommandProcessor>();
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<ProgramHelper>();

        // Register commands
        services.AddSingleton<ICommand, ParseCommand>();
        services.AddSingleton<ICommand, QueryCommand>();
        services.AddSingleton<ICommand, FormatCommand>();
        services.AddSingleton<ICommand, ExportCommand>();
        services.AddSingleton<ICommand, HelpCommand>();

        return services.BuildServiceProvider();
    }
}