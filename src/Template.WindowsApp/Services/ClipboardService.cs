namespace Template.WindowsApp.Services;

using System.Windows.Media.Imaging;

public sealed class ClipboardService : IClipboardService
{
    public bool ContainsText() => Clipboard.ContainsText();

    public bool ContainsImage() => Clipboard.ContainsImage();

    public bool ContainsFileDropList() => Clipboard.ContainsFileDropList();

    public bool ContainsAudio() => Clipboard.ContainsAudio();

    public IReadOnlyList<string> GetFormats() => Clipboard.GetDataObject()?.GetFormats() ?? [];

    public string GetText() => Clipboard.GetText();

    public BitmapSource? GetImage() => Clipboard.GetImage();

    public void SetText(string text) => Clipboard.SetText(text);

    public void SetImage(BitmapSource image) => Clipboard.SetImage(image);

    public void Clear() => Clipboard.Clear();
}
