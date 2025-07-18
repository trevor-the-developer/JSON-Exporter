namespace JsonExtractor.Models;

public record CommandResult
{
    public bool Success { get; private init; }
    public string? Message { get; private init; }
    public object? Data { get; private init; }
    public Exception? Exception { get; private init; }

    public static CommandResult CreateSuccess(string? message = null, object? data = null)
    {
        return new CommandResult { Success = true, Message = message, Data = data };
    }

    public static CommandResult CreateError(string message, Exception? exception = null)
    {
        return new CommandResult { Success = false, Message = message, Exception = exception };
    }
}