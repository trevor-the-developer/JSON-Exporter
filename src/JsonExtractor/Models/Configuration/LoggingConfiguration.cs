namespace JsonExtractor.Models.Configuration;

public class LoggingConfiguration
{
    public string LogLevel { get; init; } = "Information";
    public bool EnableConsoleLogging { get; init; } = true;
    public bool EnableFileLogging { get; init; } = false;
    public string? LogFilePath { get; init; }
    public bool EnableStructuredLogging { get; init; } = true;
}