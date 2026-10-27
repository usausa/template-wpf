namespace Template.WindowsApp.Controls;

public sealed record EmojiShape(IReadOnlyList<EmojiLayer> Layers, double Width, Vector Offset);
