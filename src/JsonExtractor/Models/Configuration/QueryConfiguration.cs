namespace JsonExtractor.Models.Configuration;

public class QueryConfiguration
{
    public int DefaultTimeout { get; init; } = 30;
    public int MaxResults { get; init; } = 10000;
    public bool EnableCaching { get; init; } = false;
    public int CacheDuration { get; init; } = 300;
}