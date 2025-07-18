namespace JsonExtractor.Interfaces;

public interface ICommandFactory
{
    ICommand CreateCommand(string commandName);
    ICommand? TryCreateCommand(string commandName);
    string[] GetSupportedCommands();
    string GetSupportedCommandsString();
}