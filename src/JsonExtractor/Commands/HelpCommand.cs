using System.Text;
using JsonExtractor.Interfaces;
using JsonExtractor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonExtractor.Commands;

public class HelpCommand : ICommand
{
    private readonly ILogger<HelpCommand> _logger;
    private readonly IServiceProvider _serviceProvider;

    public HelpCommand(IServiceProvider serviceProvider, ILogger<HelpCommand> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "help";
    public string Description => "Show help information";

    public Task<CommandResult> ExecuteAsync(string[] args)
    {
        var helpText = BuildHelpText();
        return Task.FromResult(CommandResult.CreateSuccess("Help information", helpText));
    }

    private string BuildHelpText()
    {
        var help = new StringBuilder();
        help.AppendLine("JSON Extractor Console App");
        help.AppendLine("=========================");
        help.AppendLine();
        help.AppendLine("A powerful tool for parsing, querying, and manipulating JSON data.");
        help.AppendLine();
        help.AppendLine("Available Commands:");
        help.AppendLine();

        var commands = _serviceProvider.GetServices<ICommand>().Where(c => c.Name != "help");
        foreach (var command in commands) help.AppendLine($"  {command.Name,-12} {command.Description}");

        help.AppendLine();
        help.AppendLine("Examples:");
        help.AppendLine("  parse '{\"name\":\"John\",\"age\":30}'");
        help.AppendLine("  parse --file data.json");
        help.AppendLine("  query '{\"users\":[{\"name\":\"John\"},{\"name\":\"Jane\"}]}' '$.users[*].name'");
        help.AppendLine("  format '{\"name\":\"John\",\"age\":30}'");
        help.AppendLine();
        help.AppendLine("JSONPath Query Examples:");
        help.AppendLine("  $                    - Root element");
        help.AppendLine("  $.store.book         - All books in store");
        help.AppendLine("  $.store.book[0]      - First book");
        help.AppendLine("  $.store.book[*].title - All book titles");
        help.AppendLine("  $..price             - All prices (recursive)");
        help.AppendLine("  $.store.book[?@.price < 10] - Books under $10");

        return help.ToString();
    }
}