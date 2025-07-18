using FluentAssertions;
using JsonExtractor.Commands;
using JsonExtractor.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JsonExtractor.Tests.Commands;

public class CommandFactoryTests
{
    private readonly Mock<IJsonExtractorService> _extractorServiceMock;
    private readonly CommandFactory _factory;
    private readonly Mock<ILogger<CommandFactory>> _loggerMock;
    private readonly Mock<IJsonQueryService> _queryServiceMock;
    private readonly IServiceProvider _serviceProvider;

    public CommandFactoryTests()
    {
        _extractorServiceMock = new Mock<IJsonExtractorService>();
        _queryServiceMock = new Mock<IJsonQueryService>();
        _loggerMock = new Mock<ILogger<CommandFactory>>();

        var services = new ServiceCollection();
        services.AddSingleton(_extractorServiceMock.Object);
        services.AddSingleton(_queryServiceMock.Object);
        services.AddLogging();
        _serviceProvider = services.BuildServiceProvider();

        _factory = new CommandFactory(
            _extractorServiceMock.Object,
            _queryServiceMock.Object,
            _serviceProvider,
            _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullExtractorService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandFactory(
            null!,
            _queryServiceMock.Object,
            _serviceProvider,
            _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullQueryService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandFactory(
            _extractorServiceMock.Object,
            null!,
            _serviceProvider,
            _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandFactory(
            _extractorServiceMock.Object,
            _queryServiceMock.Object,
            null!,
            _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandFactory(
            _extractorServiceMock.Object,
            _queryServiceMock.Object,
            _serviceProvider,
            null!));
    }

    [Theory]
    [InlineData("parse", typeof(ParseCommand))]
    [InlineData("query", typeof(QueryCommand))]
    [InlineData("format", typeof(FormatCommand))]
    [InlineData("export", typeof(ExportCommand))]
    [InlineData("help", typeof(HelpCommand))]
    [InlineData("PARSE", typeof(ParseCommand))] // Case insensitive
    [InlineData("Query", typeof(QueryCommand))]
    public void CreateCommand_WithValidCommandName_ShouldReturnCorrectCommandType(string commandName, Type expectedType)
    {
        // Act
        var command = _factory.CreateCommand(commandName);

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType(expectedType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateCommand_WithInvalidCommandName_ShouldThrowArgumentException(string commandName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _factory.CreateCommand(commandName));
    }

    [Fact]
    public void CreateCommand_WithUnsupportedCommand_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _factory.CreateCommand("unsupported"));
        exception.Message.Should().Contain("Unsupported command: unsupported");
    }

    [Fact]
    public void TryCreateCommand_WithValidCommandName_ShouldReturnCommand()
    {
        // Act
        var command = _factory.TryCreateCommand("parse");

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<ParseCommand>();
    }

    [Fact]
    public void TryCreateCommand_WithInvalidCommandName_ShouldReturnNull()
    {
        // Act
        var command = _factory.TryCreateCommand("unsupported");

        // Assert
        command.Should().BeNull();
    }

    [Fact]
    public void GetSupportedCommands_ShouldReturnAllSupportedCommands()
    {
        // Act
        var commands = _factory.GetSupportedCommands();

        // Assert
        commands.Should().NotBeNull();
        commands.Should().Contain("parse");
        commands.Should().Contain("query");
        commands.Should().Contain("format");
        commands.Should().Contain("export");
        commands.Should().Contain("help");
        commands.Length.Should().Be(5);
    }

    [Fact]
    public void GetSupportedCommandsString_ShouldReturnCommaSeparatedString()
    {
        // Act
        var result = _factory.GetSupportedCommandsString();

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("parse");
        result.Should().Contain("query");
        result.Should().Contain("format");
        result.Should().Contain("export");
        result.Should().Contain("help");
        result.Should().Contain(",");
    }

    [Fact]
    public void CreateCommand_ParseCommand_ShouldHaveCorrectProperties()
    {
        // Act
        var command = _factory.CreateCommand("parse");

        // Assert
        command.Name.Should().Be("parse");
        command.Description.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateCommand_QueryCommand_ShouldHaveCorrectProperties()
    {
        // Act
        var command = _factory.CreateCommand("query");

        // Assert
        command.Name.Should().Be("query");
        command.Description.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateCommand_FormatCommand_ShouldHaveCorrectProperties()
    {
        // Act
        var command = _factory.CreateCommand("format");

        // Assert
        command.Name.Should().Be("format");
        command.Description.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateCommand_ExportCommand_ShouldHaveCorrectProperties()
    {
        // Act
        var command = _factory.CreateCommand("export");

        // Assert
        command.Name.Should().Be("export");
        command.Description.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateCommand_HelpCommand_ShouldHaveCorrectProperties()
    {
        // Act
        var command = _factory.CreateCommand("help");

        // Assert
        command.Name.Should().Be("help");
        command.Description.Should().NotBeEmpty();
    }
}