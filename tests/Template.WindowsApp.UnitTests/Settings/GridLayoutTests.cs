namespace Template.WindowsApp.Settings;

using System.Text.Json;

public sealed class GridLayoutTests
{
    [Fact]
    public void RoundTripsThroughJson()
    {
        // Arrange
        var layouts = new Dictionary<string, GridLayout>
        {
            ["Products"] = new()
            {
                FrozenColumnCount = 2,
                Columns = [new() { Key = "Supplier", IsVisible = false }, new() { Key = "Name", Width = 300 }]
            }
        };

        // Act
        var restored = JsonSerializer.Deserialize<Dictionary<string, GridLayout>>(JsonSerializer.Serialize(layouts));

        // Assert
        var layout = Assert.Contains("Products", restored!);
        Assert.Equal(2, layout.FrozenColumnCount);
        Assert.Equal([("Supplier", false, null), ("Name", true, 300d)], layout.Columns.Select(static x => (x.Key, x.IsVisible, x.Width)).ToArray());
    }
}
