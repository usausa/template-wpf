namespace Template.WindowsApp.Views.Forms;

using System.Globalization;

public sealed class ServerGroupSummaryConverterTests
{
    [Fact]
    public void SummarizesServersCpuAndErrors()
    {
        // Arrange
        var converter = new ServerGroupSummaryConverter();
        ServerRow[] servers =
        [
            new() { Name = "web-01", Cpu = 40, Errors = 3 },
            new() { Name = "web-02", Cpu = 62, Errors = 5 },
            new() { Name = "web-03", Status = ServerState.Down }
        ];

        // Act
        var text = converter.Convert(servers, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("Servers 3 · CPU average 51% · Errors 8", text);
    }

    [Fact]
    public void ShowsDashWhenNoServerHasCpu()
    {
        // Arrange
        var converter = new ServerGroupSummaryConverter();
        ServerRow[] servers = [new() { Name = "mail-01", Status = ServerState.Down }];

        // Act
        var text = converter.Convert(servers, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("Servers 1 · CPU average - · Errors 0", text);
    }
}
