using FluentAssertions;
using JsonExtractor.Commands;
using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JsonExtractor.Tests.Commands;

public class ParseCommandTests
{
    private readonly ParseCommand _command;
    private readonly Mock<IJsonExtractorService> _extractorServiceMock;

    public ParseCommandTests()
    {
        _extractorServiceMock = new Mock<IJsonExtractorService>();
        var loggerMock = new Mock<ILogger<ParseCommand>>();
        _command = new ParseCommand(_extractorServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public void Command_Properties_ShouldBeCorrect()
    {
        // Assert
        _command.Name.Should().Be("parse");
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
    public async Task ExecuteAsync_WithJsonString_ShouldCallParseJson()
    {
        // Arrange
        const string json = "{\"test\": true}";
        var expectedResult = CommandResult.CreateSuccess("Parsed", "formatted json");
        _extractorServiceMock.Setup(x => x.ParseJsonAsync(json, It.IsAny<ExtractorOptions>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { json });

        // Assert
        result.Should().Be(expectedResult);
        _extractorServiceMock.Verify(x => x.ParseJsonAsync(json, It.IsAny<ExtractorOptions>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithFileFlag_ShouldCallParseJsonFromFile()
    {
        // Arrange
        const string filePath = "test.json";
        var expectedResult = CommandResult.CreateSuccess("Parsed from file", "formatted json");
        _extractorServiceMock.Setup(x => x.ParseJsonFromFileAsync(filePath, It.IsAny<ExtractorOptions>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _command.ExecuteAsync(new[] { "--file", filePath });

        // Assert
        result.Should().Be(expectedResult);
        _extractorServiceMock.Verify(x => x.ParseJsonFromFileAsync(filePath, It.IsAny<ExtractorOptions>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithFileFlagButNoPath_ShouldReturnError()
    {
        // Act
        var result = await _command.ExecuteAsync(new[] { "--file" });

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("File path required");
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleJsonParts_ShouldJoinThem()
    {
        // Arrange
        var jsonParts = new[] { "{\"name\":", "\"John\"}" };
        const string expectedJson = "{\"name\": \"John\"}";
        var expectedResult = CommandResult.CreateSuccess("Parsed", "formatted json");
        _extractorServiceMock.Setup(x => x.ParseJsonAsync(expectedJson, It.IsAny<ExtractorOptions>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _command.ExecuteAsync(jsonParts);

        // Assert
        result.Should().Be(expectedResult);
        _extractorServiceMock.Verify(x => x.ParseJsonAsync(expectedJson, It.IsAny<ExtractorOptions>()), Times.Once);
    }
}