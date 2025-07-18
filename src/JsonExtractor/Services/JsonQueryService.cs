using System.Diagnostics;
using System.Text.Json;
using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Services;

public class JsonQueryService : IJsonQueryService
{
    private readonly AdvancedJsonPathParser _advancedParser;
    private readonly ILogger<JsonQueryService> _logger;

    public JsonQueryService(ILogger<JsonQueryService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _advancedParser = new AdvancedJsonPathParser();
    }

    public QueryResult ExecuteJsonPath(JsonElement rootElement, string jsonPath)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
                return QueryResult.CreateError("JSONPath query cannot be null or empty", jsonPath);

            // Try advanced JSONPath parser first for complex expressions
            if (IsAdvancedJsonPath(jsonPath))
            {
                var advancedResult = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(rootElement, jsonPath);
                stopwatch.Stop();
                _logger.LogInformation("Advanced JSONPath query '{Query}' returned {Count} results in {Duration}ms",
                    jsonPath, advancedResult.Results.Count, stopwatch.ElapsedMilliseconds);
                return advancedResult;
            }

            // Fall back to simple JSONPath for basic queries
            var results = ExecuteSimpleJsonPath(rootElement, jsonPath);
            stopwatch.Stop();

            _logger.LogInformation("JSONPath query '{Query}' returned {Count} results in {Duration}ms",
                jsonPath, results.Count, stopwatch.ElapsedMilliseconds);

            return QueryResult.CreateSuccess(results, jsonPath, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing JSONPath query '{Query}'", jsonPath);
            return QueryResult.CreateError($"JSONPath error: {ex.Message}", jsonPath);
        }
    }

    public QueryResult ExecuteMultipleQueries(JsonElement rootElement, IEnumerable<string> queries)
    {
        var stopwatch = Stopwatch.StartNew();
        var allResults = new List<JsonElement>();
        var queryList = queries.ToList();

        try
        {
            foreach (var query in queryList)
            {
                var result = ExecuteJsonPath(rootElement, query);
                if (result.Success)
                    allResults.AddRange(result.Results);
                else
                    _logger.LogWarning("Query '{Query}' failed: {Error}", query, result.ErrorMessage);
            }

            stopwatch.Stop();
            var combinedQuery = string.Join(" | ", queryList);

            return QueryResult.CreateSuccess(allResults, combinedQuery, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return QueryResult.CreateError($"Multiple queries error: {ex.Message}", string.Join(" | ", queryList));
        }
    }

    public QueryResult FindByKey(JsonElement rootElement, string key, bool caseSensitive = true)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<JsonElement>();

        try
        {
            FindByKeyRecursive(rootElement, key, caseSensitive, results);
            stopwatch.Stop();

            var query = $"FindByKey('{key}', caseSensitive: {caseSensitive})";
            return QueryResult.CreateSuccess(results, query, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return QueryResult.CreateError($"FindByKey error: {ex.Message}", key);
        }
    }

    public QueryResult FindByValue(JsonElement rootElement, object value, bool caseSensitive = true)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<JsonElement>();

        try
        {
            FindByValueRecursive(rootElement, value, caseSensitive, results);
            stopwatch.Stop();

            var query = $"FindByValue('{value}', caseSensitive: {caseSensitive})";
            return QueryResult.CreateSuccess(results, query, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return QueryResult.CreateError($"FindByValue error: {ex.Message}", value?.ToString() ?? "null");
        }
    }

    public QueryResult GetArrayElements(JsonElement rootElement, string arrayPath, int? skip = null, int? take = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var arrayResult = ExecuteJsonPath(rootElement, arrayPath);
            if (!arrayResult.Success || !arrayResult.Results.Any())
                return QueryResult.CreateError($"Array not found at path: {arrayPath}", arrayPath);

            var arrayElement = arrayResult.Results.First();
            if (arrayElement.ValueKind != JsonValueKind.Array)
                return QueryResult.CreateError($"Element at path '{arrayPath}' is not an array", arrayPath);

            var elements = arrayElement.EnumerateArray().AsEnumerable();

            if (skip.HasValue)
                elements = elements.Skip(skip.Value);

            if (take.HasValue)
                elements = elements.Take(take.Value);

            var results = elements.ToList();
            stopwatch.Stop();

            var query = $"GetArrayElements('{arrayPath}', skip: {skip}, take: {take})";
            return QueryResult.CreateSuccess(results, query, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return QueryResult.CreateError($"GetArrayElements error: {ex.Message}", arrayPath);
        }
    }

    public QueryResult FilterArray(JsonElement rootElement, string arrayPath, Func<JsonElement, bool> predicate)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var arrayResult = GetArrayElements(rootElement, arrayPath);
            if (!arrayResult.Success) return arrayResult;

            var filteredResults = arrayResult.Results.Where(predicate).ToList();
            stopwatch.Stop();

            var query = $"FilterArray('{arrayPath}', custom_predicate)";
            return QueryResult.CreateSuccess(filteredResults, query, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return QueryResult.CreateError($"FilterArray error: {ex.Message}", arrayPath);
        }
    }

    private static void FindByKeyRecursive(JsonElement element, string key, bool caseSensitive,
        List<JsonElement> results)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var matches = caseSensitive
                        ? property.Name.Equals(key, StringComparison.Ordinal)
                        : property.Name.Equals(key, StringComparison.OrdinalIgnoreCase);

                    if (matches) results.Add(property.Value);

                    FindByKeyRecursive(property.Value, key, caseSensitive, results);
                }

                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray()) FindByKeyRecursive(item, key, caseSensitive, results);
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void FindByValueRecursive(JsonElement element, object value, bool caseSensitive,
        List<JsonElement> results)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var stringValue = element.GetString();
                var targetString = value?.ToString();
                if (stringValue != null && targetString != null)
                {
                    var matches = caseSensitive
                        ? stringValue.Equals(targetString, StringComparison.Ordinal)
                        : stringValue.Equals(targetString, StringComparison.OrdinalIgnoreCase);

                    if (matches) results.Add(element);
                }

                break;

            case JsonValueKind.Number:
                if (value is int intValue && element.TryGetInt32(out var elementInt) && elementInt == intValue)
                    results.Add(element);
                else if (value is double doubleValue && element.TryGetDouble(out var elementDouble) &&
                         Math.Abs(elementDouble - doubleValue) < 0.0001)
                    results.Add(element);
                break;

            case JsonValueKind.True:
                if (value is bool boolValue && boolValue)
                    results.Add(element);
                break;

            case JsonValueKind.False:
                if (value is bool boolValueFalse && !boolValueFalse)
                    results.Add(element);
                break;

            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    FindByValueRecursive(property.Value, value, caseSensitive, results);
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    FindByValueRecursive(item, value, caseSensitive, results);
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static List<JsonElement> ExecuteSimpleJsonPath(JsonElement rootElement, string jsonPath)
    {
        var results = new List<JsonElement>();

        // Handle root element
        if (jsonPath == "$")
        {
            results.Add(rootElement);
            return results;
        }

        // Remove the $ prefix if present
        if (jsonPath.StartsWith("$."))
            jsonPath = jsonPath.Substring(2);
        else if (jsonPath.StartsWith("$")) jsonPath = jsonPath.Substring(1);

        // Split path into parts
        var pathParts = jsonPath.Split('.');

        // Start traversal from root
        var currentElements = new List<JsonElement> { rootElement };

        foreach (var part in pathParts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            var nextElements = new List<JsonElement>();

            foreach (var element in currentElements)
                // Handle array indexing [0], [*], etc.
                if (part.Contains('['))
                {
                    var propertyName = part.Substring(0, part.IndexOf('['));
                    var indexPart = part.Substring(part.IndexOf('[') + 1,
                        part.LastIndexOf(']') - part.IndexOf('[') - 1);

                    var targetElement = element;

                    // If there's a property name, navigate to it first
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        if (element.ValueKind == JsonValueKind.Object &&
                            element.TryGetProperty(propertyName, out var prop))
                            targetElement = prop;
                        else
                            continue;
                    }

                    // Handle array access
                    if (targetElement.ValueKind != JsonValueKind.Array) continue;
                    if (indexPart == "*")
                    {
                        // Add all array elements
                        nextElements.AddRange(targetElement.EnumerateArray());
                    }
                    else if (int.TryParse(indexPart, out var index))
                    {
                        // Add specific array element
                        var arrayElements = targetElement.EnumerateArray().ToList();
                        if (index >= 0 && index < arrayElements.Count) nextElements.Add(arrayElements[index]);
                    }
                }
                else
                {
                    // Handle simple property access
                    if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(part, out var property))
                        nextElements.Add(property);
                }

            currentElements = nextElements;
        }

        return currentElements;
    }

    private static bool IsAdvancedJsonPath(string jsonPath)
    {
        // Check for advanced JSONPath features
        return jsonPath.Contains("..") || // Recursive descent
               jsonPath.Contains("[?") || // Filters
               jsonPath.Contains(':') || // Array slices
               jsonPath.Contains(',') || // Multiple indices
               jsonPath.Contains("()") || // Functions
               jsonPath.Count(c => c == '[') > 1; // Complex array operations
    }
}