namespace JsonExtractor.Models.Configuration;

public class JsonExtractorConfiguration
{
    public LoggingConfiguration Logging { get; init; } = new();
    public JsonProcessingConfiguration JsonProcessing { get; init; } = new();
    public QueryConfiguration Query { get; init; } = new();
    public ExportConfiguration Export { get; init; } = new();
    public PerformanceConfiguration Performance { get; init; } = new();
}