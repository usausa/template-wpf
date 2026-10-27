namespace Template.WindowsApp.Services;

using System.Globalization;
using System.IO;

using Template.WindowsApp.Mocks;
using Template.WindowsApp.Models;
using Template.WindowsApp.Models.Entity;
using Template.WindowsApp.Models.Sample;

public sealed class DataServiceTests
{
    private static readonly FixedTimeProvider Time = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task RebuildCreatesTablesWithSample()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);

        // Act
        var info = await database.DataService.GetDatabaseInfoAsync();

        // Assert
        Assert.True(info.Size > 0);
        Assert.Equal((50L, 20L, 12L, 0L, 0L), (info.ProductCount, info.CustomerCount, info.OrderLineCount, info.DataCount, info.BulkDataCount));
    }

    [Fact]
    public async Task InitializeCreatesOnlyMissingFile()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var service = database.DataService;
        await service.InsertDataAsync(new DataEntity { Id = 1, Name = "Kept", CreateAt = Time.GetUtcNow().UtcDateTime });

        // Act
        await service.InitializeAsync();
        var kept = await service.GetDatabaseInfoAsync();
        File.Delete(service.DatabasePath);
        await service.InitializeAsync();
        var created = await service.GetDatabaseInfoAsync();

        // Assert
        Assert.Equal(1, kept.DataCount);
        Assert.Equal((50L, 0L), (created.ProductCount, created.DataCount));
    }

    [Theory]
    [InlineData("", 50)]
    [InlineData("editor", 4)]
    [InlineData(" P-00 ", 9)]
    [InlineData("%", 0)]
    [InlineData("_", 0)]
    public async Task CountProductsMatchesCodeOrName(string keyword, long expected)
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);

        // Act
        var count = database.DataService.CountProducts(keyword);

        // Assert
        Assert.Equal(expected, count);
    }

    [Fact]
    public async Task QueryProductsSortsAndPages()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var service = database.DataService;

        // Act
        var first = service.QueryProducts(string.Empty, ProductSort.Price, true, 0, 15);
        var last = service.QueryProducts(string.Empty, ProductSort.Price, true, 45, 15);

        // Assert
        Assert.Equal(15, first.Count);
        Assert.Equal(5, last.Count);
        Assert.Equal(ProductSample.LoadProducts().Max(static x => x.Price), first[0].Price);
        Assert.Equal(first.Select(static x => x.Price).OrderDescending(), first.Select(static x => x.Price));
        Assert.True(first[^1].Price >= last[0].Price);
    }

    [Fact]
    public async Task CustomerInsertUpdateAndDelete()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var service = database.DataService;

        // Act
        var added = await service.InsertCustomerAsync("New customer");
        added.City = "Kyoto";
        var updated = await service.UpdateCustomerAsync(added);
        var afterUpdate = service.QueryCustomers();
        var deleted = await service.DeleteCustomerAsync(1);
        var afterDelete = service.QueryCustomers();

        // Assert
        Assert.Equal(21, added.Id);
        Assert.Equal(1, updated);
        Assert.Equal("Kyoto", afterUpdate.Single(x => x.Id == added.Id).City);
        Assert.Equal(1, deleted);
        Assert.Equal(20, afterDelete.Count);
        Assert.DoesNotContain(afterDelete, static x => x.Id == 1);
    }

    [Fact]
    public async Task ReplaceOrderLinesKeepsOnlyGivenLines()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var service = database.DataService;
        var lines = service.QueryOrderLines().Take(3).ToList();
        lines[0].Quantity = 9;

        // Act
        await service.ReplaceOrderLinesAsync(lines);
        var saved = service.QueryOrderLines();

        // Assert
        Assert.Equal(3, saved.Count);
        Assert.Equal(9, saved[0].Quantity);
    }

    [Fact]
    public async Task InsertDataReportsDuplicateKey()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var service = database.DataService;
        var entity = new DataEntity { Id = 1, Name = "Data-1", CreateAt = Time.GetUtcNow().UtcDateTime };

        // Act
        var first = await service.InsertDataAsync(entity);
        var second = await service.InsertDataAsync(entity);
        var saved = await service.QueryDataAsync(1);

        // Assert
        Assert.True(first);
        Assert.False(second);
        Assert.NotNull(saved);
        Assert.Equal("Data-1", saved.Name);
        Assert.Equal(entity.CreateAt, saved.CreateAt);
        Assert.Equal(DateTimeKind.Utc, saved.CreateAt.Kind);
    }

    [Fact]
    public async Task BulkDataInsertReadAndDelete()
    {
        // Arrange
        using var database = await TestDatabase.CreateAsync(Time);
        var service = database.DataService;
        var source = Enumerable.Range(1, 100)
            .Select(static x => new BulkDataEntity { Key1 = "01", Key2 = x.ToString("D3", CultureInfo.InvariantCulture), Key3 = "0", Value1 = x })
            .ToList();

        // Act
        service.InsertBulkData(source);
        var count = await service.CountBulkDataAsync();
        var rows = service.QueryAllBulkData();
        var deleted = await service.DeleteAllBulkDataAsync();
        var remaining = await service.CountBulkDataAsync();

        // Assert
        Assert.Equal(100, count);
        Assert.Equal(100, rows.Count);
        Assert.Equal(100, deleted);
        Assert.Equal(0, remaining);
    }
}
