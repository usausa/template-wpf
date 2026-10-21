namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Documents;

using BunnyTail.XamlProperty;

public static partial class TextBlockHelper
{
    [AttachedProperty(PropertyChanged = nameof(OnRunsChanged))]
    public static partial IReadOnlyList<HighlightRun>? GetRuns(TextBlock textBlock);

    public static partial void SetRuns(TextBlock textBlock, IReadOnlyList<HighlightRun>? value);

    private static void OnRunsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock)
        {
            return;
        }

        textBlock.Inlines.Clear();
        if (e.NewValue is not IReadOnlyList<HighlightRun> runs)
        {
            return;
        }

        foreach (var run in runs)
        {
            textBlock.Inlines.Add(run.IsHighlighted ? new Run(run.Text) { FontWeight = FontWeights.SemiBold } : new Run(run.Text));
        }
    }
}
