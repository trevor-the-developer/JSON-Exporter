using System.Text.Json;
using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Commands;

public class QueryCommand : ICommand
{
    private readonly IJsonExtractorService _extractorService;
    private readonly ILogger<QueryCommand> _logger;
    private readonly IJsonQueryService _queryService;

    public QueryCommand(IJsonExtractorService extractorService, IJsonQueryService queryService,
        ILogger<QueryCommand> logger)
    {
        _extractorService = extractorService ?? throw new ArgumentNullException(nameof(extractorService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "query";
    public string Description => "Query JSON data using JSONPath expressions";

    public async Task<CommandResult> ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length < 2)
                return CommandResult.CreateError(
                    "Usage: query <json-string> <jsonpath-query> OR query --file <file-path> <jsonpath-query>");

            JsonElement rootElement;
            string query;

            if (args[0] == "--file" || args[0] == "-f")
            {
                if (args.Length < 3)
                    return CommandResult.CreateError("File path and query required when using --file option");

                var parseResult = await _extractorService.ParseJsonFromFileAsync(args[1]);
                if (!parseResult.Success) return parseResult;

                using var document = JsonDocument.Parse(parseResult.Data?.ToString() ?? "{}");
                rootElement = document.RootElement.Clone();
                query = args[2];
            }
            else
            {
                var parseResult = await _extractorService.ParseJsonAsync(args[0]);
                if (!parseResult.Success) return parseResult;

                using var document = JsonDocument.Parse(parseResult.Data?.ToString() ?? "{}");
                rootElement = document.RootElement.Clone();
                query = args[1];
            }

            var queryResult = _queryService.ExecuteJsonPath(rootElement, query);

            if (!queryResult.Success) return CommandResult.CreateError(queryResult.ErrorMessage ?? "Query failed");
            var options = new JsonSerializerOptions { WriteIndented = true };
            var resultsJson = queryResult.Results.Any()
                ? JsonSerializer.Serialize(queryResult.Results, options)
                : "[]";

            return CommandResult.CreateSuccess(
                $"Query executed successfully. Found {queryResult.Results.Count} results in {queryResult.ExecutionTime.TotalMilliseconds:F2}ms",
                resultsJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in QueryCommand");
            return CommandResult.CreateError($"Query command error: {ex.Message}", ex);
        }
    }
}