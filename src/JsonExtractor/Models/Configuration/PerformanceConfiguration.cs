namespace JsonExtractor.Models.Configuration;

public class PerformanceConfiguration
{
    public bool EnableAsyncProcessing { get; init; } = true;
    public int MaxConcurrentOperations { get; init; } = 4;
    public int TimeoutSeconds { get; init; } = 300;
    public bool EnableStreaming { get; init; } = false;
}