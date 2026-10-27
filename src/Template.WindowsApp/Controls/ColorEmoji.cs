namespace Template.WindowsApp.Controls;

using System.IO;
using System.Text;

public static class ColorEmoji
{
    public const double EmSize = 100;

    private const string FamilyName = "Segoe UI Emoji";

    private const int ForegroundIndex = 0xFFFF;

    private static readonly Dictionary<string, EmojiShape?> Shapes = [];

    private static readonly Lazy<Font?> EmojiFont = new(Font.Load);

    public static EmojiShape? Find(string? text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return null;
        }

        if (!Shapes.TryGetValue(text, out var shape))
        {
            shape = Build(text);
            Shapes[text] = shape;
        }
        return shape;
    }

    private static EmojiShape? Build(string text)
    {
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface(FamilyName), EmSize, Brushes.Black, 1);
        var drawing = new DrawingGroup();
        using (var context = drawing.Open())
        {
            context.DrawText(formatted, new Point(0, 0));
        }

        var layers = new List<EmojiLayer>();
        Collect(drawing, EmojiFont.Value, layers);
        if (layers.Count == 0)
        {
            return null;
        }

        var bounds = Rect.Empty;
        foreach (var layer in layers)
        {
            bounds.Union(layer.Geometry.Bounds);
        }
        var width = Math.Max(1, bounds.Width / EmSize);
        return new EmojiShape(layers, width, new Vector(((width * EmSize) - bounds.Left - bounds.Right) / 2, (EmSize - bounds.Top - bounds.Bottom) / 2));
    }

    private static void Collect(Drawing drawing, Font? font, List<EmojiLayer> layers)
    {
        if (drawing is DrawingGroup group)
        {
            foreach (var child in group.Children)
            {
                Collect(child, font, layers);
            }
            return;
        }

        if (drawing is not GlyphRunDrawing { GlyphRun: { } run })
        {
            return;
        }

        var typeface = run.GlyphTypeface;
        var colored = (font is not null) && (typeface.FontUri == font.Uri);
        var x = run.BaselineOrigin.X;
        var y = typeface.Baseline * run.FontRenderingEmSize;
        for (var i = 0; i < run.GlyphIndices.Count; i++)
        {
            var glyph = run.GlyphIndices[i];
            if (colored && font!.BaseGlyphs.TryGetValue(glyph, out var record))
            {
                for (var layer = record.First; layer < record.First + record.Count; layer++)
                {
                    var (layerGlyph, palette) = font.Layers[layer];
                    layers.Add(new EmojiLayer(Outline(typeface, layerGlyph, run.FontRenderingEmSize, x, y), palette == ForegroundIndex ? null : font.Brushes[palette]));
                }
            }
            else
            {
                layers.Add(new EmojiLayer(Outline(typeface, glyph, run.FontRenderingEmSize, x, y), null));
            }
            x += run.AdvanceWidths[i];
        }
    }

    private static Geometry Outline(GlyphTypeface typeface, ushort glyph, double size, double x, double y)
    {
        var geometry = typeface.GetGlyphOutline(glyph, size, size).Clone();
        geometry.Transform = new TranslateTransform(x, y);
        geometry.Freeze();
        return geometry;
    }

    private sealed record Font(Uri Uri, Dictionary<ushort, (int First, int Count)> BaseGlyphs, (ushort Glyph, int Palette)[] Layers, Brush[] Brushes)
    {
        public static Font? Load()
        {
            if (!new Typeface(FamilyName).TryGetGlyphTypeface(out var typeface))
            {
                return null;
            }

            using var stream = typeface.GetFontStream();
            if (stream is null)
            {
                return null;
            }

            var tables = ReadTables(stream);
            if (!tables.TryGetValue("COLR", out var colr) || !tables.TryGetValue("CPAL", out var cpal))
            {
                return null;
            }

            var colrData = ReadBytes(stream, colr.Offset, colr.Length);
            var baseCount = ReadUInt16(colrData, 2);
            var baseOffset = (int)ReadUInt32(colrData, 4);
            var layerOffset = (int)ReadUInt32(colrData, 8);
            var layerCount = ReadUInt16(colrData, 12);
            var baseGlyphs = new Dictionary<ushort, (int, int)>(baseCount);
            for (var i = 0; i < baseCount; i++)
            {
                var offset = baseOffset + (i * 6);
                baseGlyphs[(ushort)ReadUInt16(colrData, offset)] = (ReadUInt16(colrData, offset + 2), ReadUInt16(colrData, offset + 4));
            }

            var layers = new (ushort, int)[layerCount];
            for (var i = 0; i < layerCount; i++)
            {
                var offset = layerOffset + (i * 4);
                layers[i] = ((ushort)ReadUInt16(colrData, offset), ReadUInt16(colrData, offset + 2));
            }

            var cpalData = ReadBytes(stream, cpal.Offset, cpal.Length);
            var entryCount = ReadUInt16(cpalData, 2);
            var colorOffset = (int)ReadUInt32(cpalData, 8);
            var firstColor = ReadUInt16(cpalData, 12);
            var brushes = new Brush[entryCount];
            for (var i = 0; i < entryCount; i++)
            {
                var offset = colorOffset + ((firstColor + i) * 4);
                var brush = new SolidColorBrush(Color.FromArgb(cpalData[offset + 3], cpalData[offset + 2], cpalData[offset + 1], cpalData[offset]));
                brush.Freeze();
                brushes[i] = brush;
            }

            return new Font(typeface.FontUri, baseGlyphs, layers, brushes);
        }

        private static Dictionary<string, (int Offset, int Length)> ReadTables(Stream stream)
        {
            var count = ReadUInt16(ReadBytes(stream, 0, 12), 4);
            var records = ReadBytes(stream, 12, count * 16);
            var tables = new Dictionary<string, (int, int)>(count);
            for (var i = 0; i < count; i++)
            {
                var offset = i * 16;
                tables[Encoding.ASCII.GetString(records, offset, 4)] = ((int)ReadUInt32(records, offset + 8), (int)ReadUInt32(records, offset + 12));
            }
            return tables;
        }

        private static byte[] ReadBytes(Stream stream, int offset, int length)
        {
            var buffer = new byte[length];
            stream.Seek(offset, SeekOrigin.Begin);
            stream.ReadExactly(buffer);
            return buffer;
        }

        private static int ReadUInt16(byte[] data, int offset) => (data[offset] << 8) | data[offset + 1];

        private static uint ReadUInt32(byte[] data, int offset) =>
            ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
    }
}
