using System.Text.Json;
using FluentAssertions;
using JsonExtractor.Services;
using JsonExtractor.Tests.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JsonExtractor.Tests.Services;

public class JsonQueryServiceTests
{
    private readonly JsonQueryService _service;

    public JsonQueryServiceTests()
    {
        var loggerMock = new Mock<ILogger<JsonQueryService>>();
        _service = new JsonQueryService(loggerMock.Object);
    }

    [Fact]
    public void ExecuteJsonPath_WithValidQuery_ShouldReturnResults()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.ComplexJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.ExecuteJsonPath(rootElement, "$.store.book[*].title");

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(3);
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Theory]
    [InlineData("$.store.book[0].title", "Sayings of the Century")]
    [InlineData("$.store.bicycle.color", "red")]
    [InlineData("$.expensive", "10")]
    public void ExecuteJsonPath_WithSpecificQueries_ShouldReturnExpectedValues(string query, string expectedValue)
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.ComplexJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.ExecuteJsonPath(rootElement, query);

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(1);

        var actualValue = result.Results[0].ValueKind switch
        {
            JsonValueKind.String => result.Results[0].GetString(),
            JsonValueKind.Number => result.Results[0].GetDecimal().ToString(),
            _ => result.Results[0].ToString()
        };

        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void ExecuteJsonPath_WithInvalidQuery_ShouldReturnError()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.ExecuteJsonPath(rootElement, "$.invalid[syntax");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
    }

    [Fact]
    public void ExecuteJsonPath_WithEmptyQuery_ShouldReturnError()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.ExecuteJsonPath(rootElement, string.Empty);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be null or empty");
    }

    [Fact]
    public void ExecuteMultipleQueries_WithValidQueries_ShouldCombineResults()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.ComplexJson);
        var rootElement = document.RootElement;
        var queries = new[] { "$.store.book[*].author", "$.store.bicycle.color" };

        // Act
        var result = _service.ExecuteMultipleQueries(rootElement, queries);

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(4); // 3 authors + 1 color
    }

    [Fact]
    public void FindByKey_WithExistingKey_ShouldReturnMatchingValues()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.FindByKey(rootElement, "name");

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(1);
        result.Results[0].GetString().Should().Be("John Doe");
    }

    [Fact]
    public void FindByKey_CaseSensitive_ShouldRespectCase()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var resultCaseSensitive = _service.FindByKey(rootElement, "NAME", true);
        var resultCaseInsensitive = _service.FindByKey(rootElement, "NAME", false);

        // Assert
        resultCaseSensitive.Success.Should().BeTrue();
        resultCaseSensitive.Results.Should().BeEmpty();

        resultCaseInsensitive.Success.Should().BeTrue();
        resultCaseInsensitive.Results.Should().HaveCount(1);
    }

    [Fact]
    public void FindByValue_WithExistingValue_ShouldReturnMatchingElements()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.FindByValue(rootElement, "John Doe");

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(1);
        result.Results[0].GetString().Should().Be("John Doe");
    }

    [Theory]
    [InlineData(30, 1)]
    [InlineData(true, 1)]
    [InlineData("New York", 1)]
    public void FindByValue_WithDifferentValueTypes_ShouldReturnCorrectResults(object value, int expectedCount)
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.FindByValue(rootElement, value);

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void GetArrayElements_WithValidArrayPath_ShouldReturnArrayItems()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.GetArrayElements(rootElement, "$.hobbies");

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(3);
        result.Results.Select(r => r.GetString()).Should().Contain(new[] { "reading", "swimming", "coding" });
    }

    [Fact]
    public void GetArrayElements_WithSkipAndTake_ShouldReturnCorrectSubset()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.GetArrayElements(rootElement, "$.hobbies", 1, 1);

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(1);
        result.Results[0].GetString().Should().Be("swimming");
    }

    [Fact]
    public void FilterArray_WithPredicate_ShouldReturnFilteredResults()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.ComplexJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.FilterArray(rootElement, "$.store.book",
            element => element.TryGetProperty("price", out var price) && price.GetDecimal() < 10);

        // Assert
        result.Success.Should().BeTrue();
        result.Results.Should().HaveCount(2); // Two books under $10
    }

    [Fact]
    public void FilterArray_WithNonArrayPath_ShouldReturnError()
    {
        // Arrange
        using var document = JsonDocument.Parse(SampleData.SimpleJson);
        var rootElement = document.RootElement;

        // Act
        var result = _service.FilterArray(rootElement, "$.name", element => true);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("is not an array");
    }
}