namespace Template.WindowsApp.Controls;

using System.Windows.Automation.Peers;
using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Template.WindowsApp.Models;

public sealed partial class SettingsCard : ContentControl
{
    private const CompareOptions MatchOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

    private static readonly CompareInfo Compare = CultureInfo.InvariantCulture.CompareInfo;

    private static readonly DependencyPropertyKey HeaderRunsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HeaderRuns),
        typeof(IReadOnlyList<HighlightRun>),
        typeof(SettingsCard),
        new PropertyMetadata(Array.Empty<HighlightRun>()));

    private static readonly DependencyPropertyKey DescriptionRunsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(DescriptionRuns),
        typeof(IReadOnlyList<HighlightRun>),
        typeof(SettingsCard),
        new PropertyMetadata(Array.Empty<HighlightRun>()));

    private static readonly DependencyPropertyKey IsMatchPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsMatch),
        typeof(bool),
        typeof(SettingsCard),
        new PropertyMetadata(true));

    public static readonly DependencyProperty HeaderRunsProperty = HeaderRunsPropertyKey.DependencyProperty;

    public static readonly DependencyProperty DescriptionRunsProperty = DescriptionRunsPropertyKey.DependencyProperty;

    public static readonly DependencyProperty IsMatchProperty = IsMatchPropertyKey.DependencyProperty;

    private string[] words = [];

    [DependencyProperty(PropertyChanged = nameof(OnTextChanged))]
    public partial string? Header { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnTextChanged))]
    public partial string? Description { get; set; }

    [DependencyProperty]
    public partial string? HeaderIcon { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnTextChanged))]
    public partial string? Keywords { get; set; }

    [DependencyProperty]
    public partial SettingsCardContentAlignment ContentAlignment { get; set; }

    public IReadOnlyList<HighlightRun> HeaderRuns => (IReadOnlyList<HighlightRun>)GetValue(HeaderRunsProperty);

    public IReadOnlyList<HighlightRun> DescriptionRuns => (IReadOnlyList<HighlightRun>)GetValue(DescriptionRunsProperty);

    public bool IsMatch => (bool)GetValue(IsMatchProperty);

    static SettingsCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingsCard), new FrameworkPropertyMetadata(typeof(SettingsCard)));
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new SettingsCardAutomationPeer(this);

    internal bool Search(string[] values)
    {
        words = values;
        Update();
        return IsMatch;
    }

    private void OnTextChanged() => Update();

    private void Update()
    {
        var header = Header ?? string.Empty;
        var description = Description ?? string.Empty;
        var keywords = Keywords ?? string.Empty;
        var headerMarks = new bool[header.Length];
        var descriptionMarks = new bool[description.Length];
        var match = true;
        foreach (var word in words)
        {
            var inHeader = Mark(header, word, headerMarks);
            var inDescription = Mark(description, word, descriptionMarks);
            if (!inHeader && !inDescription && (Compare.IndexOf(keywords, word, MatchOptions) < 0))
            {
                match = false;
            }
        }

        SetValue(IsMatchPropertyKey, match);
        SetValue(HeaderRunsPropertyKey, HighlightRun.Split(header, headerMarks));
        SetValue(DescriptionRunsPropertyKey, HighlightRun.Split(description, descriptionMarks));
    }

    private static bool Mark(string text, string word, bool[] marks)
    {
        var found = false;
        var start = 0;
        while (start < text.Length)
        {
            var index = Compare.IndexOf(text.AsSpan(start), word.AsSpan(), MatchOptions, out var length);
            if (index < 0)
            {
                break;
            }

            marks.AsSpan(start + index, length).Fill(true);
            found = true;
            start += index + Math.Max(length, 1);
        }

        return found;
    }
}
