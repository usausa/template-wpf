namespace Template.WindowsApp.Services;

using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MemoryLogService : ILogEventSink, IDisposable
{
    public const int Capacity = 1000;

    private readonly Lock sync = new();

    private readonly Queue<LogEntry> entries = new(Capacity);

    private readonly Subject<LogEntry> added = new();

    private readonly MessageTemplateTextFormatter formatter = new("{Message:lj}", CultureInfo.InvariantCulture);

    private long sequence;

    public IObservable<LogEntry> Added => added;

    public void Dispose() => added.Dispose();

    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        formatter.Format(logEvent, writer);
        var message = writer.ToString();
        var source = FindSource(logEvent);
        var exception = logEvent.Exception?.ToString();

        lock (sync)
        {
            sequence++;
            var entry = new LogEntry(sequence, logEvent.Timestamp, logEvent.Level, source, message, exception);
            entries.Enqueue(entry);
            if (entries.Count > Capacity)
            {
                entries.Dequeue();
            }

            added.OnNext(entry);
        }
    }

    public IReadOnlyList<LogEntry> Snapshot()
    {
        lock (sync)
        {
            return entries.ToArray();
        }
    }

    public void Clear()
    {
        lock (sync)
        {
            entries.Clear();
        }
    }

    private static string FindSource(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(Constants.SourceContextPropertyName, out var value) && (value is ScalarValue { Value: string context }))
        {
            return context[(context.LastIndexOf('.') + 1)..];
        }

        return string.Empty;
    }
}
