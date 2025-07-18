using System.Text.Json;
using JsonExtractor.Services;
using Xunit;

namespace JsonExtractor.Tests.Services;

public class AdvancedJsonPathParserTests
{
    private readonly AdvancedJsonPathParser _parser;
    private readonly JsonElement _sampleJson;

    public AdvancedJsonPathParserTests()
    {
        _parser = new AdvancedJsonPathParser();
        const string json = """
                            {
                                "store": {
                                    "book": [
                                        { "category": "reference", "author": "Nigel Rees", "price": 8.95, "title": "Sayings of the Century" },
                                        { "category": "fiction", "author": "Evelyn Waugh", "price": 12.99, "title": "Sword of Honour" },
                                        { "category": "fiction", "author": "Herman Melville", "isbn": "0-553-21311-3", "price": 8.99, "title": "Moby Dick" },
                                        { "category": "fiction", "author": "J. R. R. Tolkien", "isbn": "0-395-19395-8", "price": 22.99, "title": "The Lord of the Rings" }
                                    ],
                                    "bicycle": { "color": "red", "price": 19.95 }
                                },
                                "expensive": 10
                            }
                            """;
        using var doc = JsonDocument.Parse(json);
        _sampleJson = doc.RootElement.Clone();
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_EmptyPath_ShouldReturnRoot()
    {
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, "$");
        Assert.True(result.Success);
        Assert.Single(result.Results);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_FilterLessThan_ShouldReturnBooksUnder10()
    {
        const string path = "$.store.book[?(@.price < 10)]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_FilterGreaterThan_ShouldReturnExpensiveBooks()
    {
        const string path = "$.store.book[?(@.price > 15)]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Single(result.Results);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_FilterEquals_ShouldReturnFictionBooks()
    {
        const string path = "$.store.book[?(@.category == \"fiction\")]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(3, result.Results.Count);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_RecursiveDescent_ShouldFindAllPrices()
    {
        const string path = "$..price";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(5, result.Results.Count); // 4 books + 1 bicycle
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_RecursiveDescentWithProperty_ShouldFindAllAuthors()
    {
        const string path = "$..author";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(4, result.Results.Count);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_NonexistentProperty_ShouldReturnEmpty()
    {
        const string path = "$..nonexistent";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Empty(result.Results);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_ArraySlice_ShouldReturnCorrectRange()
    {
        const string path = "$.store.book[1:3]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_ArraySliceFromStart_ShouldReturnCorrectRange()
    {
        const string path = "$.store.book[:2]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_ArraySliceToEnd_ShouldReturnCorrectRange()
    {
        const string path = "$.store.book[2:]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_MultipleIndices_ShouldReturnSpecificElements()
    {
        const string path = "$.store.book[0,2]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_ArrayWildcard_ShouldReturnAllElements()
    {
        const string path = "$.store.book[*]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(4, result.Results.Count);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_PropertyWildcard_ShouldReturnAllProperties()
    {
        const string path = "$.store.*";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Equal(2, result.Results.Count); // book array + bicycle object
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_Functions_Length_ShouldReturnCorrectLength()
    {
        const string path = "$.store.book.length()";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Single(result.Results);
        Assert.Equal(4, result.Results[0].GetInt32());
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_ArrayIndex_ShouldReturnSpecificElement()
    {
        const string path = "$.store.book[0]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Single(result.Results);
        Assert.Equal("reference", result.Results[0].GetProperty("category").GetString());
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_InvalidJsonPath_ShouldReturnEmpty()
    {
        const string path = "$.store.book[invalid syntax";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Empty(result.Results);
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_ComplexNestedPath_ShouldReturnCorrectElements()
    {
        var path = "$.store.book[0].title";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Single(result.Results);
        Assert.Equal("Sayings of the Century", result.Results[0].GetString());
    }

    [Fact]
    public void ExecuteAdvancedJsonPath_FilterWithNotEquals_ShouldReturnCorrectElements()
    {
        const string path = "$.store.book[?(@.category != \"fiction\")]";
        var result = AdvancedJsonPathParser.ExecuteAdvancedJsonPath(_sampleJson, path);
        Assert.True(result.Success);
        Assert.Single(result.Results);
        Assert.Equal("reference", result.Results[0].GetProperty("category").GetString());
    }
}