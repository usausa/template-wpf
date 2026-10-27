namespace Template.WindowsApp.Views.Platform;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models.Entity;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class DatabaseViewModel : AppViewModelBase, IShellCommandProvider
{
    private const long SampleId = 1;

    private const int BulkCount = 10000;

    private static readonly string[] SizeUnits = ["bytes", "KB", "MB", "GB"];

    private readonly TimeProvider timeProvider;

    private readonly IDialog dialog;

    private readonly DataService dataService;

    [ObservableProperty]
    public partial string DatabasePath { get; set; }

    [ObservableProperty]
    public partial string DatabaseSize { get; set; }

    [ObservableProperty]
    public partial long ProductCount { get; set; }

    [ObservableProperty]
    public partial long CustomerCount { get; set; }

    [ObservableProperty]
    public partial long OrderLineCount { get; set; }

    [ObservableProperty]
    public partial long DataCount { get; set; }

    [ObservableProperty]
    public partial long BulkDataCount { get; set; }

    [ObservableProperty]
    public partial long TotalCount { get; set; }

    [ObservableProperty]
    public partial string? FileResult { get; set; }

    [ObservableProperty]
    public partial string? RowResult { get; set; }

    [ObservableProperty]
    public partial string? BulkResult { get; set; }

    public IObserveCommand OpenFolderCommand { get; }

    public IObserveCommand RebuildCommand { get; }

    public IObserveCommand RefreshCommand { get; }

    public IObserveCommand InsertCommand { get; }

    public IObserveCommand UpdateCommand { get; }

    public IObserveCommand DeleteCommand { get; }

    public IObserveCommand QueryCommand { get; }

    public IObserveCommand BulkInsertCommand { get; }

    public IObserveCommand ReadAllCommand { get; }

    public IObserveCommand DeleteAllCommand { get; }

    public DatabaseViewModel(TimeProvider timeProvider, IDialog dialog, IExplorerLauncher explorerLauncher, DataService dataService)
    {
        this.timeProvider = timeProvider;
        this.dialog = dialog;
        this.dataService = dataService;

        DatabasePath = dataService.DatabasePath;
        DatabaseSize = string.Empty;

        OpenFolderCommand = MakeDelegateCommand(() => explorerLauncher.Open(DatabasePath));
        RebuildCommand = MakeAsyncCommand(RebuildAsync);
        RefreshCommand = MakeAsyncCommand(RefreshAsync);
        InsertCommand = MakeAsyncCommand(InsertAsync);
        UpdateCommand = MakeAsyncCommand(UpdateAsync);
        DeleteCommand = MakeAsyncCommand(DeleteAsync);
        QueryCommand = MakeAsyncCommand(QueryAsync);
        BulkInsertCommand = MakeAsyncCommand(BulkInsertAsync, () => BulkDataCount == 0);
        ReadAllCommand = MakeAsyncCommand(ReadAllAsync, () => BulkDataCount > 0);
        DeleteAllCommand = MakeAsyncCommand(DeleteAllAsync, () => BulkDataCount > 0);
    }

    public override Task OnNavigatingToAsync(INavigationContext context) => RefreshAsync();

    private static string FormatSize(long length)
    {
        var size = (double)length;
        var unit = 0;
        while ((size >= 1024) && (unit < SizeUnits.Length - 1))
        {
            size /= 1024;
            unit++;
        }
        return String.Create(CultureInfo.CurrentCulture, $"{size:0.#} {SizeUnits[unit]}");
    }

    private static string Rows(long count) =>
        String.Create(CultureInfo.CurrentCulture, $"{count:N0} {(count == 1 ? "row" : "rows")}");

    private async Task RefreshAsync()
    {
        var info = await dataService.GetDatabaseInfoAsync().ConfigureAwait(true);
        DatabasePath = info.Path;
        DatabaseSize = FormatSize(info.Size);
        ProductCount = info.ProductCount;
        CustomerCount = info.CustomerCount;
        OrderLineCount = info.OrderLineCount;
        DataCount = info.DataCount;
        BulkDataCount = info.BulkDataCount;
        TotalCount = info.ProductCount + info.CustomerCount + info.OrderLineCount + info.DataCount + info.BulkDataCount;
    }

    private async Task RebuildAsync()
    {
        if (!await dialog.ConfirmAsync("Rebuild the database?", "The tables are created again with the sample data. Changes in Data grid, Editable grid and List & detail are lost.", "Rebuild", "Cancel").ConfigureAwait(true))
        {
            return;
        }

        await dataService.RebuildAsync().ConfigureAwait(true);
        await RefreshAsync().ConfigureAwait(true);
        FileResult = "Rebuilt with the sample data";
    }

    private async Task InsertAsync()
    {
        var inserted = await dataService.InsertDataAsync(new DataEntity { Id = SampleId, Name = "Data-1", CreateAt = timeProvider.GetUtcNow().UtcDateTime }).ConfigureAwait(true);
        RowResult = inserted ? "Inserted" : "Key duplicate";
        await RefreshAsync().ConfigureAwait(true);
    }

    private async Task UpdateAsync()
    {
        var count = await dataService.UpdateDataAsync(SampleId, "Updated").ConfigureAwait(true);
        RowResult = "Updated " + Rows(count);
    }

    private async Task DeleteAsync()
    {
        var count = await dataService.DeleteDataAsync(SampleId).ConfigureAwait(true);
        RowResult = "Deleted " + Rows(count);
        await RefreshAsync().ConfigureAwait(true);
    }

    private async Task QueryAsync()
    {
        var entity = await dataService.QueryDataAsync(SampleId).ConfigureAwait(true);
        RowResult = entity is null
            ? "Not found"
            : String.Create(CultureInfo.CurrentCulture, $"{entity.Name}, created {entity.CreateAt.ToLocalTime():G}");
    }

    private async Task BulkInsertAsync()
    {
        var source = Enumerable.Range(1, BulkCount)
            .Select(static x => new BulkDataEntity
            {
                Key1 = (x / 1000).ToString("D2", CultureInfo.InvariantCulture),
                Key2 = (x % 1000).ToString("D3", CultureInfo.InvariantCulture),
                Key3 = "0",
                Value1 = 1,
                Value2 = 2,
                Value3 = 3,
                Value4 = 4,
                Value5 = 5
            })
            .ToList();

        var start = timeProvider.GetTimestamp();
        await Task.Run(() => dataService.InsertBulkData(source)).ConfigureAwait(true);
        var elapsed = timeProvider.GetElapsedTime(start);

        BulkResult = String.Create(CultureInfo.CurrentCulture, $"Inserted {Rows(source.Count)} in {elapsed.TotalMilliseconds:N0} ms");
        await RefreshAsync().ConfigureAwait(true);
    }

    private async Task ReadAllAsync()
    {
        var start = timeProvider.GetTimestamp();
        var rows = await Task.Run(dataService.QueryAllBulkData).ConfigureAwait(true);
        var elapsed = timeProvider.GetElapsedTime(start);

        BulkResult = String.Create(CultureInfo.CurrentCulture, $"Read {Rows(rows.Count)} in {elapsed.TotalMilliseconds:N0} ms");
    }

    private async Task DeleteAllAsync()
    {
        var count = await dataService.DeleteAllBulkDataAsync().ConfigureAwait(true);
        BulkResult = "Deleted " + Rows(count);
        await RefreshAsync().ConfigureAwait(true);
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Database:Refresh", "Refresh tables", "Database", "\uE72C", RefreshCommand),
        new("Database:Rebuild", "Rebuild database", "Database", "\uE90F", RebuildCommand)
    ];
}
