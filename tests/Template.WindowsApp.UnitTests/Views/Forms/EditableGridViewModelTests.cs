namespace Template.WindowsApp.Views.Forms;

using System.IO;

using NSubstitute;

using Template.WindowsApp.Mocks;
using Template.WindowsApp.Services;
using Template.WindowsApp.Views.Dialogs;

[Collection(ViewModelTestGroup.Name)]
public sealed class EditableGridViewModelTests
{
    private static readonly FixedTimeProvider Time = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void ChangeSetsFieldsOnSelectedRowsExceptShipped()
    {
        // Arrange
        var title = string.Empty;
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowAsync(Arg.Any<DialogViewModelBase>()).Returns(x =>
        {
            var dialog = (BulkChangeDialogViewModel)x[0];
            title = dialog.Title;
            dialog.Status = OrderStatus.Confirmed;
            dialog.Discount = 5;
            return Task.FromResult(true);
        });
        using var viewModel = MakeViewModel(dialogService, new NotificationService(Time));
        viewModel.OnActivated();
        viewModel.SelectedRows = [viewModel.Lines[2], viewModel.Lines[3], viewModel.Lines[6]];

        // Act
        viewModel.ChangeCommand.Execute(null);

        // Assert
        Assert.Equal("Change 2 rows", title);
        Assert.Equal((OrderStatus.Confirmed, 5), (viewModel.Lines[2].Status, viewModel.Lines[2].Discount));
        Assert.Equal((OrderStatus.Confirmed, 5), (viewModel.Lines[3].Status, viewModel.Lines[3].Discount));
        Assert.Equal((OrderStatus.Shipped, 0), (viewModel.Lines[6].Status, viewModel.Lines[6].Discount));
        Assert.Equal(new DateTime(2026, 11, 6), viewModel.Lines[2].Delivery);
        Assert.Equal("2 changed", viewModel.Summary);
    }

    [Fact]
    public void ChangeCanceledKeepsRows()
    {
        // Arrange
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowAsync(Arg.Any<DialogViewModelBase>()).Returns(x =>
        {
            ((BulkChangeDialogViewModel)x[0]).Status = OrderStatus.Confirmed;
            return Task.FromResult(false);
        });
        using var viewModel = MakeViewModel(dialogService, new NotificationService(Time));
        viewModel.OnActivated();
        viewModel.SelectedRows = [viewModel.Lines[2]];

        // Act
        viewModel.ChangeCommand.Execute(null);

        // Assert
        Assert.Equal(OrderStatus.Draft, viewModel.Lines[2].Status);
        Assert.Equal("No changes", viewModel.Summary);
    }

    [Fact]
    public void ChangeNeedsRowThatIsNotShipped()
    {
        // Arrange
        using var viewModel = MakeViewModel(Substitute.For<IDialogService>(), new NotificationService(Time));
        viewModel.OnActivated();

        // Act
        viewModel.SelectedRows = [viewModel.Lines[6], viewModel.Lines[7]];
        var shippedOnly = viewModel.ChangeCommand.CanExecute(null);
        viewModel.SelectedRows = [viewModel.Lines[6], viewModel.Lines[0]];
        var withDraft = viewModel.ChangeCommand.CanExecute(null);

        // Assert
        Assert.False(shippedOnly);
        Assert.True(withDraft);
    }

    [Fact]
    public void ExportWritesCsvAndNotifies()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
        var dialogService = Substitute.For<IDialogService>();
        dialogService.SaveFile(Arg.Any<string?>(), Arg.Any<string?>()).Returns(path);
        var notifications = new NotificationService(Time);
        using var viewModel = MakeViewModel(dialogService, notifications);
        viewModel.OnActivated();

        try
        {
            // Act
            viewModel.ExportCommand.Execute(null);

            // Assert
            var bytes = File.ReadAllBytes(path);
            var lines = File.ReadAllLines(path);
            Assert.Equal([0xEF, 0xBB, 0xBF], bytes.Take(3));
            Assert.Equal(13, lines.Length);
            Assert.StartsWith("Item code,Description,", lines[0], StringComparison.Ordinal);
            Assert.Equal("AB-1001,Laptop stand,2,45.00,0,90.00,2026-11-03,Confirmed,", lines[1]);
            var notification = Assert.Single(notifications.History);
            Assert.Equal("Exported", notification.Title);
            Assert.Equal($"12 rows to {Path.GetFileName(path)}", notification.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ExportCanceledWritesNothing()
    {
        // Arrange
        var dialogService = Substitute.For<IDialogService>();
        dialogService.SaveFile(Arg.Any<string?>(), Arg.Any<string?>()).Returns((string?)null);
        var notifications = new NotificationService(Time);
        using var viewModel = MakeViewModel(dialogService, notifications);
        viewModel.OnActivated();

        // Act
        viewModel.ExportCommand.Execute(null);

        // Assert
        Assert.Empty(notifications.History);
    }

    private static EditableGridViewModel MakeViewModel(IDialogService dialogService, NotificationService notifications) =>
        new(Time, dialogService, Substitute.For<IClipboardService>(), notifications);
}
