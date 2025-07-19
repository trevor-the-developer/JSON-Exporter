using System.Text.Json;
using System.Text.RegularExpressions;

namespace JsonExtractor.Helpers;

public static class JsonParserHelper
{
    private static readonly Regex ArrayIndexRegex = new(@"^\[(\d+)\]$");
    private static readonly Regex ArraySliceRegex = new(@"^\[(\d*):(\d*)\]$");
    private static readonly Regex ArrayMultipleRegex = new(@"^\[(\d+(?:,\d+)*)\]$");
    private static readonly Regex FilterRegex = new(@"^\[\?\(\@\.([^\s<>=!]+)\s*([<>=!]+)\s*(.+)\)\]$");
    private static readonly Regex FunctionRegex = new(@"^(\w+)\(\)$");    
    public static List<string> ParsePathSegments(string path)
    {
        var segments = new List<string>();
        var current = "";
        var inBrackets = false;
        var bracketDepth = 0;

        for (var i = 0; i < path.Length; i++)
        {
            var ch = path[i];

            switch (ch)
            {
                case '[':
                    {
                        if (!inBrackets && !string.IsNullOrEmpty(current))
                        {
                            segments.Add(current);
                            current = "";
                        }

                        inBrackets = true;
                        bracketDepth++;
                        current += ch;
                        break;
                    }
                case ']':
                    {
                        current += ch;
                        bracketDepth--;
                        if (bracketDepth == 0)
                        {
                            inBrackets = false;
                            segments.Add(current);
                            current = "";
                        }

                        break;
                    }
                case '.' when !inBrackets:
                    {
                        if (!string.IsNullOrEmpty(current))
                        {
                            segments.Add(current);
                            current = "";
                        }

                        // Skip the dot unless it's part of recursive descent
                        if (i + 1 < path.Length && path[i + 1] == '.')
                        {
                            current = "..";
                            i++; // Skip the next dot
                        }

                        break;
                    }
                default:
                    current += ch;
                    break;
            }
        }

        if (!string.IsNullOrEmpty(current)) segments.Add(current);

        return segments.Where(s => !string.IsNullOrEmpty(s)).ToList();
    }
    
    public static List<JsonElement> ApplySegment(List<JsonElement> elements, string segment)
    {
        var results = new List<JsonElement>();

        foreach (var element in elements) results.AddRange(ApplySegmentToElement(element, segment));

        return results;
    }

    private static List<JsonElement> ApplySegmentToElement(JsonElement element, string segment)
    {
        var results = new List<JsonElement>();

        // Handle recursive descent (..) - this should be followed by a property name
        if (segment == "..")
        {
            results.AddRange(GetAllDescendants(element));
            return results;
        }

        // Handle recursive descent with property name (..property)
        if (segment.StartsWith("..") && segment.Length > 2)
        {
            var propertyName = segment[2..];
            results.AddRange(FindPropertyRecursively(element, propertyName));
            return results;
        }

        // Handle wildcard (*)
        if (segment == "*")
        {
            results.AddRange(GetAllChildren(element));
            return results;
        }

        // Handle array operations
        if (segment.StartsWith('[') && segment.EndsWith(']'))
        {
            results.AddRange(HandleArrayOperation(element, segment));
            return results;
        }

        // Handle functions
        var functionMatch = FunctionRegex.Match(segment);
        if (functionMatch.Success)
        {
            var functionResult = ExecuteFunction(element, functionMatch.Groups[1].Value);
            if (functionResult.HasValue)
                results.Add(functionResult.Value);
            return results;
        }

        // Handle property access
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(segment, out var property))
            results.Add(property);

        return results;
    }

    private static List<JsonElement> GetAllDescendants(JsonElement element)
    {
        var results = new List<JsonElement> { element };

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject()) results.AddRange(GetAllDescendants(property.Value));
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray()) results.AddRange(GetAllDescendants(item));
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return results;
    }

    private static List<JsonElement> FindPropertyRecursively(JsonElement element, string propertyName)
    {
        var results = new List<JsonElement>();

        // Check current element if it's an object
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property))
            results.Add(property);

        // Recursively search in child elements
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                    results.AddRange(FindPropertyRecursively(prop.Value, propertyName));
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    results.AddRange(FindPropertyRecursively(item, propertyName));
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return results;
    }

    private static List<JsonElement> GetAllChildren(JsonElement element)
    {
        var results = new List<JsonElement>();

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject()) results.Add(property.Value);
                break;
            case JsonValueKind.Array:
                results.AddRange(element.EnumerateArray());
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return results;
    }

    private static List<JsonElement> HandleArrayOperation(JsonElement element, string segment)
    {
        var results = new List<JsonElement>();

        if (element.ValueKind != JsonValueKind.Array)
            return results;

        var array = element.EnumerateArray().ToArray();

        // Handle wildcard [*]
        if (segment == "[*]")
        {
            results.AddRange(array);
            return results;
        }

        // Handle simple array index [n]
        var indexMatch = ArrayIndexRegex.Match(segment);
        if (indexMatch.Success)
        {
            var index = int.Parse(indexMatch.Groups[1].Value as string);
            if (index >= 0 && index < array.Length)
                results.Add(array[index]);
            return results;
        }

        // Handle array slice [start:end]
        var sliceMatch = ArraySliceRegex.Match(segment);
        if (sliceMatch.Success)
        {
            var startStr = sliceMatch.Groups[1].Value;
            var endStr = sliceMatch.Groups[2].Value;

            var start = string.IsNullOrEmpty(startStr) ? 0 : int.Parse(startStr as string);
            var end = string.IsNullOrEmpty(endStr) ? array.Length : int.Parse(endStr as string);

            start = Math.Max(0, start);
            end = Math.Min(array.Length, end);

            for (var i = start; i < end; i++) results.Add(array[i]);
            return results;
        }

        // Handle multiple indices [1,3,5]
        var multipleMatch = ArrayMultipleRegex.Match(segment);
        if (multipleMatch.Success)
        {
            var indices = multipleMatch.Groups[1].Value.Split(',')
                .Select(int.Parse)
                .Where(i => i >= 0 && i < array.Length);

            foreach (var index in indices) results.Add(array[index]);
            return results;
        }

        // Handle filter expressions [?@.property op value]
        var filterMatch = FilterRegex.Match(segment);
        if (!filterMatch.Success) return results;
        var property = filterMatch.Groups[1].Value;
        var operatorValue = filterMatch.Groups[2].Value;
        var value = filterMatch.Groups[3].Value.Trim('\\', '"');

        foreach (var item in array)
            if (EvaluateFilter(item, property, operatorValue, value))
                results.Add(item);

        return results;
    }

    private static JsonElement? ExecuteFunction(JsonElement element, string functionName)
    {
        return functionName.ToLowerInvariant() switch
        {
            "length" => JsonParserHelper.CreateNumberElement(GetLength(element)),
            "keys" => JsonParserHelper.CreateArrayElement(GetKeys(element)),
            "values" => JsonParserHelper.CreateArrayElement(GetValues(element)),
            _ => null
        };
    }

    private static JsonElement CreateNumberElement(int value)
    {
        var json = value.ToString();
        return JsonDocument.Parse(json).RootElement;
    }

    private static JsonElement CreateStringElement(string value)
    {
        var json = $"\"{value}\"";
        return JsonDocument.Parse(json).RootElement;
    }

    private static JsonElement CreateArrayElement(List<JsonElement> elements)
    {
        var jsonArray = "[" + string.Join(",", elements.Select(e => e.GetRawText())) + "]";
        return JsonDocument.Parse(jsonArray).RootElement;
    }
    
    private static int GetLength(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Array => element.GetArrayLength(),
            JsonValueKind.Object => element.EnumerateObject().Count(),
            JsonValueKind.String => element.GetString()?.Length ?? 0,
            _ => 0
        };
    }

    private static List<JsonElement> GetKeys(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return new List<JsonElement>();

        return element.EnumerateObject()
            .Select(prop => CreateStringElement(prop.Name))
            .ToList();
    }

    private static List<JsonElement> GetValues(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Select(prop => prop.Value).ToList(),
            JsonValueKind.Array => element.EnumerateArray().ToList(),
            _ => new List<JsonElement>()
        };
    }   
    
    private static bool EvaluateFilter(JsonElement element, string property, string operatorValue, string value)
    {
        try
        {
            if (element.ValueKind != JsonValueKind.Object)
                return false;

            if (!element.TryGetProperty(property, out var propertyElement))
                return false;

            var propertyValue = GetElementValue(propertyElement);

            return operatorValue switch
            {
                "==" or "=" => CompareValues(propertyValue, value) == 0,
                "!=" => CompareValues(propertyValue, value) != 0,
                ">" => CompareValues(propertyValue, value) > 0,
                ">=" => CompareValues(propertyValue, value) >= 0,
                "<" => CompareValues(propertyValue, value) < 0,
                "<=" => CompareValues(propertyValue, value) <= 0,
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }
    
    private static string GetElementValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            _ => element.ToString()
        };
    }    
    
    private static int CompareValues(string left, string right)
    {
        // Try numeric comparison first
        if (double.TryParse(left, out var leftNum) && double.TryParse(right, out var rightNum))
            return leftNum.CompareTo(rightNum);

        // Fall back to string comparison
        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }    
}