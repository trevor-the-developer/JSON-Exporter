using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;

namespace JsonExtractor;

public class CommandProcessor : ICommandProcessor
{
    private readonly IEnumerable<ICommand> _commands;
    private readonly ILogger<CommandProcessor> _logger;

    public CommandProcessor(IEnumerable<ICommand> commands, ILogger<CommandProcessor> logger)
    {
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommandResult> ProcessCommandAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                var helpCommand = _commands.FirstOrDefault(c => c.Name == "help");
                return await (helpCommand?.ExecuteAsync(args) ??
                              Task.FromResult(CommandResult.CreateError("No help command available")));
            }

            var commandName = args[0].ToLowerInvariant();
            var command = _commands.FirstOrDefault(c => c.Name == commandName);

            if (command == null)
                return CommandResult.CreateError(
                    $"Unknown command: {commandName}. Use 'help' to see available commands.");

            var commandArgs = args.Skip(1).ToArray();
            return await command.ExecuteAsync(commandArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command");
            return CommandResult.CreateError($"Command processing error: {ex.Message}", ex);
        }
    }
}