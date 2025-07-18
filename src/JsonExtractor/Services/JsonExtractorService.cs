using System.Text.Json;
using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Services;

public class JsonExtractorService : IJsonExtractorService
{
    private readonly ILogger<JsonExtractorService> _logger;

    public JsonExtractorService(ILogger<JsonExtractorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<CommandResult> ParseJsonAsync(string json, ExtractorOptions? options = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return Task.FromResult(CommandResult.CreateError("JSON input cannot be null or empty"));

            var serializerOptions = CreateSerializerOptions(options);
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = options?.AllowTrailingCommas ?? true,
                CommentHandling = options?.AllowComments == true
                    ? JsonCommentHandling.Skip
                    : JsonCommentHandling.Disallow,
                MaxDepth = options?.MaxDepth ?? 1000
            });

            var formattedJson = JsonSerializer.Serialize(document.RootElement, serializerOptions);

            _logger.LogInformation("Successfully parsed JSON with {ElementCount} elements",
                CountElements(document.RootElement));

            return Task.FromResult(CommandResult.CreateSuccess("JSON parsed successfully", formattedJson));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON");
            return Task.FromResult(CommandResult.CreateError($"Invalid JSON: {ex.Message}", ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JSON parsing");
            return Task.FromResult(CommandResult.CreateError($"Unexpected error: {ex.Message}", ex));
        }
    }

    public async Task<CommandResult> ParseJsonFromFileAsync(string filePath, ExtractorOptions? options = null)
    {
        try
        {
            if (!File.Exists(filePath)) return CommandResult.CreateError($"File not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);
            return await ParseJsonAsync(json, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FilePath}", filePath);
            return CommandResult.CreateError($"Error reading file: {ex.Message}", ex);
        }
    }

    public async Task<CommandResult> FormatJsonAsync(string json, ExtractorOptions? options = null)
    {
        try
        {
            var parseResult = await ParseJsonAsync(json, options);
            return !parseResult.Success
                ? parseResult
                : CommandResult.CreateSuccess("JSON formatted successfully", parseResult.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting JSON");
            return CommandResult.CreateError($"Error formatting JSON: {ex.Message}", ex);
        }
    }

    public Task<CommandResult> ValidateJsonAsync(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return Task.FromResult(CommandResult.CreateSuccess("JSON is valid", true));
        }
        catch (JsonException ex)
        {
            return Task.FromResult(CommandResult.CreateError($"Invalid JSON: {ex.Message}", ex));
        }
        catch (Exception ex)
        {
            return Task.FromResult(CommandResult.CreateError($"Validation error: {ex.Message}", ex));
        }
    }

    public CommandResult ConvertToObject<T>(string json, ExtractorOptions? options = null) where T : class
    {
        try
        {
            var serializerOptions = CreateSerializerOptions(options);
            var obj = JsonSerializer.Deserialize<T>(json, serializerOptions);
            return CommandResult.CreateSuccess("Object deserialized successfully", obj);
        }
        catch (JsonException ex)
        {
            return CommandResult.CreateError($"Deserialization error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return CommandResult.CreateError($"Unexpected error: {ex.Message}", ex);
        }
    }

    public CommandResult SerializeObject<T>(T obj, ExtractorOptions? options = null) where T : class
    {
        try
        {
            var serializerOptions = CreateSerializerOptions(options);
            var json = JsonSerializer.Serialize(obj, serializerOptions);
            return CommandResult.CreateSuccess("Object serialized successfully", json);
        }
        catch (Exception ex)
        {
            return CommandResult.CreateError($"Serialization error: {ex.Message}", ex);
        }
    }

    public Task<CommandResult> ParseJsonDocumentAsync(string json, ExtractorOptions? options = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return Task.FromResult(CommandResult.CreateError("JSON input cannot be null or empty"));

            var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = options?.AllowTrailingCommas ?? true,
                CommentHandling = options?.AllowComments == true
                    ? JsonCommentHandling.Skip
                    : JsonCommentHandling.Disallow,
                MaxDepth = options?.MaxDepth ?? 1000
            });

            _logger.LogInformation("Successfully parsed JSON document with {ElementCount} elements",
                CountElements(document.RootElement));

            return Task.FromResult(CommandResult.CreateSuccess("JSON document parsed successfully", document));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON document");
            return Task.FromResult(CommandResult.CreateError($"Invalid JSON: {ex.Message}", ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JSON document parsing");
            return Task.FromResult(CommandResult.CreateError($"Unexpected error: {ex.Message}", ex));
        }
    }

    private static JsonSerializerOptions CreateSerializerOptions(ExtractorOptions? options)
    {
        return new JsonSerializerOptions
        {
            WriteIndented = options?.PrettyPrint ?? true,
            PropertyNameCaseInsensitive = !options?.CaseSensitive ?? true,
            AllowTrailingCommas = options?.AllowTrailingCommas ?? true,
            ReadCommentHandling =
                options?.AllowComments == true ? JsonCommentHandling.Skip : JsonCommentHandling.Disallow,
            MaxDepth = options?.MaxDepth ?? 1000,
            PropertyNamingPolicy = options?.PropertyNamingPolicy
        };
    }

    private static int CountElements(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Sum(prop => 1 + CountElements(prop.Value)),
            JsonValueKind.Array => element.EnumerateArray().Sum(item => 1 + CountElements(item)),
            _ => 1
        };
    }
}