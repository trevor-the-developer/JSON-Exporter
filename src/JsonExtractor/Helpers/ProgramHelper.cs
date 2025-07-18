using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Helpers;

public class ProgramHelper
{
    private readonly ICommandFactory _commandFactory;
    private readonly ICommandProcessor _commandProcessor;
    private readonly ILogger<ProgramHelper> _logger;

    public ProgramHelper(
        ICommandProcessor commandProcessor,
        ICommandFactory commandFactory,
        ILogger<ProgramHelper> logger)
    {
        _commandProcessor = commandProcessor ?? throw new ArgumentNullException(nameof(commandProcessor));
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommandResult> ProcessCommandAsync(string[] args)
    {
        try
        {
            if (args.Length == 0) return await ShowHelpAsync();

            var result = await _commandProcessor.ProcessCommandAsync(args);
            LogResult(result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command");
            return CommandResult.CreateError($"Command processing error: {ex.Message}", ex);
        }
    }

    public async Task<CommandResult> ShowHelpAsync()
    {
        try
        {
            var helpCommand = _commandFactory.CreateCommand("help");
            return await helpCommand.ExecuteAsync([]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing help");
            return CommandResult.CreateError("Help command failed", ex);
        }
    }

    public void ParseArguments(string[] args)
    {
        try
        {
            var result = ProcessCommandAsync(args).GetAwaiter().GetResult();
            DisplayResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing arguments");
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    public void DisplayResult(CommandResult result)
    {
        try
        {
            if (result.Success)
            {
                if (result.Data != null) Console.WriteLine(result.Data);
                if (!string.IsNullOrEmpty(result.Message)) Console.WriteLine($"\n{result.Message}");
            }
            else
            {
                Console.Error.WriteLine($"Error: {result.Message}");
                if (result.Exception != null) Console.Error.WriteLine($"Details: {result.Exception.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying result");
            Console.Error.WriteLine($"Display error: {ex.Message}");
        }
    }

    public void DisplayWelcome()
    {
        Console.WriteLine("JSON Extractor Console App (.NET 9.0)");
        Console.WriteLine("======================================");
        Console.WriteLine("A powerful tool for parsing, querying, and manipulating JSON data.");
        Console.WriteLine($"Available commands: {_commandFactory.GetSupportedCommandsString()}");
        Console.WriteLine();
    }

    public static void DisplayUsage()
    {
        Console.WriteLine("Usage: JsonExtractor <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  parse     Parse and format JSON from string or file");
        Console.WriteLine("  query     Query JSON data using JSONPath expressions");
        Console.WriteLine("  format    Format and prettify JSON");
        Console.WriteLine("  help      Show help information");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  JsonExtractor parse '{\"name\":\"John\",\"age\":30}'");
        Console.WriteLine("  JsonExtractor query '{\"users\":[{\"name\":\"John\"}]}' '$.users[*].name'");
        Console.WriteLine("  JsonExtractor format compact.json");
        Console.WriteLine();
        Console.WriteLine("For detailed help, use: JsonExtractor help");
    }

    private void LogResult(CommandResult result)
    {
        if (result.Success)
        {
            _logger.LogInformation("Command executed successfully");
        }
        else
        {
            _logger.LogError("Command failed: {Message}", result.Message);
            if (result.Exception != null) _logger.LogError(result.Exception, "Command exception details");
        }
    }
}