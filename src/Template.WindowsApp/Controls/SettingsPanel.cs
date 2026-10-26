namespace Template.WindowsApp.Controls;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

public sealed partial class SettingsPanel : StackPanel
{
    private static readonly DependencyPropertyKey HasNoResultsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasNoResults),
        typeof(bool),
        typeof(SettingsPanel),
        new PropertyMetadata(false));

    public static readonly DependencyProperty HasNoResultsProperty = HasNoResultsPropertyKey.DependencyProperty;

    [DependencyProperty(PropertyChanged = nameof(OnStateChanged))]
    public partial object? SelectedPage { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnStateChanged))]
    public partial string? Query { get; set; }

    [AttachedProperty]
    public static partial object? GetPage(UIElement element);

    public static partial void SetPage(UIElement element, object? value);

    public bool HasNoResults => (bool)GetValue(HasNoResultsProperty);

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        Update();
    }

    private void OnStateChanged() => Update();

    private void Update()
    {
        if (!IsInitialized)
        {
            return;
        }

        var words = (Query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var total = 0;
        foreach (UIElement child in Children)
        {
            var count = Search(child, words);
            total += count;
            var visible = words.Length > 0 ? count > 0 : Equals(GetPage(child), SelectedPage);
            child.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        SetValue(HasNoResultsPropertyKey, (words.Length > 0) && (total == 0));
        BringIntoView(new Rect(0, 0, 1, 1));
    }

    private static int Search(DependencyObject parent, string[] words)
    {
        if (parent is SettingsCard card)
        {
            return card.Search(words) ? 1 : 0;
        }

        var count = 0;
        foreach (var child in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
        {
            count += Search(child, words);
        }

        return count;
    }
}
