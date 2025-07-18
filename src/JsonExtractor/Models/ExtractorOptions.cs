using System.Text.Json;

namespace JsonExtractor.Models;

public record ExtractorOptions
{
    public bool PrettyPrint { get; init; } = true;
    public bool CaseSensitive { get; init; } = true;
    public bool AllowComments { get; init; } = true;
    public bool AllowTrailingCommas { get; init; } = true;
    public int MaxDepth { get; init; } = 1000;
    public JsonNamingPolicy? PropertyNamingPolicy { get; init; }
}