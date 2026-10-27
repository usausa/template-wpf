namespace Template.WindowsApp.Mocks;

using System.IO;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using Smart.Data;

using Template.WindowsApp.Services;

public sealed class TestDatabase : IDisposable
{
    private readonly ServiceProvider provider;

    private readonly string path;

    public DataService DataService { get; }

    private TestDatabase(TimeProvider timeProvider)
    {
        path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={path};Pooling=False";
        provider = new ServiceCollection()
            .AddSingleton(timeProvider)
            .AddSingleton<IDbProvider>(new DelegateDbProvider(() => new SqliteConnection(connectionString)))
            .AddDataAccessors()
            .AddSingleton<DataService>()
            .BuildServiceProvider();
        DataService = provider.GetRequiredService<DataService>();
    }

    public static async Task<TestDatabase> CreateAsync(TimeProvider timeProvider)
    {
        var database = new TestDatabase(timeProvider);
        await database.DataService.RebuildAsync();
        return database;
    }

    public void Dispose()
    {
        provider.Dispose();
        foreach (var file in new[] { path, $"{path}-wal", $"{path}-shm" })
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
