using JsonExtractor.Models;

namespace JsonExtractor.Interfaces;

public interface ICommandProcessor
{
    Task<CommandResult> ProcessCommandAsync(string[] args);
}