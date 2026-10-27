namespace Template.WindowsApp.Views.Platform;

using System.Globalization;

using NSubstitute;

using Smart.Navigation;

using Template.WindowsApp.Components;
using Template.WindowsApp.Mocks;
using Template.WindowsApp.Models.Entity;
using Template.WindowsApp.Services;

[Collection(ViewModelTestGroup.Name)]
public sealed class DatabaseViewModelTests
{
    private static readonly FixedTimeProvider Time = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task NavigatingShowsTableCounts()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = MakeViewModel(Substitute.For<IDialog>(), database.DataService);

        // Act
        await viewModel.OnNavigatingToAsync(Substitute.For<INavigationContext>());

        // Assert
        Assert.Equal(database.DataService.DatabasePath, viewModel.DatabasePath);
        Assert.NotEmpty(viewModel.DatabaseSize);
        Assert.Equal((50L, 20L, 12L, 0L, 0L), (viewModel.ProductCount, viewModel.CustomerCount, viewModel.OrderLineCount, viewModel.DataCount, viewModel.BulkDataCount));
        Assert.Equal(82, viewModel.TotalCount);
    }

    [Fact]
    public async Task SingleRowCommandsShowResult()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = MakeViewModel(Substitute.For<IDialog>(), database.DataService);
        viewModel.OnActivated();

        // Act
        viewModel.InsertCommand.Execute(null);
        var inserted = viewModel.RowResult;
        viewModel.InsertCommand.Execute(null);
        var duplicate = viewModel.RowResult;
        viewModel.UpdateCommand.Execute(null);
        var updated = viewModel.RowResult;
        viewModel.QueryCommand.Execute(null);
        var found = viewModel.RowResult;
        viewModel.DeleteCommand.Execute(null);
        var deleted = viewModel.RowResult;
        viewModel.QueryCommand.Execute(null);
        var missing = viewModel.RowResult;

        // Assert
        Assert.Equal("Inserted", inserted);
        Assert.Equal("Key duplicate", duplicate);
        Assert.Equal("Updated 1 row", updated);
        Assert.StartsWith("Updated, created ", found, StringComparison.Ordinal);
        Assert.Equal("Deleted 1 row", deleted);
        Assert.Equal("Not found", missing);
        Assert.Equal(0, viewModel.DataCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RebuildAsksFirst(bool accepted)
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var dialog = Substitute.For<IDialog>();
        dialog.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(accepted);
        using var viewModel = MakeViewModel(dialog, database.DataService);
        viewModel.OnActivated();
        viewModel.InsertCommand.Execute(null);

        // Act
        viewModel.RebuildCommand.Execute(null);

        // Assert
        Assert.Equal(accepted ? "Rebuilt with the sample data" : null, viewModel.FileResult);
        Assert.Equal(accepted ? 0 : 1, viewModel.DataCount);
    }

    [Fact]
    public async Task BulkCommandsFollowRowCount()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = MakeViewModel(Substitute.For<IDialog>(), database.DataService);
        viewModel.OnActivated();
        await viewModel.OnNavigatingToAsync(Substitute.For<INavigationContext>());
        var empty = (viewModel.BulkInsertCommand.CanExecute(null), viewModel.ReadAllCommand.CanExecute(null), viewModel.DeleteAllCommand.CanExecute(null));
        database.DataService.InsertBulkData(Enumerable.Range(1, 100).Select(static x => new BulkDataEntity { Key1 = "01", Key2 = x.ToString("D3", CultureInfo.InvariantCulture), Key3 = "0" }));
        await viewModel.OnNavigatingToAsync(Substitute.For<INavigationContext>());
        var filled = (viewModel.BulkInsertCommand.CanExecute(null), viewModel.ReadAllCommand.CanExecute(null), viewModel.DeleteAllCommand.CanExecute(null));

        // Act
        viewModel.DeleteAllCommand.Execute(null);

        // Assert
        Assert.Equal((true, false, false), empty);
        Assert.Equal((false, true, true), filled);
        Assert.Equal("Deleted 100 rows", viewModel.BulkResult);
        Assert.Equal(0, viewModel.BulkDataCount);
    }

    private static DatabaseViewModel MakeViewModel(IDialog dialog, DataService dataService) =>
        new(Time, dialog, Substitute.For<IExplorerLauncher>(), dataService);
}
