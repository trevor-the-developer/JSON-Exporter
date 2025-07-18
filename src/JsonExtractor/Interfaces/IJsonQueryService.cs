using System.Text.Json;
using JsonExtractor.Models;

namespace JsonExtractor.Interfaces;

public interface IJsonQueryService
{
    QueryResult ExecuteJsonPath(JsonElement rootElement, string jsonPath);
    QueryResult ExecuteMultipleQueries(JsonElement rootElement, IEnumerable<string> queries);
    QueryResult FindByKey(JsonElement rootElement, string key, bool caseSensitive = true);
    QueryResult FindByValue(JsonElement rootElement, object value, bool caseSensitive = true);
    QueryResult GetArrayElements(JsonElement rootElement, string arrayPath, int? skip = null, int? take = null);
    QueryResult FilterArray(JsonElement rootElement, string arrayPath, Func<JsonElement, bool> predicate);
}