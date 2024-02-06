namespace Template.WindowsApp;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Execute start.")]
    public static partial void InfoExecuteStart(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Execute end.")]
    public static partial void InfoExecuteEnd(this ILogger logger);
}
