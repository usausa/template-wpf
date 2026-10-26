namespace Template.WindowsApp.Views.Forms;

using System.Globalization;

using Template.WindowsApp.Mocks;

public sealed class OrderLineTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    private static readonly DateTime Today = Now.Date;

    private static readonly FixedTimeProvider Time = new(Now);

    [Fact]
    public void LoadedLineHasNoErrorsOrChanges()
    {
        // Act
        var line = OrderLine.Load(MakeValues(), Time);

        // Assert
        Assert.False(line.IsNew);
        Assert.False(line.IsChanged);
        Assert.False(line.HasErrors);
    }

    [Fact]
    public void AmountAppliesDiscount()
    {
        // Act
        var line = OrderLine.Load(MakeValues() with { Quantity = 3, UnitPrice = 10.00m, Discount = 10 }, Time);

        // Assert
        Assert.Equal(27.00m, line.Amount);
    }

    [Fact]
    public void AmountIsEmptyWithoutQuantity()
    {
        // Act
        var line = OrderLine.Load(MakeValues() with { Quantity = null }, Time);

        // Assert
        Assert.Null(line.Amount);
    }

    [Fact]
    public void ChangedValueMarksCell()
    {
        // Arrange
        var line = OrderLine.Load(MakeValues(), Time);

        // Act
        line.Code = "KB-200";

        // Assert
        Assert.True(line.IsChanged);
        Assert.True(line.IsCellChanged(nameof(OrderLine.Code)));
        Assert.False(line.IsCellChanged(nameof(OrderLine.Description)));
    }

    [Fact]
    public void InvalidCodeShowsError()
    {
        // Arrange
        var line = OrderLine.Load(MakeValues(), Time);

        // Act
        line.Code = "kb 200";

        // Assert
        Assert.Equal("Use capital letters, digits and hyphens.", line.GetCellError(nameof(OrderLine.Code)));
    }

    [Fact]
    public void NewLineShowsErrorsOnlyOfTouchedCells()
    {
        // Arrange
        var line = OrderLine.Create(MakeEmptyValues(), Time);

        // Act
        line.Code = "x";

        // Assert
        Assert.NotNull(line.GetCellError(nameof(OrderLine.Code)));
        Assert.Null(line.GetCellError(nameof(OrderLine.Description)));
    }

    [Fact]
    public void RevealErrorsShowsAllErrorsOfNewLine()
    {
        // Arrange
        var line = OrderLine.Create(MakeEmptyValues(), Time);

        // Act
        line.RevealErrors();

        // Assert
        Assert.True(line.HasErrors);
        Assert.Equal("Enter an item code.", line.GetCellError(nameof(OrderLine.Code)));
        Assert.Equal("Choose a delivery date.", line.GetCellError(nameof(OrderLine.Delivery)));
    }

    [Fact]
    public void LargeDiscountNeedsNote()
    {
        // Arrange
        var line = OrderLine.Load(MakeValues(), Time);

        // Act
        line.Discount = 25;

        // Assert
        Assert.Equal("A discount over 20% needs a note.", line.GetCellError(nameof(OrderLine.Note)));
    }

    [Fact]
    public void ChangedDeliveryBeforeTodayIsError()
    {
        // Arrange
        var line = OrderLine.Load(MakeValues(), Time);

        // Act
        line.Delivery = Today.AddDays(-1);

        // Assert
        Assert.Equal("Choose today or a later date.", line.GetCellError(nameof(OrderLine.Delivery)));
    }

    [Fact]
    public void SavedDeliveryBeforeTodayIsNotError()
    {
        // Act
        var line = OrderLine.Load(MakeValues() with { Delivery = Today.AddDays(-1) }, Time);

        // Assert
        Assert.Null(line.GetCellError(nameof(OrderLine.Delivery)));
    }

    [Fact]
    public void ShippedLineIsLockedWithoutErrors()
    {
        // Act
        var line = OrderLine.Load(MakeValues() with { Code = "x", Status = OrderStatus.Shipped }, Time);

        // Assert
        Assert.True(line.IsLocked);
        Assert.False(line.HasErrors);
    }

    [Theory]
    [InlineData(nameof(OrderLine.Code), " kb-300 ", true)]
    [InlineData(nameof(OrderLine.Code), "kb 300", false)]
    [InlineData(nameof(OrderLine.Quantity), "5", true)]
    [InlineData(nameof(OrderLine.Quantity), "abc", false)]
    [InlineData(nameof(OrderLine.Quantity), "1000", false)]
    [InlineData(nameof(OrderLine.Discount), "51", false)]
    [InlineData(nameof(OrderLine.Delivery), "2026-10-27", true)]
    [InlineData(nameof(OrderLine.Delivery), "2026-10-25", false)]
    [InlineData(nameof(OrderLine.Status), "shipped", true)]
    [InlineData(nameof(OrderLine.Status), "Closed", false)]
    [InlineData(nameof(OrderLine.Amount), "10", false)]
    public void TryPasteAcceptsOnlyValidValues(string key, string text, bool expected)
    {
        // Arrange
        var line = OrderLine.Create(MakeEmptyValues(), Time);

        // Act
        var pasted = line.TryPaste(key, text);

        // Assert
        Assert.Equal(expected, pasted);
    }

    [Theory]
    [InlineData(12.34, true)]
    [InlineData(12.345, false)]
    [InlineData(0, false)]
    public void TryPasteAcceptsUnitPriceWithTwoDecimals(double price, bool expected)
    {
        // Arrange
        var line = OrderLine.Create(MakeEmptyValues(), Time);

        // Act
        var pasted = line.TryPaste(nameof(OrderLine.UnitPrice), ((decimal)price).ToString(CultureInfo.CurrentCulture));

        // Assert
        Assert.Equal(expected, pasted);
    }

    [Fact]
    public void TryPasteNormalizesCode()
    {
        // Arrange
        var line = OrderLine.Create(MakeEmptyValues(), Time);

        // Act
        line.TryPaste(nameof(OrderLine.Code), " kb-300 ");

        // Assert
        Assert.Equal("KB-300", line.Code);
    }

    [Fact]
    public void CancelEditRestoresValues()
    {
        // Arrange
        var line = OrderLine.Load(MakeValues(), Time);
        line.BeginEdit();
        line.Code = "KB-999";

        // Act
        line.CancelEdit();

        // Assert
        Assert.Equal("KB-100", line.Code);
        Assert.False(line.IsChanged);
    }

    [Fact]
    public void CancelEditOfUncommittedLineDiscardsIt()
    {
        // Arrange
        var line = OrderLine.Create(MakeEmptyValues(), Time);
        var discarded = false;
        line.Discarded += (_, _) => discarded = true;
        line.BeginEdit();

        // Act
        line.CancelEdit();

        // Assert
        Assert.True(discarded);
    }

    [Fact]
    public void AcceptChangesSavesValues()
    {
        // Arrange
        var line = OrderLine.Load(MakeValues(), Time);
        line.Code = "KB-200";

        // Act
        line.AcceptChanges();

        // Assert
        Assert.False(line.IsChanged);
        Assert.False(line.IsCellChanged(nameof(OrderLine.Code)));
    }

    private static OrderLineValues MakeValues() =>
        new("KB-100", "Keyboard", 2, 10.00m, 0, Today.AddDays(1), OrderStatus.Draft, string.Empty);

    private static OrderLineValues MakeEmptyValues() =>
        new(string.Empty, string.Empty, null, null, null, null, OrderStatus.Draft, string.Empty);
}
