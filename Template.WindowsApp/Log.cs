namespace Template.WindowsApp;

#pragma warning disable SYSLIB1006
public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Execute start.")]
    public static partial void InfoExecuteStart(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Execute end.")]
    public static partial void InfoExecuteEnd(this ILogger logger);
}
#pragma warning restore SYSLIB1006
