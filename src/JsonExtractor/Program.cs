using JsonExtractor.Helpers;
using JsonExtractor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonExtractor;

internal static class Program
{
    private static IServiceProvider _serviceProvider = null!;
    private static ILogger _logger = null!;
    private static ProgramHelper _programHelper = null!;

    private static async Task<int> Main(string[] args)
    {
        // Initialize services
        _serviceProvider = ServiceContainer.ConfigureServices();
        _logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
        _programHelper = _serviceProvider.GetRequiredService<ProgramHelper>();

        _logger.LogInformation("JSON Extractor Console App (.NET 9.0) - Starting");
        _logger.LogInformation("Supports: JSON parsing, querying, and formatting");

        try
        {
            _programHelper.DisplayWelcome();

            if (args.Length == 0)
            {
                ProgramHelper.DisplayUsage();
                Console.WriteLine();
                Console.Write("Enter command: ");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("No command specified. Showing help...");
                    var helpResult = await _programHelper.ShowHelpAsync();
                    _programHelper.DisplayResult(helpResult);
                    return 0;
                }

                args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }

            var result = await _programHelper.ProcessCommandAsync(args);
            _programHelper.DisplayResult(result);

            return result.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in main");
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
        finally
        {
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}