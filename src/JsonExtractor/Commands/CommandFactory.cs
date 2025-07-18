using JsonExtractor.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Commands;

public class CommandFactory : ICommandFactory
{
    private static readonly Dictionary<string, Type> CommandTypes = new()
    {
        { "parse", typeof(ParseCommand) },
        { "query", typeof(QueryCommand) },
        { "format", typeof(FormatCommand) },
        { "export", typeof(ExportCommand) },
        { "help", typeof(HelpCommand) }
    };

    private readonly IJsonExtractorService _extractorService;
    private readonly ILogger<CommandFactory> _logger;
    private readonly IJsonQueryService _queryService;
    private readonly IServiceProvider _serviceProvider;

    public CommandFactory(
        IJsonExtractorService extractorService,
        IJsonQueryService queryService,
        IServiceProvider serviceProvider,
        ILogger<CommandFactory> logger)
    {
        _extractorService = extractorService ?? throw new ArgumentNullException(nameof(extractorService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ICommand CreateCommand(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("Command name cannot be null or empty", nameof(commandName));

        var normalizedName = commandName.ToLowerInvariant();

        if (CommandTypes.TryGetValue(normalizedName, out var commandType))
            try
            {
                var command = CreateCommandInstance(commandType);
                _logger.LogDebug("Created command: {CommandName}", commandName);
                return command;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating command: {CommandName}", commandName);
                throw new InvalidOperationException($"Failed to create command: {commandName}", ex);
            }

        throw new ArgumentException($"Unsupported command: {commandName}");
    }

    public ICommand? TryCreateCommand(string commandName)
    {
        try
        {
            return CreateCommand(commandName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create command: {CommandName}", commandName);
            return null;
        }
    }

    public string[] GetSupportedCommands()
    {
        return CommandTypes.Keys.ToArray();
    }

    public string GetSupportedCommandsString()
    {
        return string.Join(", ", CommandTypes.Keys);
    }

    private ICommand CreateCommandInstance(Type commandType)
    {
        return commandType.Name switch
        {
            nameof(ParseCommand) => new ParseCommand(_extractorService,
                _serviceProvider.GetRequiredService<ILogger<ParseCommand>>()),
            nameof(QueryCommand) => new QueryCommand(_extractorService, _queryService,
                _serviceProvider.GetRequiredService<ILogger<QueryCommand>>()),
            nameof(FormatCommand) => new FormatCommand(_extractorService,
                _serviceProvider.GetRequiredService<ILogger<FormatCommand>>()),
            nameof(ExportCommand) => new ExportCommand(_extractorService, _queryService,
                _serviceProvider.GetRequiredService<ILogger<ExportCommand>>()),
            nameof(HelpCommand) => new HelpCommand(_serviceProvider,
                _serviceProvider.GetRequiredService<ILogger<HelpCommand>>()),
            _ => throw new InvalidOperationException($"Unknown command type: {commandType.Name}")
        };
    }
}