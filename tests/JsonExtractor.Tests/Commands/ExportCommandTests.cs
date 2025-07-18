using System.Text.Json;
using FluentAssertions;
using JsonExtractor.Commands;
using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JsonExtractor.Tests.Commands;

public class ExportCommandTests
{
    private readonly ExportCommand _command;
    private readonly Mock<IJsonExtractorService> _extractorServiceMock;
    private readonly Mock<IJsonQueryService> _queryServiceMock;

    public ExportCommandTests()
    {
        _extractorServiceMock = new Mock<IJsonExtractorService>();
        _queryServiceMock = new Mock<IJsonQueryService>();
        var loggerMock = new Mock<ILogger<ExportCommand>>();
        _command = new ExportCommand(_extractorServiceMock.Object, _queryServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public void Command_Properties_ShouldBeCorrect()
    {
        // Assert
        _command.Name.Should().Be("export");
        _command.Description.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithNoArgs_ShouldReturnError()
    {
        // Act
        var result = await _command.ExecuteAsync(Array.Empty<string>());

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Usage:");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidJsonAndCsvFormat_ShouldExportToCsv()
    {
        // Arrange
        const string json = """{"users":[{"name":"John","age":30},{"name":"Jane","age":25}]}""";
        var jsonDocument = JsonDocument.Parse(json);
        var parseResult = CommandResult.CreateSuccess("Parsed", jsonDocument);

        _extractorServiceMock.Setup(x => x.ParseJsonDocumentAsync(json, null))
            .ReturnsAsync(parseResult);

        var queryResult = QueryResult.CreateSuccess(
            jsonDocument.RootElement.GetProperty("users").EnumerateArray().ToList(),
            "$.users[*]",
            TimeSpan.FromMilliseconds(1));

        _queryServiceMock.Setup(x => x.ExecuteJsonPath(It.IsAny<JsonElement>(), "$.users[*]"))
            .Returns(queryResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, "csv", "$.users[*]" });

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data?.ToString().Should().Contain("name,age");
        result.Data?.ToString().Should().Contain("John,30");
        result.Data?.ToString().Should().Contain("Jane,25");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidJsonAndXmlFormat_ShouldExportToXml()
    {
        // Arrange
        var json = """{"name":"John","age":30}""";
        var jsonDocument = JsonDocument.Parse(json);
        var parseResult = CommandResult.CreateSuccess("Parsed", jsonDocument);

        _extractorServiceMock.Setup(x => x.ParseJsonDocumentAsync(json, null))
            .ReturnsAsync(parseResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, "xml" });

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var xmlOutput = result.Data?.ToString() ?? "";
        xmlOutput.Should().Contain("<Results>");
        xmlOutput.Should().Contain("<name>John</name>");
        xmlOutput.Should().Contain("<age>30</age>");
        xmlOutput.Should().Contain("</Results>");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidJsonAndJsonFormat_ShouldExportToJson()
    {
        // Arrange
        var json = """{"name":"John","age":30}""";
        var jsonDocument = JsonDocument.Parse(json);
        var parseResult = CommandResult.CreateSuccess("Parsed", jsonDocument);

        _extractorServiceMock.Setup(x => x.ParseJsonDocumentAsync(json, null))
            .ReturnsAsync(parseResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, "json" });

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var jsonOutput = result.Data?.ToString() ?? "";
        jsonOutput.Should().Contain("\"name\":\"John\"");
        jsonOutput.Should().Contain("\"age\":30");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidFormat_ShouldReturnError()
    {
        // Arrange
        const string json = """{"name":"John"}""";
        var jsonDocument = JsonDocument.Parse(json);
        var parseResult = CommandResult.CreateSuccess("Parsed", jsonDocument);

        _extractorServiceMock.Setup(x => x.ParseJsonDocumentAsync(json, null))
            .ReturnsAsync(parseResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, "invalid" });

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Export failed");
    }

    [Fact]
    public async Task ExecuteAsync_WithFailedParsing_ShouldReturnError()
    {
        // Arrange
        const string json = """{"invalid":json}""";
        var parseResult = CommandResult.CreateError("Parse failed");

        _extractorServiceMock.Setup(x => x.ParseJsonDocumentAsync(json, null))
            .ReturnsAsync(parseResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, "csv" });

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to parse JSON");
    }

    [Fact]
    public async Task ExecuteAsync_WithDelimiterOption_ShouldUseSemicolonDelimiter()
    {
        // Arrange
        const string json = """{"users":[{"name":"John","age":30}]}""";
        var jsonDocument = JsonDocument.Parse(json);
        var parseResult = CommandResult.CreateSuccess("Parsed", jsonDocument);

        _extractorServiceMock.Setup(x => x.ParseJsonDocumentAsync(json, null))
            .ReturnsAsync(parseResult);

        var queryResult = QueryResult.CreateSuccess(
            jsonDocument.RootElement.GetProperty("users").EnumerateArray().ToList(),
            "$.users[*]",
            TimeSpan.FromMilliseconds(1));

        _queryServiceMock.Setup(x => x.ExecuteJsonPath(It.IsAny<JsonElement>(), "$.users[*]"))
            .Returns(queryResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json, "csv", "$.users[*]", "--delimiter=;" });

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data?.ToString().Should().Contain("name;age");
        result.Data?.ToString().Should().Contain("John;30");
    }
}