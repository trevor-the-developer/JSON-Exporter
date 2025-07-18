namespace JsonExtractor.Models.Configuration;

public class JsonExtractorConfiguration
{
    public LoggingConfiguration Logging { get; init; } = new();
    public JsonProcessingConfiguration JsonProcessing { get; init; } = new();
    public QueryConfiguration Query { get; init; } = new();
    public ExportConfiguration Export { get; init; } = new();
    public PerformanceConfiguration Performance { get; init; } = new();
}

public class LoggingConfiguration
{
    public string LogLevel { get; init; } = "Information";
    public bool EnableConsoleLogging { get; init; } = true;
    public bool EnableFileLogging { get; init; } = false;
    public string? LogFilePath { get; init; }
    public bool EnableStructuredLogging { get; init; } = true;
}

public class JsonProcessingConfiguration
{
    public int DefaultIndentSize { get; init; } = 2;
    public bool EnablePrettyPrinting { get; init; } = true;
    public int MaxJsonSize { get; init; } = 104857600; // 100MB
    public bool EnableValidation { get; init; } = true;
    public int MaxDepth { get; init; } = 64;
}

public class QueryConfiguration
{
    public int DefaultTimeout { get; init; } = 30;
    public int MaxResults { get; init; } = 10000;
    public bool EnableCaching { get; init; } = false;
    public int CacheDuration { get; init; } = 300;
}

public class ExportConfiguration
{
    public string DefaultFormat { get; init; } = "json";
    public bool SortResultsByKey { get; init; } = true;
    public bool IncludeEmptyFields { get; init; } = false;
    public string DateFormat { get; init; } = "yyyy-MM-dd";
    public string TimeFormat { get; init; } = "HH:mm:ss";
    public string DateTimeFormat { get; init; } = "yyyy-MM-dd HH:mm:ss";
}

public class PerformanceConfiguration
{
    public bool EnableAsyncProcessing { get; init; } = true;
    public int MaxConcurrentOperations { get; init; } = 4;
    public int TimeoutSeconds { get; init; } = 300;
    public bool EnableStreaming { get; init; } = false;
}