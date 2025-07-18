using System.Text;
using System.Text.Json;

namespace JsonExtractor.Utilities;

public static class CsvExporter
{
    public static string ExportToCsv(IEnumerable<JsonElement> elements, string delimiter = ",")
    {
        if (!elements.Any()) return string.Empty;

        var sb = new StringBuilder();
        var headerWritten = false;

        foreach (var element in elements)
        {
            var dictionary = JsonToDictionary(element);

            if (!headerWritten)
            {
                sb.AppendLine(string.Join(delimiter, dictionary.Keys));
                headerWritten = true;
            }

            sb.AppendLine(string.Join(delimiter, dictionary.Values.Select(Escape)));
        }

        return sb.ToString();
    }

    private static Dictionary<string, string> JsonToDictionary(JsonElement element)
    {
        var dictionary = new Dictionary<string, string>();

        foreach (var property in element.EnumerateObject()) dictionary[property.Name] = property.Value.ToString();

        return dictionary;
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        if (value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}