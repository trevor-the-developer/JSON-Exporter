using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Commands;

public class FormatCommand : ICommand
{
    private readonly IJsonExtractorService _extractorService;
    private readonly ILogger<FormatCommand> _logger;

    public FormatCommand(IJsonExtractorService extractorService, ILogger<FormatCommand> logger)
    {
        _extractorService = extractorService ?? throw new ArgumentNullException(nameof(extractorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "format";
    public string Description => "Format and prettify JSON";

    public async Task<CommandResult> ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
                return CommandResult.CreateError("Usage: format <json-string> OR format --file <file-path>");

            var options = new ExtractorOptions { PrettyPrint = true };

            if (args[0] == "--file" || args[0] == "-f")
            {
                if (args.Length < 2) return CommandResult.CreateError("File path required when using --file option");

                return await _extractorService.FormatJsonAsync(await File.ReadAllTextAsync(args[1]), options);
            }

            var json = string.Join(" ", args);
            return await _extractorService.FormatJsonAsync(json, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FormatCommand");
            return CommandResult.CreateError($"Format command error: {ex.Message}", ex);
        }
    }
}