using JsonExtractor.Models.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonExtractor.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private readonly ILogger<ConfigurationService> _logger;
    private JsonExtractorConfiguration _configuration;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        _configuration = new JsonExtractorConfiguration();
        LoadConfiguration();
    }

    public JsonExtractorConfiguration GetConfiguration()
    {
        return _configuration;
    }

    public T GetSection<T>(string sectionName) where T : new()
    {
        try
        {
            var property = typeof(JsonExtractorConfiguration).GetProperty(sectionName);
            if (property != null)
            {
                var value = property.GetValue(_configuration);
                return value is T result ? result : new T();
            }

            _logger.LogWarning("Configuration section '{SectionName}' not found", sectionName);
            return new T();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration section '{SectionName}'", sectionName);
            return new T();
        }
    }

    public void UpdateConfiguration(JsonExtractorConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger.LogInformation("Configuration updated");
    }

    public void SaveConfiguration()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_configuration, Formatting.Indented);
            File.WriteAllText(_configFilePath, json);
            _logger.LogInformation("Configuration saved to {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration to {FilePath}", _configFilePath);
        }
    }

    public void LoadConfiguration(string? filePath = null)
    {
        var targetPath = filePath ?? _configFilePath;
        try
        {
            if (File.Exists(targetPath))
            {
                var json = File.ReadAllText(targetPath);
                var config = JsonConvert.DeserializeObject<JsonExtractorConfiguration>(json);
                _configuration = config ?? new JsonExtractorConfiguration();
                _logger.LogInformation("Configuration loaded from {FilePath}", targetPath);
            }
            else
            {
                _logger.LogWarning("Configuration file not found at {FilePath}, using defaults", targetPath);
                _configuration = new JsonExtractorConfiguration();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration from {FilePath}, using defaults", targetPath);
            _configuration = new JsonExtractorConfiguration();
        }
    }
}