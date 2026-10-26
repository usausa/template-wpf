namespace Template.WindowsApp.Views.Forms;

[Collection(ViewModelTestGroup.Name)]
public sealed class GridVisualsViewModelTests
{
    [Fact]
    public void TotalsCoverAllServers()
    {
        // Arrange
        using var viewModel = new GridVisualsViewModel();

        // Assert
        Assert.Equal((12, 3, 1, 54), (viewModel.ServerCount, viewModel.WarningCount, viewModel.DownCount, viewModel.ErrorTotal));
        Assert.Equal(52d, Math.Round(viewModel.AverageCpu));
    }

    [Fact]
    public void GroupsByRoleAtFirst()
    {
        // Arrange
        using var viewModel = new GridVisualsViewModel();

        // Assert
        Assert.Equal(ServerGrouping.Role, viewModel.GroupBy);
    }

    [Fact]
    public void HourLabelsMarkEverySixHours()
    {
        // Act
        var labels = GridVisualsViewModel.HourLabels;

        // Assert
        Assert.Equal(24, labels.Count);
        Assert.Equal(["-18h", "-12h", "-6h", "now"], labels.Where(static x => x.Length > 0).ToArray());
    }
}
