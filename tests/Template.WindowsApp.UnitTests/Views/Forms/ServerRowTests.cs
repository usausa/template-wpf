namespace Template.WindowsApp.Views.Forms;

public sealed class ServerRowTests
{
    [Theory]
    [InlineData(null, MetricLevel.Normal)]
    [InlineData(79.9, MetricLevel.Normal)]
    [InlineData(80.0, MetricLevel.Caution)]
    [InlineData(89.9, MetricLevel.Caution)]
    [InlineData(90.0, MetricLevel.Critical)]
    public void CpuLevelFollowsThresholds(double? cpu, MetricLevel expected)
    {
        // Act
        var row = new ServerRow { Name = "web-01", Cpu = cpu };

        // Assert
        Assert.Equal(expected, row.CpuLevel);
    }

    [Theory]
    [InlineData(15.0, MetricLevel.Normal)]
    [InlineData(16.0, MetricLevel.Caution)]
    [InlineData(18.0, MetricLevel.Critical)]
    public void MemoryLevelUsesPercentOfTotal(double used, MetricLevel expected)
    {
        // Act
        var row = new ServerRow { Name = "web-01", MemoryUsed = used, MemoryTotal = 20 };

        // Assert
        Assert.Equal(expected, row.MemoryLevel);
    }

    [Theory]
    [InlineData(99.95, MetricLevel.Normal)]
    [InlineData(99.9, MetricLevel.Normal)]
    [InlineData(99.5, MetricLevel.Caution)]
    [InlineData(99.0, MetricLevel.Caution)]
    [InlineData(98.9, MetricLevel.Critical)]
    public void UptimeLevelFollowsThresholds(double uptime, MetricLevel expected)
    {
        // Act
        var row = new ServerRow { Name = "web-01", Uptime = uptime };

        // Assert
        Assert.Equal(expected, row.UptimeLevel);
    }

    [Theory]
    [InlineData(3, ChangeDirection.Up, 3)]
    [InlineData(-2, ChangeDirection.Down, 2)]
    [InlineData(0, ChangeDirection.Same, 0)]
    [InlineData(null, ChangeDirection.Same, 0)]
    public void ErrorChangeHasDirectionAndAmount(int? change, ChangeDirection direction, int amount)
    {
        // Act
        var row = new ServerRow { Name = "web-01", ErrorChange = change };

        // Assert
        Assert.Equal(direction, row.ErrorDirection);
        Assert.Equal(amount, row.ErrorChangeAmount);
    }

    [Fact]
    public void LatencyGivesNowMinimumAndMaximum()
    {
        // Act
        var row = new ServerRow { Name = "web-01", Latency = [70, 160, 90] };

        // Assert
        Assert.Equal(90d, row.LatencyNow);
        Assert.Equal(70d, row.LatencyMin);
        Assert.Equal(160d, row.LatencyMax);
    }

    [Fact]
    public void LatencyIsEmptyWithoutData()
    {
        // Act
        var row = new ServerRow { Name = "web-01" };

        // Assert
        Assert.Null(row.LatencyNow);
        Assert.Null(row.LatencyMin);
        Assert.Null(row.LatencyMax);
    }
}
