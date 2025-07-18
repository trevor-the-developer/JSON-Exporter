using JsonExtractor.Models;

namespace JsonExtractor.Interfaces;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task<CommandResult> ExecuteAsync(string[] args);
}