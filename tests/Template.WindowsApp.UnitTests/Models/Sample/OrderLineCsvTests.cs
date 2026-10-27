namespace Template.WindowsApp.Models.Sample;

using System.Globalization;

public sealed class OrderLineCsvTests
{
    private const string Header = "Item code,Description,Quantity,Unit price,Discount (%),Amount,Delivery,Status,Note\r\n";

    private static readonly DateTime Delivery = new(2026, 11, 3);

    [Fact]
    public void FormatWritesHeaderAndValues()
    {
        // Act
        var csv = OrderLineCsv.Format([new OrderLineValues("AB-1001", "Laptop stand", 2, 45m, 10, Delivery, OrderStatus.Confirmed, string.Empty)]);

        // Assert
        Assert.Equal(Header + "AB-1001,Laptop stand,2,45.00,10,81.00,2026-11-03,Confirmed,\r\n", csv);
    }

    [Fact]
    public void FormatLeavesMissingValuesEmpty()
    {
        // Act
        var csv = OrderLineCsv.Format([new OrderLineValues(string.Empty, string.Empty, null, null, 0, null, OrderStatus.Draft, string.Empty)]);

        // Assert
        Assert.Equal(Header + ",,,,0,,,Draft,\r\n", csv);
    }

    [Fact]
    public void FormatQuotesCommasQuotesAndLineBreaks()
    {
        // Act
        var csv = OrderLineCsv.Format([new OrderLineValues("AB-1", "Cable, 2 m", 1, 1m, 0, Delivery, OrderStatus.Draft, "Say \"hi\"\nthen ship")]);

        // Assert
        Assert.Equal(Header + "AB-1,\"Cable, 2 m\",1,1.00,0,1.00,2026-11-03,Draft,\"Say \"\"hi\"\"\nthen ship\"\r\n", csv);
    }

    [Fact]
    public void FormatUsesInvariantNumbers()
    {
        // Arrange
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            // Act
            var csv = OrderLineCsv.Format([new OrderLineValues("JK-5002", "Cable set", 20, 4.5m, 0, Delivery, OrderStatus.Confirmed, string.Empty)]);

            // Assert
            Assert.Equal(Header + "JK-5002,Cable set,20,4.50,0,90.00,2026-11-03,Confirmed,\r\n", csv);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void FormatWithoutLinesWritesHeaderOnly()
    {
        // Act
        var csv = OrderLineCsv.Format([]);

        // Assert
        Assert.Equal(Header, csv);
    }
}
