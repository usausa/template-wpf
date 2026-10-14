namespace Template.WindowsApp.Markup;

using System.Windows.Markup;

using Template.WindowsApp.Views;

[MarkupExtensionReturnType(typeof(ViewId))]
public sealed class ViewIdExtension : MarkupExtension
{
    [ConstructorArgument("value")]
    public ViewId Value { get; set; }

    public ViewIdExtension(ViewId value)
    {
        Value = value;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => Value;
}
