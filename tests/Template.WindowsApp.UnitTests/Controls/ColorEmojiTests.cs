namespace Template.WindowsApp.Controls;

using System.Windows;

public sealed class ColorEmojiTests
{
    [Fact]
    public void EmojiHasColoredLayers()
    {
        // Act
        var shape = ColorEmoji.Find("\U0001F4E6");

        // Assert
        Assert.NotNull(shape);
        Assert.True(shape.Layers.Count > 1);
        Assert.All(shape.Layers, static x => Assert.NotNull(x.Fill));
        Assert.InRange(shape.Width, 0.9, 1.1);
    }

    [Fact]
    public void JoinedSequenceBecomesOneEmoji()
    {
        // Act
        var joined = ColorEmoji.Find("\U0001F468\u200D\U0001F469\u200D\U0001F467\u200D\U0001F466");
        var separate = ColorEmoji.Find("\U0001F468\U0001F469\U0001F467\U0001F466");

        // Assert
        Assert.NotNull(joined);
        Assert.NotNull(separate);
        Assert.True(joined.Width < separate.Width / 2);
        Assert.All(joined.Layers, static x => Assert.NotNull(x.Fill));
    }

    [Fact]
    public void ShapeIsCenteredInItsBox()
    {
        // Act
        var shape = ColorEmoji.Find("\u2328\uFE0F");

        // Assert
        Assert.NotNull(shape);
        var bounds = Rect.Empty;
        foreach (var layer in shape.Layers)
        {
            bounds.Union(layer.Geometry.Bounds);
        }
        bounds.Offset(shape.Offset);
        Assert.Equal(shape.Width * ColorEmoji.EmSize / 2, (bounds.Left + bounds.Right) / 2, 6);
        Assert.Equal(ColorEmoji.EmSize / 2, (bounds.Top + bounds.Bottom) / 2, 6);
    }

    [Fact]
    public void TextWithoutColorLayersUsesForeground()
    {
        // Act
        var shape = ColorEmoji.Find("A");

        // Assert
        Assert.NotNull(shape);
        Assert.All(shape.Layers, static x => Assert.Null(x.Fill));
    }

    [Fact]
    public void EmptyTextHasNoShape()
    {
        // Assert
        Assert.Null(ColorEmoji.Find(null));
        Assert.Null(ColorEmoji.Find(string.Empty));
    }

    [Fact]
    public void SameTextReturnsSameShape()
    {
        // Act
        var first = ColorEmoji.Find("\U0001F525");
        var second = ColorEmoji.Find("\U0001F525");

        // Assert
        Assert.Same(first, second);
    }
}
