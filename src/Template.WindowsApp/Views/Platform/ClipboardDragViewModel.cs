namespace Template.WindowsApp.Views.Platform;

using System.Windows.Media.Imaging;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ClipboardDragViewModel : AppViewModelBase, IShellCommandProvider
{
    private const int ImageWidth = 240;

    private const int ImageHeight = 120;

    private const int ImageStride = ImageWidth * 4;

    private static readonly Color[] BandColors =
    [
        Color.FromRgb(0xC0, 0xD8, 0xFF),
        Color.FromRgb(0x8A, 0xB8, 0xFF),
        Color.FromRgb(0x57, 0x94, 0xF2),
        Color.FromRgb(0x32, 0x74, 0xD9),
        Color.FromRgb(0x1F, 0x60, 0xC4)
    ];

    private static readonly string[] SizeUnits = ["bytes", "KB", "MB", "GB"];

    private readonly IClipboard clipboard;

    public BitmapSource SampleImage { get; } = MakeSampleImage();

    public string DragText { get; } = "Hello, drag and drop!";

    [ObservableProperty]
    public partial string Text { get; set; }

    [ObservableProperty]
    public partial string? TextResult { get; set; }

    [ObservableProperty]
    public partial BitmapSource? PastedImage { get; set; }

    [ObservableProperty]
    public partial string? PastedSize { get; set; }

    [ObservableProperty]
    public partial bool HasText { get; set; }

    [ObservableProperty]
    public partial bool HasImage { get; set; }

    [ObservableProperty]
    public partial bool HasFiles { get; set; }

    [ObservableProperty]
    public partial bool HasAudio { get; set; }

    [ObservableProperty]
    public partial string Formats { get; set; }

    public ObservableCollection<DroppedItem> DroppedItems { get; } = [];

    [ObservableProperty]
    public partial string? DropResult { get; set; }

    public IObserveCommand CopyTextCommand { get; }

    public IObserveCommand PasteTextCommand { get; }

    public IObserveCommand CopyImageCommand { get; }

    public IObserveCommand PasteImageCommand { get; }

    public IObserveCommand RefreshCommand { get; }

    public IObserveCommand ClearCommand { get; }

    public IObserveCommand DropFilesCommand { get; }

    public IObserveCommand DropTextCommand { get; }

    public ClipboardDragViewModel(IClipboard clipboard)
    {
        this.clipboard = clipboard;

        Text = "Hello, clipboard!";
        Formats = string.Empty;
        Refresh();

        CopyTextCommand = MakeDelegateCommand(CopyText, () => Text.Length > 0);
        PasteTextCommand = MakeDelegateCommand(PasteText);
        CopyImageCommand = MakeDelegateCommand(CopyImage);
        PasteImageCommand = MakeDelegateCommand(PasteImage);
        RefreshCommand = MakeDelegateCommand(Refresh);
        ClearCommand = MakeDelegateCommand(Clear);
        DropFilesCommand = MakeDelegateCommand<string[]>(DropFiles);
        DropTextCommand = MakeDelegateCommand<string>(DropText);
    }

    private static BitmapSource MakeSampleImage()
    {
        var pixels = new byte[ImageStride * ImageHeight];
        for (var y = 0; y < ImageHeight; y++)
        {
            for (var x = 0; x < ImageWidth; x++)
            {
                var color = BandColors[x * BandColors.Length / ImageWidth];
                var offset = (y * ImageStride) + (x * 4);
                pixels[offset] = color.B;
                pixels[offset + 1] = color.G;
                pixels[offset + 2] = color.R;
                pixels[offset + 3] = color.A;
            }
        }

        var image = BitmapSource.Create(ImageWidth, ImageHeight, 96, 96, PixelFormats.Bgra32, null, pixels, ImageStride);
        image.Freeze();
        return image;
    }

    private static string CountCharacters(string text) =>
        text.Length == 1 ? "1 character" : $"{text.Length} characters";

    private static string DescribeFile(string path)
    {
        if (Directory.Exists(path))
        {
            return "Folder";
        }
        return File.Exists(path) ? FormatSize(new FileInfo(path).Length) : "-";
    }

    private static string FormatSize(long length)
    {
        var size = (double)length;
        var unit = 0;
        while ((size >= 1024) && (unit < SizeUnits.Length - 1))
        {
            size /= 1024;
            unit++;
        }
        return String.Create(CultureInfo.CurrentCulture, $"{size:0.#} {SizeUnits[unit]}");
    }

    private void CopyText()
    {
        clipboard.SetText(Text);
        TextResult = $"Copied {CountCharacters(Text)}";
        Refresh();
    }

    private void PasteText()
    {
        if (clipboard.ContainsText())
        {
            Text = clipboard.GetText();
            TextResult = $"Pasted {CountCharacters(Text)}";
        }
        else
        {
            TextResult = "No text to paste";
        }
    }

    private void CopyImage()
    {
        clipboard.SetImage(SampleImage);
        Refresh();
    }

    private void PasteImage()
    {
        PastedImage = clipboard.ContainsImage() ? clipboard.GetImage() : null;
        PastedSize = PastedImage is { } image ? $"{image.PixelWidth} × {image.PixelHeight}" : "none";
    }

    private void Refresh()
    {
        HasText = clipboard.ContainsText();
        HasImage = clipboard.ContainsImage();
        HasFiles = clipboard.ContainsFileDropList();
        HasAudio = clipboard.ContainsAudio();
        var formats = clipboard.GetFormats();
        Formats = formats.Count > 0 ? String.Join(", ", formats) : "-";
    }

    private void Clear()
    {
        clipboard.Clear();
        Refresh();
    }

    private void DropFiles(string[] files)
    {
        DroppedItems.Clear();
        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            DroppedItems.Add(new DroppedItem(name.Length > 0 ? name : file, DescribeFile(file)));
        }
        DropResult = files.Length == 1 ? "1 file" : $"{files.Length} files";
    }

    private void DropText(string text)
    {
        DroppedItems.Clear();
        DroppedItems.Add(new DroppedItem(text.ReplaceLineEndings(" ").Trim(), "Text"));
        DropResult = "text";
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("ClipboardDrag:Refresh", "Refresh formats", "Clipboard & drag", "\uE72C", RefreshCommand),
        new("ClipboardDrag:Clear", "Clear clipboard", "Clipboard & drag", "\uE894", ClearCommand)
    ];
}
