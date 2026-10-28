namespace Template.WindowsApp.Helpers;

using System.Diagnostics;
using System.IO.Pipes;

public sealed class SingleInstance : IDisposable
{
    private const int ConnectTimeout = 5000;

    private const char Separator = '\0';

    private readonly Mutex mutex;

    private readonly string pipeName;

    private readonly CancellationTokenSource cancellation = new();

    public bool IsFirst { get; }

    public SingleInstance(string name)
    {
        mutex = new Mutex(true, $"Local\\{name}", out var createdNew);
        IsFirst = createdNew;

        using var process = Process.GetCurrentProcess();
        pipeName = $"{name}.{process.SessionId}";
    }

    public void Dispose()
    {
        cancellation.Cancel();
        cancellation.Dispose();
        if (IsFirst)
        {
            mutex.ReleaseMutex();
        }
        mutex.Dispose();
    }

    public void Send(string[] args)
    {
        using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.CurrentUserOnly);
        try
        {
            client.Connect(ConnectTimeout);
        }
        catch (TimeoutException)
        {
            return;
        }

        if (NativeMethods.GetNamedPipeServerProcessId(client.SafePipeHandle, out var processId))
        {
            _ = NativeMethods.AllowSetForegroundWindow(processId);
        }

        client.Write(Encoding.UTF8.GetBytes(String.Join(Separator, args)));
    }

    public void Listen(Action<string[]> received)
    {
        _ = ListenAsync(received, cancellation.Token);
    }

    private async Task ListenAsync(Action<string[]> received, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            await using (server.ConfigureAwait(true))
            {
                using var buffer = new MemoryStream();
                try
                {
                    await server.WaitForConnectionAsync(token).ConfigureAwait(true);
                    await server.CopyToAsync(buffer, token).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (IOException)
                {
                    continue;
                }

                received(Encoding.UTF8.GetString(buffer.ToArray()).Split(Separator, StringSplitOptions.RemoveEmptyEntries));
            }
        }
    }
}
