namespace Template.WindowsApp.Views.Collections;

using Template.WindowsApp.Models;
using Template.WindowsApp.Models.Entity;
using Template.WindowsApp.Models.Sample;
using Template.WindowsApp.Services;
using Template.WindowsApp.Settings;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class DataGridViewModel : AppViewModelBase, IShellCommandProvider
{
    private const int PageSize = 15;

    private const string LayoutKey = "Products";

    private const int FrozenCount = 2;

    private readonly IWindowManager windowManager;

    private readonly DataService dataService;

    private int pageIndex;

    public static IReadOnlyList<string> Categories { get; } = ProductSample.LoadCategories();

    public ObservableCollection<ProductEntity> Items { get; } = [];

    public IReadOnlyList<DataGridColumnSetting> Columns { get; } =
    [
        new("Code", false),
        new("Name", false),
        new("Category", true),
        new("Supplier", true),
        new("Warehouse", true),
        new("Price", true),
        new("Stock", true),
        new("Updated", true),
        new("Active", true)
    ];

    [ObservableProperty]
    public partial string SearchText { get; set; }

    [ObservableProperty]
    public partial SortDescription? Sort { get; set; }

    [ObservableProperty]
    public partial int RangeStart { get; set; }

    [ObservableProperty]
    public partial int RangeEnd { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(FrozenColumnCount)])]
    public partial bool IsFrozen { get; set; }

    public int FrozenColumnCount => IsFrozen ? FrozenCount : 0;

    public IObserveCommand PreviousCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public IObserveCommand ResetColumnsCommand { get; }

    public IObserveCommand UpdateCommand { get; }

    public DataGridViewModel(IWindowManager windowManager, DataService dataService)
    {
        this.windowManager = windowManager;
        this.dataService = dataService;
        SearchText = string.Empty;
        IsFrozen = true;
        RestoreLayout();

        SubscribeSearchText(_ => ShowPage(0));
        SubscribeSort(_ => ShowPage(0));
        SubscribeIsFrozen(_ => SaveLayout());
        foreach (var column in Columns)
        {
            column.PropertyChanged += OnColumnChanged;
        }

        PreviousCommand = MakeDelegateCommand(() => ShowPage(pageIndex - 1), () => pageIndex > 0);
        NextCommand = MakeDelegateCommand(() => ShowPage(pageIndex + 1), () => RangeEnd < TotalCount);
        ResetCommand = MakeAsyncCommand(ResetAsync);
        ResetColumnsCommand = MakeDelegateCommand(ResetColumns);
        UpdateCommand = MakeAsyncCommand<ProductEntity>(UpdateAsync);

        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        var (sort, desc) = SortKey();
        TotalCount = (int)dataService.CountProducts(SearchText);
        pageIndex = index;
        Items.Clear();
        foreach (var product in dataService.QueryProducts(SearchText, sort, desc, index * PageSize, PageSize))
        {
            Items.Add(product);
        }

        RangeStart = Items.Count > 0 ? (index * PageSize) + 1 : 0;
        RangeEnd = (index * PageSize) + Items.Count;
    }

    private (ProductSort Sort, bool Desc) SortKey()
    {
        if (Sort is not { } sort)
        {
            return (ProductSort.Code, false);
        }

        var key = sort.PropertyName switch
        {
            nameof(ProductEntity.Name) => ProductSort.Name,
            nameof(ProductEntity.Category) => ProductSort.Category,
            nameof(ProductEntity.Supplier) => ProductSort.Supplier,
            nameof(ProductEntity.Warehouse) => ProductSort.Warehouse,
            nameof(ProductEntity.Updated) => ProductSort.Updated,
            nameof(ProductEntity.Price) => ProductSort.Price,
            nameof(ProductEntity.Stock) => ProductSort.Stock,
            nameof(ProductEntity.IsActive) => ProductSort.IsActive,
            _ => ProductSort.Code
        };
        return (key, sort.Direction == ListSortDirection.Descending);
    }

    private Task UpdateAsync(ProductEntity product) =>
        dataService.UpdateProductAsync(product).AsTask();

    private void RestoreLayout()
    {
        if (!windowManager.GridLayouts.TryGetValue(LayoutKey, out var layout))
        {
            return;
        }

        IsFrozen = layout.FrozenColumnCount > 0;
        foreach (var saved in layout.Columns)
        {
            if (Columns.FirstOrDefault(x => x.Header == saved.Key) is { } column)
            {
                column.IsVisible = saved.IsVisible || !column.CanHide;
                column.Width = saved.Width;
            }
        }
    }

    private void SaveLayout() =>
        windowManager.GridLayouts[LayoutKey] = new GridLayout
        {
            FrozenColumnCount = FrozenColumnCount,
            Columns = Columns.Select(static x => new GridColumnLayout { Key = x.Header, IsVisible = x.IsVisible, Width = x.Width }).ToList()
        };

    private void OnColumnChanged(object? sender, PropertyChangedEventArgs e) => SaveLayout();

    private void ResetColumns()
    {
        foreach (var column in Columns)
        {
            column.IsVisible = true;
            column.Width = null;
        }
        IsFrozen = true;
    }

    private async Task ResetAsync()
    {
        await dataService.ResetProductsAsync().ConfigureAwait(true);
        SearchText = string.Empty;
        Sort = null;
        ShowPage(0);
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("DataGrid:Reset", "Reset", "Data grid", "\uE72C", ResetCommand),
        new("DataGrid:ResetColumns", "Reset columns", "Data grid", "\uE72C", ResetColumnsCommand)
    ];
}
