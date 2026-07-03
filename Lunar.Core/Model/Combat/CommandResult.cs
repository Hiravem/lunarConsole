namespace Lunar.Core.Model.Combat;

public sealed class CommandResult
{
    public bool Success { get; }
    public IReadOnlyList<string> LogLines { get; }
    public string? Error { get; }

    private CommandResult(bool success, IReadOnlyList<string> logLines, string? error)
    {
        Success = success;
        LogLines = logLines;
        Error = error;
    }

    public static CommandResult Ok(params string[] lines) =>
        new(true, lines, null);

    public static CommandResult Fail(string error) =>
        new(false, Array.Empty<string>(), error);
}
