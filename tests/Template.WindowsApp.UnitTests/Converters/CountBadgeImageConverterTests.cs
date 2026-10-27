namespace Template.WindowsApp.Converters;

using System.Globalization;
using System.Windows.Media;

public sealed class CountBadgeImageConverterTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ZeroOrLessShowsNothing(int count)
    {
        // Arrange
        var converter = new CountBadgeImageConverter();

        // Act
        var image = converter.Convert(count, typeof(ImageSource), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(image);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(150)]
    public void CountMakesSixteenPixelImage(int count)
    {
        // Arrange
        var converter = new CountBadgeImageConverter();

        // Act
        var image = converter.Convert(count, typeof(ImageSource), null, CultureInfo.InvariantCulture);

        // Assert
        var drawing = Assert.IsType<DrawingImage>(image);
        Assert.True(drawing.IsFrozen);
        Assert.Equal(16, drawing.Width);
        Assert.Equal(16, drawing.Height);
    }
}
