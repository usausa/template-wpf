namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Markup;

using BunnyTail.XamlProperty;

public sealed partial class XamlPresenter : ContentControl
{
    private const string PresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly DependencyPropertyKey ErrorPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Error),
        typeof(string),
        typeof(XamlPresenter),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ErrorProperty = ErrorPropertyKey.DependencyProperty;

    [DependencyProperty(PropertyChanged = nameof(OnXamlChanged))]
    public partial string? Xaml { get; set; }

    public string? Error => (string?)GetValue(ErrorProperty);

    private void OnXamlChanged()
    {
        if (String.IsNullOrWhiteSpace(Xaml))
        {
            Content = null;
            SetValue(ErrorPropertyKey, null);
            return;
        }

        var context = new ParserContext();
        context.XmlnsDictionary.Add(string.Empty, PresentationNamespace);
        context.XmlnsDictionary.Add("x", XamlNamespace);
        try
        {
            Content = XamlReader.Parse(Xaml, context);
            SetValue(ErrorPropertyKey, null);
        }
        catch (XamlParseException ex)
        {
            SetValue(ErrorPropertyKey, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            SetValue(ErrorPropertyKey, ex.Message);
        }
    }
}
