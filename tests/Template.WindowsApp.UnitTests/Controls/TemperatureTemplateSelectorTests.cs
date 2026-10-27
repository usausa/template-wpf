namespace Template.WindowsApp.Controls;

using System.Windows;

public sealed class TemperatureTemplateSelectorTests
{
    [Theory]
    [InlineData(29, false)]
    [InlineData(30, true)]
    [InlineData(35, true)]
    public void HotTemplateFromTheThreshold(int temperature, bool hot)
    {
        // Arrange
        var normal = new DataTemplate();
        var hotTemplate = new DataTemplate();
        var selector = new TemperatureTemplateSelector
        {
            HotThreshold = 30,
            NormalTemplate = normal,
            HotTemplate = hotTemplate
        };

        // Act
        var template = selector.SelectTemplate(temperature, new DependencyObject());

        // Assert
        Assert.Same(hot ? hotTemplate : normal, template);
    }
}
