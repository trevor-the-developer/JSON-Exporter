namespace JsonExtractor.Models.Configuration;

public class JsonProcessingConfiguration
{
    public int DefaultIndentSize { get; init; } = 2;
    public bool EnablePrettyPrinting { get; init; } = true;
    public int MaxJsonSize { get; init; } = 104857600; // 100MB
    public bool EnableValidation { get; init; } = true;
    public int MaxDepth { get; init; } = 64;
}