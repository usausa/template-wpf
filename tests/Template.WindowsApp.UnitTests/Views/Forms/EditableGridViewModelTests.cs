namespace Template.WindowsApp.Views.Forms;

using System.IO;

using NSubstitute;

using Template.WindowsApp.Components;
using Template.WindowsApp.Mocks;
using Template.WindowsApp.Models.Sample;
using Template.WindowsApp.Services;

[Collection(ViewModelTestGroup.Name)]
public sealed class EditableGridViewModelTests
{
    private static readonly FixedTimeProvider Time = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task ChangeSetsFieldsOnSelectedRowsExceptShipped()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
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
        using var viewModel = MakeViewModel(dialog, new NotificationCenter(Time), database.DataService);
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
    public async Task ChangeCanceledKeepsRows()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var dialog = Substitute.For<IDialog>();
        dialog.ShowAsync(Arg.Any<AppDialogViewModelBase>()).Returns(x =>
        {
            ((BulkChangeDialogViewModel)x[0]).Status = OrderStatus.Confirmed;
            return Task.FromResult(false);
        });
        using var viewModel = MakeViewModel(dialog, new NotificationCenter(Time), database.DataService);
        viewModel.OnActivated();
        viewModel.SelectedRows = [viewModel.Lines[2]];

        // Act
        viewModel.ChangeCommand.Execute(null);

        // Assert
        Assert.Equal(OrderStatus.Draft, viewModel.Lines[2].Status);
        Assert.Equal("No changes", viewModel.Summary);
    }

    [Fact]
    public async Task ChangeNeedsRowThatIsNotShipped()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = MakeViewModel(Substitute.For<IDialog>(), new NotificationCenter(Time), database.DataService);
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
    public async Task ExportWritesCsvAndNotifies()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
        var dialog = Substitute.For<IDialog>();
        dialog.SaveFile(Arg.Any<string?>(), Arg.Any<string?>()).Returns(path);
        var notificationCenter = new NotificationCenter(Time);
        using var viewModel = MakeViewModel(dialog, notificationCenter, database.DataService);
        viewModel.OnActivated();

        try
        {
            // Act
            viewModel.ExportCommand.Execute(null);

            // Assert
            var bytes = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
            var lines = await File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
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
    public async Task ExportCanceledWritesNothing()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var dialog = Substitute.For<IDialog>();
        dialog.SaveFile(Arg.Any<string?>(), Arg.Any<string?>()).Returns((string?)null);
        var notificationCenter = new NotificationCenter(Time);
        using var viewModel = MakeViewModel(dialog, notificationCenter, database.DataService);
        viewModel.OnActivated();

        // Act
        viewModel.ExportCommand.Execute(null);

        // Assert
        Assert.Empty(notificationCenter.History);
    }

    [Fact]
    public async Task SaveWritesLinesToDatabase()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        using var viewModel = MakeViewModel(Substitute.For<IDialog>(), new NotificationCenter(Time), database.DataService);
        viewModel.OnActivated();

        // Act
        viewModel.Lines[2].Quantity = 5;
        viewModel.SaveCommand.Execute(null);
        using var reopened = MakeViewModel(Substitute.For<IDialog>(), new NotificationCenter(Time), database.DataService);

        // Assert
        Assert.Equal("No changes", viewModel.Summary);
        Assert.Equal(5, reopened.Lines[2].Quantity);
        Assert.Equal(12, reopened.Lines.Count);
    }

    private static EditableGridViewModel MakeViewModel(IDialog dialog, NotificationCenter notificationCenter, DataService dataService) =>
        new(Time, dialog, Substitute.For<IClipboard>(), notificationCenter, dataService);
}
