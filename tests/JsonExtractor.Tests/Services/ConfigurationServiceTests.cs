using FluentAssertions;
using JsonExtractor.Models.Configuration;
using JsonExtractor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JsonExtractor.Tests.Services;

public class ConfigurationServiceTests
{
    private readonly ConfigurationService _service;

    public ConfigurationServiceTests()
    {
        var loggerMock = new Mock<ILogger<ConfigurationService>>();
        _service = new ConfigurationService(loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultConfiguration()
    {
        // Arrange & Act
        var configuration = _service.GetConfiguration();

        // Assert
        configuration.Should().NotBeNull();
        configuration.Logging.Should().NotBeNull();
        configuration.JsonProcessing.Should().NotBeNull();
        configuration.Query.Should().NotBeNull();
        configuration.Export.Should().NotBeNull();
        configuration.Performance.Should().NotBeNull();
    }

    [Fact]
    public void GetConfiguration_ShouldReturnCurrentConfiguration()
    {
        // Act
        var result = _service.GetConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<JsonExtractorConfiguration>();
    }

    [Fact]
    public void GetSection_WithValidSectionName_ShouldReturnSection()
    {
        // Act
        var loggingSection = _service.GetSection<LoggingConfiguration>("Logging");

        // Assert
        loggingSection.Should().NotBeNull();
        loggingSection.Should().BeOfType<LoggingConfiguration>();
        loggingSection.LogLevel.Should().Be("Information");
    }

    [Fact]
    public void GetSection_WithInvalidSectionName_ShouldReturnDefaultInstance()
    {
        // Act
        var result = _service.GetSection<LoggingConfiguration>("NonExistentSection");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<LoggingConfiguration>();
    }

    [Fact]
    public void UpdateConfiguration_WithValidConfiguration_ShouldUpdateSuccessfully()
    {
        // Arrange
        var newConfiguration = new JsonExtractorConfiguration
        {
            Logging = new LoggingConfiguration
            {
                LogLevel = "Debug"
            }
        };

        // Act
        _service.UpdateConfiguration(newConfiguration);
        var result = _service.GetConfiguration();

        // Assert
        result.Should().Be(newConfiguration);
        result.Logging.LogLevel.Should().Be("Debug");
    }

    [Fact]
    public void UpdateConfiguration_WithNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.UpdateConfiguration(null!));
    }

    [Theory]
    [InlineData("JsonProcessing")]
    [InlineData("Query")]
    [InlineData("Export")]
    [InlineData("Performance")]
    public void GetSection_WithValidSectionNames_ShouldReturnCorrectTypes(string sectionName)
    {
        // Act & Assert
        object result = sectionName switch
        {
            "JsonProcessing" => _service.GetSection<JsonProcessingConfiguration>(sectionName),
            "Query" => _service.GetSection<QueryConfiguration>(sectionName),
            "Export" => _service.GetSection<ExportConfiguration>(sectionName),
            "Performance" => _service.GetSection<PerformanceConfiguration>(sectionName),
            _ => throw new ArgumentException("Invalid section name")
        };

        result.Should().NotBeNull();
    }
}