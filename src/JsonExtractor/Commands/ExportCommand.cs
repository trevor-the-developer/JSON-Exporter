using System.Text.Json;
using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using JsonExtractor.Utilities;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Commands;

public class ExportCommand : ICommand
{
    private readonly IJsonExtractorService _extractorService;
    private readonly ILogger<ExportCommand> _logger;
    private readonly IJsonQueryService _queryService;

    public ExportCommand(
        IJsonExtractorService extractorService,
        IJsonQueryService queryService,
        ILogger<ExportCommand> logger)
    {
        _extractorService = extractorService ?? throw new ArgumentNullException(nameof(extractorService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "export";
    public string Description => "Export JSON data to various formats (CSV, XML, JSON)";

    public async Task<CommandResult> ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length < 2)
                return CommandResult.CreateError("Usage: export <json> <format> [query] [options]\n" +
                                                 "Formats: csv, xml, json\n" +
                                                 "Examples:\n" +
                                                 "  export '{\"users\":[{\"name\":\"John\",\"age\":30}]}' csv\n" +
                                                 "  export '{\"data\":{\"items\":[1,2,3]}}' xml '$.data.items[*]'\n" +
                                                 "  export '{\"products\":[{\"name\":\"A\",\"price\":10}]}' csv '$.products[*]' --delimiter=';'");

            var json = args[0];
            var format = args[1].ToLowerInvariant();
            var query = args.Length > 2 ? args[2] : null;
            var options = ParseOptions(args.Skip(3).ToArray());

            // Parse the JSON
            var parseResult = await _extractorService.ParseJsonDocumentAsync(json);
            if (!parseResult.Success) return CommandResult.CreateError($"Failed to parse JSON: {parseResult.Message}");

            var jsonDocument = parseResult.Data as JsonDocument;
            if (jsonDocument == null) return CommandResult.CreateError("Invalid JSON document");

            // Apply query if provided
            IEnumerable<JsonElement> elements;
            if (!string.IsNullOrEmpty(query))
            {
                var queryResult = _queryService.ExecuteJsonPath(jsonDocument.RootElement, query);
                if (!queryResult.Success) return CommandResult.CreateError($"Query failed: {queryResult.ErrorMessage}");

                elements = queryResult.Results;
            }
            else
            {
                elements = new[] { jsonDocument.RootElement };
            }

            // Export to the specified format
            var exportResult = ExportToFormat(elements, format, options);

            _logger.LogInformation("Successfully exported to {Format} format", format.ToUpper());
            return CommandResult.CreateSuccess($"Export completed successfully to {format.ToUpper()} format",
                exportResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during export operation");
            return CommandResult.CreateError($"Export failed: {ex.Message}", ex);
        }
    }

    private string ExportToFormat(IEnumerable<JsonElement> elements, string format, Dictionary<string, string> options)
    {
        return format switch
        {
            "csv" => ExportToCsv(elements, options),
            "xml" => ExportToXml(elements, options),
            "json" => ExportToJson(elements, options),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    private static string ExportToCsv(IEnumerable<JsonElement> elements, Dictionary<string, string> options)
    {
        var delimiter = options.GetValueOrDefault("delimiter", ",");
        return CsvExporter.ExportToCsv(elements, delimiter);
    }

    private static string ExportToXml(IEnumerable<JsonElement> elements, Dictionary<string, string> options)
    {
        var rootElement = options.GetValueOrDefault("root", "Results");
        var itemElement = options.GetValueOrDefault("item", "Item");
        return XmlExporter.ExportToXml(elements, rootElement, itemElement);
    }

    private string ExportToJson(IEnumerable<JsonElement> elements, Dictionary<string, string> options)
    {
        var indent = options.ContainsKey("indent") && options["indent"] != "false";
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = indent,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var array = elements.ToArray();
        return array.Length == 1
            ? JsonSerializer.Serialize(array[0], jsonOptions)
            : JsonSerializer.Serialize(array, jsonOptions);
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>();

        foreach (var arg in args)
        {
            if (!arg.StartsWith("--")) continue;
            var parts = arg[2..].Split('=', 2);
            if (parts.Length == 2)
                options[parts[0]] = parts[1];
            else
                options[parts[0]] = "true";
        }

        return options;
    }
}