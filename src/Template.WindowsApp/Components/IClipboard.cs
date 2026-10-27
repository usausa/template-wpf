namespace Template.WindowsApp.Components;

using System.Windows.Media.Imaging;

public interface IClipboard
{
    bool ContainsText();

    bool ContainsImage();

    bool ContainsFileDropList();

    bool ContainsAudio();

    IReadOnlyList<string> GetFormats();

    string GetText();

    BitmapSource? GetImage();

    void SetText(string text);

    void SetImage(BitmapSource image);

    void Clear();
}
