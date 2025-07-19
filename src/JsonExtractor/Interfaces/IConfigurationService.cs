using JsonExtractor.Models.Configuration;

public interface IConfigurationService
{
    JsonExtractorConfiguration GetConfiguration();
    T GetSection<T>(string sectionName) where T : new();
    void UpdateConfiguration(JsonExtractorConfiguration configuration);
    void SaveConfiguration();
    void LoadConfiguration(string? filePath = null);
}