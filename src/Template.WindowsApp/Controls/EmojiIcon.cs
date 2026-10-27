namespace Template.WindowsApp.Controls;

using System.Windows.Documents;

using BunnyTail.XamlProperty;

public sealed partial class EmojiIcon : FrameworkElement
{
    public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
        typeof(EmojiIcon),
        new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
        typeof(EmojiIcon),
        new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial string? Glyph { get; set; }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize) =>
        ColorEmoji.Find(Glyph) is { } shape ? new Size(shape.Width * FontSize, FontSize) : default;

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (ColorEmoji.Find(Glyph) is not { } shape)
        {
            return;
        }

        var scale = FontSize / ColorEmoji.EmSize;
        drawingContext.PushTransform(new MatrixTransform(scale, 0, 0, scale, shape.Offset.X * scale, shape.Offset.Y * scale));
        foreach (var layer in shape.Layers)
        {
            drawingContext.DrawGeometry(layer.Fill ?? Foreground, null, layer.Geometry);
        }
        drawingContext.Pop();
    }
}
