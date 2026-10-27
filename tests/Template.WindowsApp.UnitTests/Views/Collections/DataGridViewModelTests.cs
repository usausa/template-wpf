namespace Template.WindowsApp.Views.Collections;

using NSubstitute;

using Template.WindowsApp.Models;
using Template.WindowsApp.Settings;
using Template.WindowsApp.Shell;

[Collection(ViewModelTestGroup.Name)]
public sealed class DataGridViewModelTests
{
    [Fact]
    public void FreezesCodeAndNameByDefault()
    {
        // Arrange
        using var viewModel = new DataGridViewModel(MakeWindowManager([]));

        // Assert
        Assert.True(viewModel.IsFrozen);
        Assert.Equal(2, viewModel.FrozenColumnCount);
        Assert.Equal(["Code", "Name"], viewModel.Columns.Where(static x => !x.CanHide).Select(static x => x.Header).ToArray());
    }

    [Fact]
    public void UnfreezingClearsFrozenColumns()
    {
        // Arrange
        var layouts = new Dictionary<string, GridLayout>();
        using var viewModel = new DataGridViewModel(MakeWindowManager(layouts));

        // Act
        viewModel.IsFrozen = false;

        // Assert
        Assert.Equal(0, viewModel.FrozenColumnCount);
        Assert.Equal(0, layouts["Products"].FrozenColumnCount);
    }

    [Fact]
    public void SavesColumnChanges()
    {
        // Arrange
        var layouts = new Dictionary<string, GridLayout>();
        using var viewModel = new DataGridViewModel(MakeWindowManager(layouts));

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
    public void RestoresSavedLayout()
    {
        // Arrange
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
        using var viewModel = new DataGridViewModel(MakeWindowManager(layouts));

        // Assert
        Assert.False(viewModel.IsFrozen);
        Assert.True(Column(viewModel, "Code").IsVisible);
        Assert.False(Column(viewModel, "Warehouse").IsVisible);
        Assert.Equal(150d, Column(viewModel, "Price").Width);
        Assert.Null(Column(viewModel, "Supplier").Width);
    }

    [Fact]
    public void ResetColumnsRestoresDefaults()
    {
        // Arrange
        var layouts = new Dictionary<string, GridLayout>();
        using var viewModel = new DataGridViewModel(MakeWindowManager(layouts));
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

    private static IWindowManager MakeWindowManager(Dictionary<string, GridLayout> layouts)
    {
        var windowManager = Substitute.For<IWindowManager>();
        windowManager.GridLayouts.Returns(layouts);
        return windowManager;
    }

    private static DataGridColumnSetting Column(DataGridViewModel viewModel, string header) =>
        viewModel.Columns.Single(x => x.Header == header);
}
