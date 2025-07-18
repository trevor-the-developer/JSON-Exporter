using System.Text.Json;
using FluentAssertions;
using JsonExtractor.Commands;
using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using JsonExtractor.Tests.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JsonExtractor.Tests.Commands;

public class QueryCommandTests
{
    private readonly QueryCommand _command;
    private readonly Mock<IJsonExtractorService> _extractorServiceMock;
    private readonly Mock<IJsonQueryService> _queryServiceMock;

    public QueryCommandTests()
    {
        _extractorServiceMock = new Mock<IJsonExtractorService>();
        _queryServiceMock = new Mock<IJsonQueryService>();
        var loggerMock = new Mock<ILogger<QueryCommand>>();
        _command = new QueryCommand(_extractorServiceMock.Object, _queryServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public void Command_Properties_ShouldBeCorrect()
    {
        // Assert
        _command.Name.Should().Be("query");
        _command.Description.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientArgs_ShouldReturnError()
    {
        // Act
        var result = await _command.ExecuteAsync(new[] { "single-arg" });

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Usage:");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidJsonAndQuery_ShouldExecuteQuery()
    {
        // Arrange
        const string json = SampleData.SimpleJson;
        const string query = "$.name";
        var parseResult = CommandResult.CreateSuccess("Parsed", json);
        var queryResult = QueryResult.CreateSuccess(new List<JsonElement>(), query, TimeSpan.FromMilliseconds(10));

        _extractorServiceMock.Setup(x => x.ParseJsonAsync(json, null))
            .ReturnsAsync(parseResult);
        _queryServiceMock.Setup(x => x.ExecuteJsonPath(It.IsAny<JsonElement>(), query))
            .Returns(queryResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, query });

        // Assert
        result.Success.Should().BeTrue();
        _queryServiceMock.Verify(x => x.ExecuteJsonPath(It.IsAny<JsonElement>(), query), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithFileFlag_ShouldParseFromFile()
    {
        // Arrange
        const string filePath = "test.json";
        const string query = "$.name";
        var parseResult = CommandResult.CreateSuccess("Parsed", SampleData.SimpleJson);
        var queryResult = QueryResult.CreateSuccess(new List<JsonElement>(), query, TimeSpan.FromMilliseconds(10));

        _extractorServiceMock.Setup(x => x.ParseJsonFromFileAsync(filePath, null))
            .ReturnsAsync(parseResult);
        _queryServiceMock.Setup(x => x.ExecuteJsonPath(It.IsAny<JsonElement>(), query))
            .Returns(queryResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { "--file", filePath, query });

        // Assert
        result.Success.Should().BeTrue();
        _extractorServiceMock.Verify(x => x.ParseJsonFromFileAsync(filePath, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedParse_ShouldReturnParseError()
    {
        // Arrange
        const string json = "invalid json";
        const string query = "$.name";
        var parseResult = CommandResult.CreateError("Parse failed");

        _extractorServiceMock.Setup(x => x.ParseJsonAsync(json, null))
            .ReturnsAsync(parseResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, query });

        // Assert
        result.Should().Be(parseResult);
        _queryServiceMock.Verify(x => x.ExecuteJsonPath(It.IsAny<JsonElement>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedQuery_ShouldReturnQueryError()
    {
        // Arrange
        const string json = SampleData.SimpleJson;
        const string query = "invalid query";
        var parseResult = CommandResult.CreateSuccess("Parsed", json);
        var queryResult = QueryResult.CreateError("Invalid query", query);

        _extractorServiceMock.Setup(x => x.ParseJsonAsync(json, null))
            .ReturnsAsync(parseResult);
        _queryServiceMock.Setup(x => x.ExecuteJsonPath(It.IsAny<JsonElement>(), query))
            .Returns(queryResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, query });

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid query");
    }
}