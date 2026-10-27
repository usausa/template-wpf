namespace Template.WindowsApp.Views.Forms;

using Smart.Mapper;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models;
using Template.WindowsApp.Models.Entity;
using Template.WindowsApp.Models.Sample;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

//--------------------------------------------------------------------------------
// Mapper
//--------------------------------------------------------------------------------

public static partial class EditableGridViewModelMapper
{
    [Mapper]
    public static partial OrderLineValues ToValues(this OrderLineEntity source);

    [Mapper]
    public static partial OrderLineEntity ToEntity(this OrderLineValues source);
}

//--------------------------------------------------------------------------------
// ViewModel
//--------------------------------------------------------------------------------

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class EditableGridViewModel : AppViewModelBase, IShellCommandProvider, IConfirmRequestAsync
{
    private static readonly string[] LineSeparators = ["\r\n", "\n"];

    private static readonly OrderLineValues EmptyValues = new(string.Empty, string.Empty, null, null, 0, null, OrderStatus.Draft, string.Empty);

    private readonly TimeProvider timeProvider;

    private readonly IDialog dialog;

    private readonly IClipboard clipboard;

    private readonly NotificationCenter notificationCenter;

    private readonly DataService dataService;

    private readonly SynchronizationContext? synchronizationContext;

    private List<OrderLineValues> saved;

    private int added;

    private int changed;

    private int deleted;

    private int notPasted;

    public static IReadOnlyList<OrderStatus> Statuses { get; } = [OrderStatus.Draft, OrderStatus.Confirmed, OrderStatus.Shipped];

    public ObservableCollection<OrderLine> Lines { get; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<object>? SelectedRows { get; set; }

    [ObservableProperty]
    public partial object? CurrentItem { get; set; }

    [ObservableProperty]
    public partial string Summary { get; set; }

    [ObservableProperty]
    public partial bool HasChanges { get; set; }

    [ObservableProperty]
    public partial int ErrorCount { get; set; }

    public EventRequest CommitRequest { get; } = new();

    public EventRequest CancelRequest { get; } = new();

    public EventRequest<OrderLine> EditRequest { get; } = new();

    public IObserveCommand AddCommand { get; }

    public IObserveCommand DuplicateCommand { get; }

    public IObserveCommand DeleteCommand { get; }

    public IObserveCommand RevertCommand { get; }

    public IObserveCommand SaveCommand { get; }

    public IObserveCommand PasteCommand { get; }

    public IObserveCommand ChangeCommand { get; }

    public IObserveCommand ExportCommand { get; }

    public EditableGridViewModel(TimeProvider timeProvider, IDialog dialog, IClipboard clipboard, NotificationCenter notificationCenter, DataService dataService)
    {
        this.timeProvider = timeProvider;
        this.dialog = dialog;
        this.clipboard = clipboard;
        this.notificationCenter = notificationCenter;
        this.dataService = dataService;
        synchronizationContext = SynchronizationContext.Current;

        saved = LoadSaved();
        Summary = string.Empty;

        AddCommand = MakeDelegateCommand(AddRow);
        DuplicateCommand = MakeDelegateCommand(Duplicate, () => SelectedLines().Count > 0);
        DeleteCommand = MakeDelegateCommand(Delete, () => SelectedLines().Any(static x => !x.IsLocked));
        RevertCommand = MakeAsyncCommand(RevertAsync, () => HasChanges);
        SaveCommand = MakeAsyncCommand(SaveAsync, () => HasChanges && (ErrorCount == 0));
        PasteCommand = MakeDelegateCommand<IReadOnlyList<string>>(Paste);
        ChangeCommand = MakeAsyncCommand(ChangeAsync, () => SelectedLines().Any(static x => !x.IsLocked));
        ExportCommand = MakeDelegateCommand(Export);

        Load();
    }

    public async Task<bool> CanNavigateAsync(INavigationContext context)
    {
        CommitRequest.Request();
        foreach (var line in Lines)
        {
            line.RevealErrors();
        }
        if (!HasChanges)
        {
            return true;
        }

        var choice = await dialog.AskSaveChangesAsync(LeaveMessage(), (ErrorCount == 0) && !Lines.Any(static x => x.IsEditing)).ConfigureAwait(true);
        if (choice == SaveChoice.Save)
        {
            await SaveAsync().ConfigureAwait(true);
        }
        return choice != SaveChoice.Cancel;
    }

    private static string Plural(int count, string one, string many) =>
        String.Create(CultureInfo.CurrentCulture, $"{count:N0} {(count == 1 ? one : many)}");

    private List<OrderLineValues> LoadSaved() =>
        [.. dataService.QueryOrderLines().Select(static x => x.ToValues())];

    private void Load()
    {
        foreach (var line in Lines.ToList())
        {
            Remove(line);
        }
        foreach (var values in saved)
        {
            Insert(Lines.Count, OrderLine.Load(values, timeProvider));
        }

        notPasted = 0;
        UpdateSummary();
    }

    private void Insert(int index, OrderLine line)
    {
        line.PropertyChanged += OnLinePropertyChanged;
        line.Discarded += OnLineDiscarded;
        Lines.Insert(index, line);
    }

    private void Remove(OrderLine line)
    {
        line.PropertyChanged -= OnLinePropertyChanged;
        line.Discarded -= OnLineDiscarded;
        Lines.Remove(line);
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OrderLine.Revision))
        {
            UpdateSummary();
        }
    }

    private void OnLineDiscarded(object? sender, EventArgs e)
    {
        if (sender is not OrderLine line)
        {
            return;
        }

        if (synchronizationContext is null)
        {
            Discard(line);
        }
        else
        {
            synchronizationContext.Post(_ => Discard(line), null);
        }
    }

    private void Discard(OrderLine line)
    {
        Remove(line);
        UpdateSummary();
    }

    private List<OrderLine> SelectedLines() =>
        SelectedRows?.OfType<OrderLine>().Where(Lines.Contains).ToList() ?? [];

    private bool CommitEdits()
    {
        CommitRequest.Request();
        return !Lines.Any(static x => x.IsEditing);
    }

    private void AddRow()
    {
        if (!CommitEdits())
        {
            return;
        }

        var selected = SelectedLines();
        var line = OrderLine.Create(EmptyValues, timeProvider);
        Insert(selected.Count > 0 ? selected.Max(Lines.IndexOf) + 1 : Lines.Count, line);
        UpdateSummary();
        EditRequest.Request(line);
    }

    private void Duplicate()
    {
        var lines = SelectedLines();
        if (!CommitEdits() || (lines.Count == 0))
        {
            return;
        }

        var index = lines.Max(Lines.IndexOf) + 1;
        foreach (var values in lines.Select(static x => x.Values))
        {
            var copy = OrderLine.Create(values.Status == OrderStatus.Shipped ? values with { Status = OrderStatus.Draft } : values, timeProvider);
            Insert(index++, copy);
            copy.RevealErrors();
        }
        UpdateSummary();
    }

    private void Delete()
    {
        var lines = SelectedLines();
        if (!CommitEdits())
        {
            return;
        }

        foreach (var line in lines.Where(static x => !x.IsLocked))
        {
            Remove(line);
        }
        UpdateSummary();
    }

    private async Task RevertAsync()
    {
        if (await dialog.ConfirmAsync("Discard all changes?", "All rows go back to how they were when you last saved.", "Discard", "Cancel").ConfigureAwait(true))
        {
            CancelRequest.Request();
            saved = LoadSaved();
            Load();
        }
    }

    private async Task SaveAsync()
    {
        if (!CommitEdits())
        {
            return;
        }

        foreach (var line in Lines)
        {
            line.RevealErrors();
        }
        if (ErrorCount > 0)
        {
            return;
        }

        var values = Lines.Select(static x => x.Values).ToList();
        await dataService.ReplaceOrderLinesAsync(values.Select(static (x, i) =>
        {
            var entity = x.ToEntity();
            entity.LineNo = i + 1;
            return entity;
        })).ConfigureAwait(true);
        saved = values;
        foreach (var line in Lines)
        {
            line.AcceptChanges();
        }
        notPasted = 0;
        UpdateSummary();
    }

    private async Task ChangeAsync()
    {
        var lines = SelectedLines().Where(static x => !x.IsLocked).ToList();
        if (!CommitEdits() || (lines.Count == 0))
        {
            return;
        }

        using var change = new BulkChangeDialogViewModel
        {
            Title = $"Change {Plural(lines.Count, "row", "rows")}"
        };
        if (await dialog.ShowAsync(change).ConfigureAwait(true))
        {
            ApplyChange(lines, change.Status, change.Delivery, (int?)change.Discount);
        }
    }

    private void ApplyChange(List<OrderLine> lines, OrderStatus? status, DateTime? delivery, int? discount)
    {
        foreach (var line in lines)
        {
            if (status is { } newStatus)
            {
                line.Status = newStatus;
            }
            if (delivery is { } newDelivery)
            {
                line.Delivery = newDelivery.Date;
            }
            if (discount is { } newDiscount)
            {
                line.Discount = newDiscount;
            }
        }
        UpdateSummary();
    }

    private void Export()
    {
        if (!CommitEdits())
        {
            return;
        }

        var path = dialog.SaveFile("order-lines.csv", "CSV files (*.csv)|*.csv|All files (*.*)|*.*");
        if (path is null)
        {
            return;
        }

        try
        {
            File.WriteAllText(path, OrderLineCsv.Format(Lines.Select(static x => x.Values)), new UTF8Encoding(true));
            notificationCenter.Show(InfoBarSeverity.Success, "Exported", $"{Plural(Lines.Count, "row", "rows")} to {Path.GetFileName(path)}");
        }
        catch (IOException ex)
        {
            notificationCenter.Show(InfoBarSeverity.Error, "Export failed", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            notificationCenter.Show(InfoBarSeverity.Error, "Export failed", ex.Message);
        }
    }

    private void Paste(IReadOnlyList<string> keys)
    {
        if ((CurrentItem is not OrderLine start) || !clipboard.ContainsText())
        {
            return;
        }

        var rows = clipboard.GetText().Split(LineSeparators, StringSplitOptions.None);
        var count = (rows.Length > 0) && (rows[^1].Length == 0) ? rows.Length - 1 : rows.Length;
        var index = Lines.IndexOf(start);
        var rejected = 0;
        for (var i = 0; i < count; i++, index++)
        {
            if (index == Lines.Count)
            {
                Insert(index, OrderLine.Create(EmptyValues, timeProvider));
            }

            var line = Lines[index];
            if (line.IsLocked)
            {
                continue;
            }

            var cells = rows[i].Split('\t');
            for (var j = 0; (j < cells.Length) && (j < keys.Count); j++)
            {
                if ((keys[j] != nameof(OrderLine.Amount)) && !line.TryPaste(keys[j], cells[j]))
                {
                    rejected++;
                }
            }
            if (line.IsNew)
            {
                line.RevealErrors();
            }
        }

        notPasted = rejected;
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        added = Lines.Count(static x => x.IsNew);
        changed = Lines.Count(static x => !x.IsNew && x.IsChanged);
        deleted = saved.Count - (Lines.Count - added);
        ErrorCount = Lines.Count(static x => x.HasErrors);
        HasChanges = (added + changed + deleted) > 0;

        var parts = new List<string>();
        if (added > 0)
        {
            parts.Add(Plural(added, "added", "added"));
        }
        if (changed > 0)
        {
            parts.Add(Plural(changed, "changed", "changed"));
        }
        if (deleted > 0)
        {
            parts.Add(Plural(deleted, "deleted", "deleted"));
        }
        if (ErrorCount > 0)
        {
            parts.Add(Plural(ErrorCount, "error", "errors"));
        }
        if (notPasted > 0)
        {
            parts.Add(Plural(notPasted, "cell not pasted", "cells not pasted"));
        }
        Summary = parts.Count > 0 ? String.Join(" · ", parts) : "No changes";
    }

    private string LeaveMessage()
    {
        var parts = new List<string>();
        if (added > 0)
        {
            parts.Add(Plural(added, "added", "added"));
        }
        if (changed > 0)
        {
            parts.Add(Plural(changed, "changed", "changed"));
        }
        if (deleted > 0)
        {
            parts.Add(Plural(deleted, "deleted", "deleted"));
        }

        var list = parts.Count > 1 ? String.Join(", ", parts.Take(parts.Count - 1)) + " and " + parts[^1] : parts[0];
        var single = (added + changed + deleted) == 1;
        var rows = single ? "row" : "rows";
        var them = single ? "it" : "them";
        return $"You have {list} {rows} in Editable grid. Do you want to save {them} before you leave?";
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("EditableGrid:AddRow", "Add row", "Editable grid", "\uE710", AddCommand),
        new("EditableGrid:Duplicate", "Duplicate", "Editable grid", "\uE8C8", DuplicateCommand),
        new("EditableGrid:Delete", "Delete", "Editable grid", "\uE74D", DeleteCommand),
        new("EditableGrid:Change", "Change selected rows", "Editable grid", "\uE70F", ChangeCommand),
        new("EditableGrid:Revert", "Revert", "Editable grid", "\uE7A7", RevertCommand),
        new("EditableGrid:Save", "Save", "Editable grid", "\uE74E", SaveCommand),
        new("EditableGrid:Export", "Export to CSV", "Editable grid", "\uEDE1", ExportCommand)
    ];
}
