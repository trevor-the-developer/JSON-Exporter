using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Commands;

public class ParseCommand : ICommand
{
    private readonly IJsonExtractorService _extractorService;
    private readonly ILogger<ParseCommand> _logger;

    public ParseCommand(IJsonExtractorService extractorService, ILogger<ParseCommand> logger)
    {
        _extractorService = extractorService ?? throw new ArgumentNullException(nameof(extractorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "parse";
    public string Description => "Parse and format JSON from string or file";

    public async Task<CommandResult> ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
                return CommandResult.CreateError("Usage: parse <json-string> OR parse --file <file-path>");

            var options = new ExtractorOptions { PrettyPrint = true };

            if (args[0] == "--file" || args[0] == "-f")
            {
                if (args.Length < 2) return CommandResult.CreateError("File path required when using --file option");

                return await _extractorService.ParseJsonFromFileAsync(args[1], options);
            }

            var json = string.Join(" ", args);
            return await _extractorService.ParseJsonAsync(json, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ParseCommand");
            return CommandResult.CreateError($"Parse command error: {ex.Message}", ex);
        }
    }
}