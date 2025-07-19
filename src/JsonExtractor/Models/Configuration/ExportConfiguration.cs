namespace JsonExtractor.Models.Configuration;

public class ExportConfiguration
{
    public string DefaultFormat { get; init; } = "json";
    public bool SortResultsByKey { get; init; } = true;
    public bool IncludeEmptyFields { get; init; } = false;
    public string DateFormat { get; init; } = "yyyy-MM-dd";
    public string TimeFormat { get; init; } = "HH:mm:ss";
    public string DateTimeFormat { get; init; } = "yyyy-MM-dd HH:mm:ss";
}