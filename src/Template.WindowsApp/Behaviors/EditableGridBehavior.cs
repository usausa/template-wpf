namespace Template.WindowsApp.Behaviors;

using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Threading;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class EditableGridBehavior : Behavior<DataGrid>
{
    private readonly CommandBinding pasteBinding;

    private bool subscribed;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
    public partial IReadOnlyList<object>? SelectedRows { get; set; }

    [DependencyProperty]
    public partial ICommand? PasteCommand { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnCommitRequestChanged))]
    public partial IEventRequest<ParameterEventArgs>? CommitRequest { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnCancelRequestChanged))]
    public partial IEventRequest<ParameterEventArgs>? CancelRequest { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnEditRequestChanged))]
    public partial IEventRequest<ParameterEventArgs>? EditRequest { get; set; }

    [AttachedProperty]
    public static partial bool GetIsReadOnly(DataGridRow row);

    public static partial void SetIsReadOnly(DataGridRow row, bool value);

    public EditableGridBehavior()
    {
        pasteBinding = new CommandBinding(ApplicationCommands.Paste, OnPaste, OnCanPaste);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        var grid = AssociatedObject;
        grid.Loaded += OnLoaded;
        grid.Unloaded += OnUnloaded;
        grid.IsKeyboardFocusWithinChanged += OnIsKeyboardFocusWithinChanged;
        grid.BeginningEdit += OnBeginningEdit;
        grid.PreviewTextInput += OnPreviewTextInput;
        grid.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        grid.LoadingRow += OnLoadingRow;
        grid.UnloadingRow += OnUnloadingRow;
        grid.SelectedCellsChanged += OnSelectedCellsChanged;
        grid.CommandBindings.Add(pasteBinding);
        ((INotifyCollectionChanged)grid.Items).CollectionChanged += OnItemsChanged;
        if (grid.IsLoaded)
        {
            Subscribe();
        }
    }

    protected override void OnDetaching()
    {
        Unsubscribe();
        var grid = AssociatedObject;
        grid.Loaded -= OnLoaded;
        grid.Unloaded -= OnUnloaded;
        grid.IsKeyboardFocusWithinChanged -= OnIsKeyboardFocusWithinChanged;
        grid.BeginningEdit -= OnBeginningEdit;
        grid.PreviewTextInput -= OnPreviewTextInput;
        grid.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        grid.LoadingRow -= OnLoadingRow;
        grid.UnloadingRow -= OnUnloadingRow;
        grid.SelectedCellsChanged -= OnSelectedCellsChanged;
        grid.CommandBindings.Remove(pasteBinding);
        ((INotifyCollectionChanged)grid.Items).CollectionChanged -= OnItemsChanged;
        base.OnDetaching();
    }

    //--------------------------------------------------------------------------------
    // Edit
    //--------------------------------------------------------------------------------

    private void OnIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!AssociatedObject.IsKeyboardFocusWithin)
        {
            CommitRow();
        }
    }

    private static void OnBeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        if (GetIsReadOnly(e.Row))
        {
            e.Cancel = true;
        }
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if ((e.OriginalSource is not DataGridCell { IsEditing: false, Column: DataGridTextColumn } cell) ||
            String.IsNullOrEmpty(e.Text) ||
            e.Text.Any(Char.IsControl) ||
            !AssociatedObject.BeginEdit())
        {
            return;
        }

        e.Handled = true;
        if (cell.Content is TextBox textBox)
        {
            TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textBox, e.Text));
        }
    }

    //--------------------------------------------------------------------------------
    // Invalid row
    //--------------------------------------------------------------------------------

    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.KeyDown -= OnRowKeyDown;
        e.Row.KeyDown += OnRowKeyDown;
    }

    private void OnUnloadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.KeyDown -= OnRowKeyDown;
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if ((e.OriginalSource is DependencyObject source) &&
            (ItemsControl.ContainerFromElement(AssociatedObject, source) is DataGridRow row) &&
            (EditingItem() is { } item) &&
            !Equals(row.Item, item) &&
            !CommitRow())
        {
            e.Handled = true;
        }
    }

    private void OnRowKeyDown(object sender, KeyEventArgs e)
    {
        if (LeavesRow(e) && (EditingItem() is not null) && !CommitRow())
        {
            e.Handled = true;
        }
    }

    private object? EditingItem()
    {
        IEditableCollectionView view = AssociatedObject.Items;
        return view.IsAddingNew ? view.CurrentAddItem : view.IsEditingItem ? view.CurrentEditItem : null;
    }

    private bool LeavesRow(KeyEventArgs e)
    {
        var modifiers = e.KeyboardDevice.Modifiers;
        return e.Key switch
        {
            Key.Up or Key.Down or Key.PageUp or Key.PageDown => true,
            Key.Home or Key.End => modifiers.HasFlag(ModifierKeys.Control),
            Key.Tab => IsEdgeColumn(modifiers.HasFlag(ModifierKeys.Shift)),
            _ => false
        };
    }

    private bool IsEdgeColumn(bool backward)
    {
        if (AssociatedObject.CurrentColumn is not { } current)
        {
            return false;
        }

        var columns = VisibleColumns().ToList();
        return (columns.Count > 0) && (current == (backward ? columns[0] : columns[^1]));
    }

    //--------------------------------------------------------------------------------
    // Selected rows
    //--------------------------------------------------------------------------------

    private void OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        var items = AssociatedObject.Items;
        SelectedRows = AssociatedObject.SelectedCells
            .Select(static x => x.Item)
            .Distinct()
            .OrderBy(items.IndexOf)
            .ToList();
    }

    //--------------------------------------------------------------------------------
    // Paste
    //--------------------------------------------------------------------------------

    private void OnCanPaste(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = PasteCommand is not null;
        e.Handled = true;
    }

    private void OnPaste(object sender, ExecutedRoutedEventArgs e)
    {
        e.Handled = true;
        var grid = AssociatedObject;
        if (!CommitRow())
        {
            return;
        }

        var items = grid.Items;
        var start = grid.SelectedCells.Count > 0
            ? grid.SelectedCells.OrderBy(x => items.IndexOf(x.Item)).ThenBy(static x => x.Column.DisplayIndex).First()
            : grid.CurrentCell;
        if (start.Column is null)
        {
            return;
        }

        grid.CurrentCell = start;
        var keys = VisibleColumns()
            .Where(x => x.DisplayIndex >= start.Column.DisplayIndex)
            .Select(static x => x.SortMemberPath)
            .ToList();
        if (PasteCommand?.CanExecute(keys) == true)
        {
            PasteCommand.Execute(keys);
        }
    }

    //--------------------------------------------------------------------------------
    // Removed row
    //--------------------------------------------------------------------------------

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var grid = AssociatedObject;
        if ((e.Action != NotifyCollectionChangedAction.Remove) || !grid.IsKeyboardFocusWithin)
        {
            return;
        }

        var index = e.OldStartingIndex;
        var column = grid.CurrentColumn;
        grid.Dispatcher.InvokeAsync(() => RestoreFocus(index, column), DispatcherPriority.Background);
    }

    private void RestoreFocus(int index, DataGridColumn? column)
    {
        var grid = AssociatedObject;
        if ((grid is null) || (grid.Items.Count == 0) || (Keyboard.FocusedElement is not (null or Window)))
        {
            return;
        }

        if (((column ?? VisibleColumns().FirstOrDefault()) is not { } target) || (grid.Items[Math.Min(index, grid.Items.Count - 1)] is not { } item))
        {
            return;
        }

        grid.Focus();
        SelectCell(item, target);
    }

    //--------------------------------------------------------------------------------
    // Request
    //--------------------------------------------------------------------------------

    private void OnLoaded(object sender, RoutedEventArgs e) => Subscribe();

    private void OnUnloaded(object sender, RoutedEventArgs e) => Unsubscribe();

    private void OnCommitRequestChanged(IEventRequest<ParameterEventArgs>? oldValue, IEventRequest<ParameterEventArgs>? newValue) =>
        Resubscribe(oldValue, newValue, OnCommitRequested);

    private void OnCancelRequestChanged(IEventRequest<ParameterEventArgs>? oldValue, IEventRequest<ParameterEventArgs>? newValue) =>
        Resubscribe(oldValue, newValue, OnCancelRequested);

    private void OnEditRequestChanged(IEventRequest<ParameterEventArgs>? oldValue, IEventRequest<ParameterEventArgs>? newValue) =>
        Resubscribe(oldValue, newValue, OnEditRequested);

    private void Subscribe()
    {
        if (subscribed)
        {
            return;
        }

        subscribed = true;
        Resubscribe(null, CommitRequest, OnCommitRequested);
        Resubscribe(null, CancelRequest, OnCancelRequested);
        Resubscribe(null, EditRequest, OnEditRequested);
    }

    private void Unsubscribe()
    {
        if (!subscribed)
        {
            return;
        }

        Resubscribe(CommitRequest, null, OnCommitRequested);
        Resubscribe(CancelRequest, null, OnCancelRequested);
        Resubscribe(EditRequest, null, OnEditRequested);
        subscribed = false;
    }

    private void Resubscribe(IEventRequest<ParameterEventArgs>? oldValue, IEventRequest<ParameterEventArgs>? newValue, EventHandler<ParameterEventArgs> handler)
    {
        if (!subscribed)
        {
            return;
        }

        if (oldValue is not null)
        {
            oldValue.Requested -= handler;
        }
        if (newValue is not null)
        {
            newValue.Requested += handler;
        }
    }

    private void OnCommitRequested(object? sender, ParameterEventArgs e) => CommitRow();

    private void OnCancelRequested(object? sender, ParameterEventArgs e) => AssociatedObject.CancelEdit(DataGridEditingUnit.Row);

    private void OnEditRequested(object? sender, ParameterEventArgs e)
    {
        var grid = AssociatedObject;
        if ((e.Parameter is not { } item) || (VisibleColumns().FirstOrDefault(static x => !x.IsReadOnly) is not { } column))
        {
            return;
        }

        SelectCell(item, column);
        grid.Dispatcher.InvokeAsync(() =>
        {
            grid.Focus();
            grid.BeginEdit();
        }, DispatcherPriority.Background);
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private bool CommitRow() => AssociatedObject.CommitEdit(DataGridEditingUnit.Row, true);

    private IEnumerable<DataGridColumn> VisibleColumns() =>
        AssociatedObject.Columns
            .Where(static x => x.Visibility == Visibility.Visible)
            .OrderBy(static x => x.DisplayIndex);

    private void SelectCell(object item, DataGridColumn column)
    {
        var grid = AssociatedObject;
        var cell = new DataGridCellInfo(item, column);
        grid.ScrollIntoView(item, column);
        grid.UnselectAllCells();
        grid.CurrentCell = cell;
        grid.SelectedCells.Add(cell);
    }
}
