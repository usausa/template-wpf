namespace Template.WindowsApp.Views.Fundamentals;

using Template.WindowsApp.Mocks;

[Collection(ViewModelTestGroup.Name)]
public sealed class BindingViewModelTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StartsWithSampleValues()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(Now);

        // Act
        using var viewModel = new BindingViewModel(timeProvider);

        // Assert
        Assert.Equal("Aiko", viewModel.Name);
        Assert.Equal(new DateTime(2026, 10, 26), viewModel.Date);
        Assert.Null(viewModel.Nickname);
        Assert.Same(viewModel.People[0], viewModel.SelectedPerson);
    }

    [Fact]
    public void NicknameCanBeSetAndCleared()
    {
        // Arrange
        using var viewModel = new BindingViewModel(new FixedTimeProvider(Now));
        viewModel.OnActivated();

        // Act
        viewModel.SetNicknameCommand.Execute(null);
        var set = viewModel.Nickname;
        viewModel.ClearNicknameCommand.Execute(null);

        // Assert
        Assert.Equal("Ai", set);
        Assert.Null(viewModel.Nickname);
    }

    [Fact]
    public void ResetRestoresSampleValues()
    {
        // Arrange
        using var viewModel = new BindingViewModel(new FixedTimeProvider(Now));
        viewModel.OnActivated();
        viewModel.Name = "Ben";
        viewModel.Date = null;
        viewModel.ShowsWeekday = true;
        viewModel.FirstName = "Ben";
        viewModel.SelectedPerson = viewModel.People[2];

        // Act
        viewModel.ResetCommand.Execute(null);

        // Assert
        Assert.Equal("Aiko", viewModel.Name);
        Assert.Equal(new DateTime(2026, 10, 26), viewModel.Date);
        Assert.False(viewModel.ShowsWeekday);
        Assert.Equal("Aiko", viewModel.FirstName);
        Assert.Same(viewModel.People[0], viewModel.SelectedPerson);
    }
}
