namespace Template.WindowsApp.Views.Forms;

using System.IO;

using NSubstitute;

using Template.WindowsApp.Components;
using Template.WindowsApp.Mocks;
using Template.WindowsApp.Models.Sample;

[Collection(ViewModelTestGroup.Name)]
public sealed class EditableGridViewModelTests
{
    private static readonly FixedTimeProvider Time = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void ChangeSetsFieldsOnSelectedRowsExceptShipped()
    {
        // Arrange
        var title = string.Empty;
        var dialog = Substitute.For<IDialog>();
        dialog.ShowAsync(Arg.Any<AppDialogViewModelBase>()).Returns(x =>
        {
            var change = (BulkChangeDialogViewModel)x[0];
            title = change.Title;
            change.Status = OrderStatus.Confirmed;
            change.Discount = 5;
            return Task.FromResult(true);
        });
        using var viewModel = MakeViewModel(dialog, new NotificationCenter(Time));
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
        var dialog = Substitute.For<IDialog>();
        dialog.ShowAsync(Arg.Any<AppDialogViewModelBase>()).Returns(x =>
        {
            ((BulkChangeDialogViewModel)x[0]).Status = OrderStatus.Confirmed;
            return Task.FromResult(false);
        });
        using var viewModel = MakeViewModel(dialog, new NotificationCenter(Time));
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
        using var viewModel = MakeViewModel(Substitute.For<IDialog>(), new NotificationCenter(Time));
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
        var dialog = Substitute.For<IDialog>();
        dialog.SaveFile(Arg.Any<string?>(), Arg.Any<string?>()).Returns(path);
        var notificationCenter = new NotificationCenter(Time);
        using var viewModel = MakeViewModel(dialog, notificationCenter);
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
            var notification = Assert.Single(notificationCenter.History);
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
        var dialog = Substitute.For<IDialog>();
        dialog.SaveFile(Arg.Any<string?>(), Arg.Any<string?>()).Returns((string?)null);
        var notificationCenter = new NotificationCenter(Time);
        using var viewModel = MakeViewModel(dialog, notificationCenter);
        viewModel.OnActivated();

        // Act
        viewModel.ExportCommand.Execute(null);

        // Assert
        Assert.Empty(notificationCenter.History);
    }

    private static EditableGridViewModel MakeViewModel(IDialog dialog, NotificationCenter notificationCenter) =>
        new(Time, dialog, Substitute.For<IClipboard>(), notificationCenter);
}
