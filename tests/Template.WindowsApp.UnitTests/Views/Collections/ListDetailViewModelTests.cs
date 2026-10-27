namespace Template.WindowsApp.Views.Collections;

using NSubstitute;

using Template.WindowsApp.Components;
using Template.WindowsApp.Mocks;

[Collection(ViewModelTestGroup.Name)]
public sealed class ListDetailViewModelTests
{
    private static readonly FixedTimeProvider Time = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task LoadsCustomersAndSelectsFirst()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);

        // Act
        using var viewModel = new ListDetailViewModel(Substitute.For<IDialog>(), database.DataService);

        // Assert
        Assert.Equal(20, viewModel.Customers.Count);
        Assert.Same(viewModel.Customers[0], viewModel.SelectedCustomer);
        Assert.Equal(("Emma Wilson", "Contoso", "Seattle"), (viewModel.Name, viewModel.Company, viewModel.City));
    }

    [Fact]
    public async Task AddInsertsAndSelectsNewCustomer()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = new ListDetailViewModel(Substitute.For<IDialog>(), database.DataService);
        viewModel.OnActivated();

        // Act
        viewModel.AddCommand.Execute(null);
        using var reopened = new ListDetailViewModel(Substitute.For<IDialog>(), database.DataService);

        // Assert
        Assert.Equal(21, viewModel.Customers.Count);
        Assert.NotNull(viewModel.SelectedCustomer);
        Assert.Equal(21, viewModel.SelectedCustomer.Id);
        Assert.Equal("New customer", viewModel.Name);
        Assert.Equal(21, reopened.Customers.Count);
    }

    [Fact]
    public async Task SaveWritesToDatabase()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = new ListDetailViewModel(Substitute.For<IDialog>(), database.DataService);
        viewModel.OnActivated();
        viewModel.City = "Kyoto";
        var canSave = viewModel.SaveCommand.CanExecute(null);

        // Act
        viewModel.SaveCommand.Execute(null);
        using var reopened = new ListDetailViewModel(Substitute.For<IDialog>(), database.DataService);

        // Assert
        Assert.True(canSave);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
        Assert.Equal("Kyoto", viewModel.Customers[0].City);
        Assert.Equal("Kyoto", reopened.City);
    }

    [Fact]
    public async Task CancelRestoresSavedValues()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = new ListDetailViewModel(Substitute.For<IDialog>(), database.DataService);
        viewModel.OnActivated();
        viewModel.Name = "Changed";

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        Assert.Equal("Emma Wilson", viewModel.Name);
        Assert.False(viewModel.CancelCommand.CanExecute(null));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsksFirst(bool accepted)
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var dialog = Substitute.For<IDialog>();
        dialog.ConfirmAsync(Arg.Any<string>()).Returns(accepted);
        using var viewModel = new ListDetailViewModel(dialog, database.DataService);
        viewModel.OnActivated();
        viewModel.SelectedCustomer = viewModel.Customers[19];

        // Act
        viewModel.DeleteCommand.Execute(null);
        using var reopened = new ListDetailViewModel(Substitute.For<IDialog>(), database.DataService);

        // Assert
        var expected = accepted ? 19 : 20;
        Assert.Equal(expected, viewModel.Customers.Count);
        Assert.Equal(expected, reopened.Customers.Count);
        Assert.Equal(accepted ? "Evelyn Clark" : "Daniel Lewis", viewModel.Name);
    }
}
