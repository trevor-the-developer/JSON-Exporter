using System.Text.Json;
using System.Text.RegularExpressions;
using JsonExtractor.Helpers;
using JsonExtractor.Models;

namespace JsonExtractor.Services;

public class AdvancedJsonPathParser
{
    public static QueryResult ExecuteAdvancedJsonPath(JsonElement rootElement, string jsonPath)
    {
        try
        {
            var startTime = DateTime.UtcNow;

            if (string.IsNullOrEmpty(jsonPath) || jsonPath == "$")
                return QueryResult.CreateSuccess(rootElement, jsonPath, DateTime.UtcNow - startTime);
            
            var path = jsonPath.StartsWith($"$") ? jsonPath[1..] : jsonPath;

            var results = new List<JsonElement> { rootElement };
            var segments = JsonParserHelper.
                ParsePathSegments(path);

            foreach (var segment in segments)
            {
                results = JsonParserHelper.
                    ApplySegment(results, segment);
                if (results.Count == 0)
                    break;
            }

            var executionTime = DateTime.UtcNow - startTime;
            return results.Count == 1
                ? QueryResult.CreateSuccess(results[0], jsonPath, executionTime)
                : QueryResult.CreateSuccess(results.AsReadOnly(), jsonPath, executionTime);
        }
        catch (Exception ex)
        {
            return QueryResult.CreateError($"JSONPath execution error: {ex.Message}", jsonPath);
        }
    }
}