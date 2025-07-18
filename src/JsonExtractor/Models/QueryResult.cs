using System.Text.Json;

namespace JsonExtractor.Models;

public record QueryResult
{
    public bool Success { get; private init; }
    public JsonElement? Result { get; init; }
    public IReadOnlyList<JsonElement> Results { get; private init; } = Array.Empty<JsonElement>();
    public string? ErrorMessage { get; private init; }
    public string Query { get; init; } = string.Empty;
    public TimeSpan ExecutionTime { get; private init; }

    public static QueryResult CreateSuccess(JsonElement result, string query, TimeSpan executionTime)
    {
        return new QueryResult
        {
            Success = true, Result = result, Results = new[] { result }, Query = query, ExecutionTime = executionTime
        };
    }

    public static QueryResult CreateSuccess(IReadOnlyList<JsonElement> results, string query, TimeSpan executionTime)
    {
        return new QueryResult { Success = true, Results = results, Query = query, ExecutionTime = executionTime };
    }

    public static QueryResult CreateError(string errorMessage, string query)
    {
        return new QueryResult { Success = false, ErrorMessage = errorMessage, Query = query };
    }
}