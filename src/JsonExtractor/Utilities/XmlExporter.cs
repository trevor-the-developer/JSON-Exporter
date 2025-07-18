using System.Text;
using System.Text.Json;
using System.Xml;

namespace JsonExtractor.Utilities;

public static class XmlExporter
{
    public static string ExportToXml(IEnumerable<JsonElement> elements, string rootElementName = "Results",
        string itemElementName = "Item")
    {
        var sb = new StringBuilder();
        using var writer = XmlWriter.Create(sb, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = true
        });

        writer.WriteStartDocument();
        writer.WriteStartElement(rootElementName);

        var elementCount = 0;
        foreach (var element in elements)
        {
            WriteJsonElement(writer, element, itemElementName);
            elementCount++;
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();

        // Force the writer to flush
        writer.Flush();

        return sb.ToString();
    }

    private static void WriteJsonElement(XmlWriter writer, JsonElement element, string elementName)
    {
        writer.WriteStartElement(elementName);

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    WriteJsonElement(writer, property.Value, property.Name);
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray()) WriteJsonElement(writer, item, $"Item{index++}");
                break;

            case JsonValueKind.String:
                writer.WriteString(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteString(element.ToString());
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                writer.WriteString(element.GetBoolean().ToString().ToLower());
                break;

            case JsonValueKind.Null:
                writer.WriteAttributeString("nil", "true");
                break;
            case JsonValueKind.Undefined:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        writer.WriteEndElement();
    }
}