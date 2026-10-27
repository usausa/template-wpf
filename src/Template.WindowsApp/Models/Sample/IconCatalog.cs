namespace Template.WindowsApp.Models.Sample;

using System.Text.Json;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record IconEntry(string Name, string Code)
{
    public string Glyph => Char.ConvertFromUtf32(Int32.Parse(Code, NumberStyles.HexNumber, CultureInfo.InvariantCulture));

    public string Xaml => $"&#x{Code};";

    public string CSharp => $"\\u{Code}";
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class IconSample
{
    private const string ResourceName = "Template.WindowsApp.Models.Sample.Icons.json";

    public static IconEntry[] LoadIcons()
    {
        using var stream = typeof(IconSample).Assembly.GetManifestResourceStream(ResourceName)!;
        return JsonSerializer.Deserialize<IconEntry[]>(stream) ?? [];
    }
}
