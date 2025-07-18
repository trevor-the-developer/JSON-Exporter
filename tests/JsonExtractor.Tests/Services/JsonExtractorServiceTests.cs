using FluentAssertions;
using JsonExtractor.Services;
using JsonExtractor.Tests.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JsonExtractor.Tests.Services;

public class JsonExtractorServiceTests
{
    private readonly JsonExtractorService _service;

    public JsonExtractorServiceTests()
    {
        var loggerMock = new Mock<ILogger<JsonExtractorService>>();
        _service = new JsonExtractorService(loggerMock.Object);
    }

    [Fact]
    public async Task ParseJsonAsync_WithValidJson_ShouldReturnSuccess()
    {
        // Act
        var result = await _service.ParseJsonAsync(SampleData.SimpleJson);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().NotBeNull();
    }

    [Fact]
    public async Task ParseJsonAsync_WithInvalidJson_ShouldReturnError()
    {
        // Act
        var result = await _service.ParseJsonAsync(SampleData.InvalidJson);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNull();
        result.Message.Should().Contain("Invalid JSON");
    }

    [Fact]
    public async Task ParseJsonAsync_WithNullInput_ShouldReturnError()
    {
        // Act
        var result = await _service.ParseJsonAsync(null!);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot be null or empty");
    }

    [Fact]
    public async Task ParseJsonAsync_WithEmptyString_ShouldReturnError()
    {
        // Act
        var result = await _service.ParseJsonAsync(string.Empty);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot be null or empty");
    }

    [Theory]
    [InlineData(SampleData.SimpleJson)]
    [InlineData(SampleData.ComplexJson)]
    [InlineData(SampleData.ArrayJson)]
    public async Task ParseJsonAsync_WithVariousValidJsonFormats_ShouldReturnSuccess(string json)
    {
        // Act
        var result = await _service.ParseJsonAsync(json);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task FormatJsonAsync_WithValidJson_ShouldReturnFormattedJson()
    {
        // Arrange
        const string compactJson = "{\"name\":\"John\",\"age\":30}";

        // Act
        var result = await _service.FormatJsonAsync(compactJson);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ToString().Should().Contain("  "); // Should be indented
    }

    [Fact]
    public async Task ValidateJsonAsync_WithValidJson_ShouldReturnSuccess()
    {
        // Act
        var result = await _service.ValidateJsonAsync(SampleData.SimpleJson);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be(true);
    }

    [Fact]
    public async Task ValidateJsonAsync_WithInvalidJson_ShouldReturnError()
    {
        // Act
        var result = await _service.ValidateJsonAsync(SampleData.InvalidJson);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid JSON");
    }

    [Fact]
    public void ConvertToObject_WithValidJson_ShouldReturnDeserializedObject()
    {
        // Arrange
        const string json = "{\"Name\":\"John\",\"Age\":30}";

        // Act
        var result = _service.ConvertToObject<TestPerson>(json);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeOfType<TestPerson>();
        var person = result.Data as TestPerson;
        person!.Name.Should().Be("John");
        person.Age.Should().Be(30);
    }

    [Fact]
    public void SerializeObject_WithValidObject_ShouldReturnJsonString()
    {
        // Arrange
        var person = new TestPerson { Name = "Jane", Age = 25 };

        // Act
        var result = _service.SerializeObject(person);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ToString().Should().Contain("Jane");
        result.Data!.ToString().Should().Contain("25");
    }

    [Fact]
    public async Task ParseJsonFromFileAsync_WithNonExistentFile_ShouldReturnError()
    {
        // Act
        var result = await _service.ParseJsonFromFileAsync("nonexistent.json");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("File not found");
    }

    private class TestPerson
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}