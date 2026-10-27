namespace Template.WindowsApp.Components;

using Serilog.Events;

public sealed record LogEntry(long Sequence, DateTimeOffset Timestamp, LogEventLevel Level, string Source, string Message, string? Exception);
