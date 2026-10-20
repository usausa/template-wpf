namespace Template.WindowsApp.Views.Design;

public sealed record IconEntry(string Name, string Code)
{
    public string Glyph => Char.ConvertFromUtf32(Int32.Parse(Code, NumberStyles.HexNumber, CultureInfo.InvariantCulture));

    public string Xaml => $"&#x{Code};";

    public string CSharp => $"\\u{Code}";
}
