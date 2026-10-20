namespace Template.WindowsApp.Behaviors;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DropBehavior : Behavior<FrameworkElement>
{
    private static readonly DependencyPropertyKey IsDragOverPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "IsDragOver",
        typeof(bool),
        typeof(DropBehavior),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsDragOverProperty = IsDragOverPropertyKey.DependencyProperty;

    [DependencyProperty]
    public partial ICommand? FilesCommand { get; set; }

    [DependencyProperty]
    public partial ICommand? TextCommand { get; set; }

    public static bool GetIsDragOver(DependencyObject obj) => (bool)obj.GetValue(IsDragOverProperty);

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AllowDrop = true;
        AssociatedObject.DragEnter += OnDragOver;
        AssociatedObject.DragOver += OnDragOver;
        AssociatedObject.DragLeave += OnDragLeave;
        AssociatedObject.Drop += OnDrop;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.DragEnter -= OnDragOver;
        AssociatedObject.DragOver -= OnDragOver;
        AssociatedObject.DragLeave -= OnDragLeave;
        AssociatedObject.Drop -= OnDrop;
        base.OnDetaching();
    }

    private static bool IsSupported(IDataObject data) =>
        data.GetDataPresent(DataFormats.FileDrop) || data.GetDataPresent(DataFormats.UnicodeText);

    private void OnDragOver(object sender, DragEventArgs e)
    {
        var supported = IsSupported(e.Data);
        e.Effects = supported ? DragDropEffects.Copy : DragDropEffects.None;
        AssociatedObject.SetValue(IsDragOverPropertyKey, supported);
        e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e) =>
        AssociatedObject.SetValue(IsDragOverPropertyKey, false);

    private void OnDrop(object sender, DragEventArgs e)
    {
        AssociatedObject.SetValue(IsDragOverPropertyKey, false);
        if ((e.Data.GetData(DataFormats.FileDrop) is string[] files) && (FilesCommand is { } filesCommand) && filesCommand.CanExecute(files))
        {
            filesCommand.Execute(files);
        }
        else if ((e.Data.GetData(DataFormats.UnicodeText) is string text) && (TextCommand is { } textCommand) && textCommand.CanExecute(text))
        {
            textCommand.Execute(text);
        }
        e.Handled = true;
    }
}
