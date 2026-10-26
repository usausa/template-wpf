namespace Template.WindowsApp.Views.Fundamentals;

using System.Windows.Controls;

public sealed class TemperatureTemplateSelector : DataTemplateSelector
{
    public int HotThreshold { get; set; }

    public DataTemplate? NormalTemplate { get; set; }

    public DataTemplate? HotTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container) =>
        (item is int value) && (value >= HotThreshold) ? HotTemplate : NormalTemplate;
}
