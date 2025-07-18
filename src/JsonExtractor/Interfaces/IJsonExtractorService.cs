using JsonExtractor.Models;

namespace JsonExtractor.Interfaces;

public interface IJsonExtractorService
{
    Task<CommandResult> ParseJsonAsync(string json, ExtractorOptions? options = null);
    Task<CommandResult> ParseJsonFromFileAsync(string filePath, ExtractorOptions? options = null);
    Task<CommandResult> FormatJsonAsync(string json, ExtractorOptions? options = null);
    Task<CommandResult> ValidateJsonAsync(string json);
    CommandResult ConvertToObject<T>(string json, ExtractorOptions? options = null) where T : class;
    CommandResult SerializeObject<T>(T obj, ExtractorOptions? options = null) where T : class;
    Task<CommandResult> ParseJsonDocumentAsync(string json, ExtractorOptions? options = null);
}