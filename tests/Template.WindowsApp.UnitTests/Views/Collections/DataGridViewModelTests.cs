namespace Template.WindowsApp.Views.Collections;

using System.ComponentModel;

using NSubstitute;

using Template.WindowsApp.Mocks;
using Template.WindowsApp.Models;
using Template.WindowsApp.Settings;
using Template.WindowsApp.Shell;

[Collection(ViewModelTestGroup.Name)]
public sealed class DataGridViewModelTests
{
    private static readonly FixedTimeProvider Time = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task FreezesCodeAndNameByDefault()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = new DataGridViewModel(MakeWindowManager([]), database.DataService);

        // Assert
        Assert.True(viewModel.IsFrozen);
        Assert.Equal(2, viewModel.FrozenColumnCount);
        Assert.Equal(["Code", "Name"], viewModel.Columns.Where(static x => !x.CanHide).Select(static x => x.Header).ToArray());
    }

    [Fact]
    public async Task UnfreezingClearsFrozenColumns()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var layouts = new Dictionary<string, GridLayout>();
        using var viewModel = new DataGridViewModel(MakeWindowManager(layouts), database.DataService);

        // Act
        viewModel.IsFrozen = false;

        // Assert
        Assert.Equal(0, viewModel.FrozenColumnCount);
        Assert.Equal(0, layouts["Products"].FrozenColumnCount);
    }

    [Fact]
    public async Task SavesColumnChanges()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var layouts = new Dictionary<string, GridLayout>();
        using var viewModel = new DataGridViewModel(MakeWindowManager(layouts), database.DataService);

        // Act
        Column(viewModel, "Supplier").IsVisible = false;
        Column(viewModel, "Name").Width = 300;

        // Assert
        var saved = layouts["Products"];
        Assert.Equal(2, saved.FrozenColumnCount);
        Assert.Equal(9, saved.Columns.Count);
        Assert.False(saved.Columns.Single(static x => x.Key == "Supplier").IsVisible);
        Assert.Equal(300d, saved.Columns.Single(static x => x.Key == "Name").Width);
    }

    [Fact]
    public async Task RestoresSavedLayout()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var layouts = new Dictionary<string, GridLayout>
        {
            ["Products"] = new()
            {
                FrozenColumnCount = 0,
                Columns =
                [
                    new() { Key = "Code", IsVisible = false },
                    new() { Key = "Warehouse", IsVisible = false },
                    new() { Key = "Price", Width = 150 },
                    new() { Key = "Removed", IsVisible = false }
                ]
            }
        };

        // Act
        using var viewModel = new DataGridViewModel(MakeWindowManager(layouts), database.DataService);

        // Assert
        Assert.False(viewModel.IsFrozen);
        Assert.True(Column(viewModel, "Code").IsVisible);
        Assert.False(Column(viewModel, "Warehouse").IsVisible);
        Assert.Equal(150d, Column(viewModel, "Price").Width);
        Assert.Null(Column(viewModel, "Supplier").Width);
    }

    [Fact]
    public async Task ResetColumnsRestoresDefaults()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var layouts = new Dictionary<string, GridLayout>();
        using var viewModel = new DataGridViewModel(MakeWindowManager(layouts), database.DataService);
        viewModel.OnActivated();
        viewModel.IsFrozen = false;
        Column(viewModel, "Category").IsVisible = false;
        Column(viewModel, "Price").Width = 180;

        // Act
        viewModel.ResetColumnsCommand.Execute(null);

        // Assert
        Assert.True(viewModel.IsFrozen);
        Assert.All(viewModel.Columns, static x => Assert.True(x.IsVisible && (x.Width is null)));
        Assert.Equal(2, layouts["Products"].FrozenColumnCount);
        Assert.All(layouts["Products"].Columns, static x => Assert.True(x.IsVisible && (x.Width is null)));
    }

    [Fact]
    public async Task ShowsFirstPageFromDatabase()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);

        // Act
        using var viewModel = new DataGridViewModel(MakeWindowManager([]), database.DataService);

        // Assert
        Assert.Equal(50, viewModel.TotalCount);
        Assert.Equal(15, viewModel.Items.Count);
        Assert.Equal((1, 15), (viewModel.RangeStart, viewModel.RangeEnd));
        Assert.Equal("P-001", viewModel.Items[0].Code);
    }

    [Fact]
    public async Task SearchSortAndPageUseDatabase()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = new DataGridViewModel(MakeWindowManager([]), database.DataService);
        viewModel.OnActivated();

        // Act
        viewModel.SearchText = "editor";
        var found = viewModel.Items.Select(static x => x.Name).ToArray();
        viewModel.SearchText = string.Empty;
        viewModel.Sort = new SortDescription("Price", ListSortDirection.Descending);
        var highest = viewModel.Items[0].Price;
        viewModel.NextCommand.Execute(null);

        // Assert
        Assert.Equal(["Photo editor", "Video editor", "PDF editor", "Code editor"], found);
        Assert.Equal(Math.Max(highest, viewModel.Items[0].Price), highest);
        Assert.Equal((16, 30), (viewModel.RangeStart, viewModel.RangeEnd));
    }

    [Fact]
    public async Task EditIsSavedAndResetRestoresSample()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = new DataGridViewModel(MakeWindowManager([]), database.DataService);
        viewModel.OnActivated();
        var product = viewModel.Items[0];

        // Act
        product.Name = "Ergonomic keyboard";
        viewModel.UpdateCommand.Execute(product);
        using var reopened = new DataGridViewModel(MakeWindowManager([]), database.DataService);
        var saved = reopened.Items[0].Name;
        viewModel.ResetCommand.Execute(null);
        using var reset = new DataGridViewModel(MakeWindowManager([]), database.DataService);

        // Assert
        Assert.Equal("Ergonomic keyboard", saved);
        Assert.Equal("Keyboard", reset.Items[0].Name);
    }

    private static IWindowManager MakeWindowManager(Dictionary<string, GridLayout> layouts)
    {
        var windowManager = Substitute.For<IWindowManager>();
        windowManager.GridLayouts.Returns(layouts);
        return windowManager;
    }

    private static DataGridColumnSetting Column(DataGridViewModel viewModel, string header) =>
        viewModel.Columns.Single(x => x.Header == header);
}
