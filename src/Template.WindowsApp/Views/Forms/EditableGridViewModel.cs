namespace Template.WindowsApp.Views.Forms;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class EditableGridViewModel : AppViewModelBase, IShellCommandProvider, IConfirmRequestAsync
{
    private static readonly string[] LineSeparators = ["\r\n", "\n"];

    private static readonly OrderLineValues EmptyValues = new(string.Empty, string.Empty, null, null, 0, null, OrderStatus.Draft, string.Empty);

    private readonly TimeProvider timeProvider;

    private readonly IDialogService dialogService;

    private readonly IClipboardService clipboardService;

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

    public EditableGridViewModel(TimeProvider timeProvider, IDialogService dialogService, IClipboardService clipboardService)
    {
        this.timeProvider = timeProvider;
        this.dialogService = dialogService;
        this.clipboardService = clipboardService;
        synchronizationContext = SynchronizationContext.Current;

        saved = MakeLines(timeProvider.GetLocalNow().Date);
        Summary = string.Empty;

        AddCommand = MakeDelegateCommand(AddRow);
        DuplicateCommand = MakeDelegateCommand(Duplicate, () => SelectedLines().Count > 0);
        DeleteCommand = MakeDelegateCommand(Delete, () => SelectedLines().Any(static x => !x.IsLocked));
        RevertCommand = MakeAsyncCommand(RevertAsync, () => HasChanges);
        SaveCommand = MakeDelegateCommand(Save, () => HasChanges && (ErrorCount == 0));
        PasteCommand = MakeDelegateCommand<IReadOnlyList<string>>(Paste);

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

        var choice = await dialogService.AskSaveChangesAsync(LeaveMessage(), (ErrorCount == 0) && !Lines.Any(static x => x.IsEditing)).ConfigureAwait(true);
        if (choice == SaveChoice.Save)
        {
            Save();
        }
        return choice != SaveChoice.Cancel;
    }

    private static List<OrderLineValues> MakeLines(DateTime today) =>
    [
        new("AB-1001", "Laptop stand", 2, 45.00m, 0, today.AddDays(8), OrderStatus.Confirmed, string.Empty),
        new("AB-1002", "USB-C hub", 3, 29.90m, 10, today.AddDays(8), OrderStatus.Confirmed, string.Empty),
        new("CD-2001", "Monitor 27 inch", 1, 329.00m, 0, today.AddDays(11), OrderStatus.Draft, string.Empty),
        new("CD-2002", "Monitor arm", 1, 89.00m, 0, today.AddDays(11), OrderStatus.Draft, string.Empty),
        new("EF-3001", "Keyboard", 10, 49.00m, 25, today.AddDays(16), OrderStatus.Draft, "Volume discount"),
        new("EF-3002", "Mouse", 10, 25.00m, 15, today.AddDays(16), OrderStatus.Draft, string.Empty),
        new("GH-4001", "Headset", 3, 79.00m, 0, today.AddDays(-5), OrderStatus.Shipped, "Delivered"),
        new("GH-4002", "Webcam", 3, 59.00m, 0, today.AddDays(-5), OrderStatus.Shipped, string.Empty),
        new("JK-5001", "Docking station", 2, 189.00m, 10, today.AddDays(18), OrderStatus.Confirmed, string.Empty),
        new("JK-5002", "Cable set", 20, 4.50m, 0, today.AddDays(18), OrderStatus.Confirmed, string.Empty),
        new("LM-6001", "Desk lamp", 4, 35.00m, 0, today.AddDays(21), OrderStatus.Draft, string.Empty),
        new("LM-6002", "Chair mat", 2, 39.00m, 0, today.AddDays(21), OrderStatus.Draft, string.Empty)
    ];

    private static string Plural(int count, string one, string many) =>
        String.Create(CultureInfo.CurrentCulture, $"{count:N0} {(count == 1 ? one : many)}");

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
        if (await dialogService.ConfirmAsync("Discard all changes?", "All rows go back to how they were when you last saved.", "Discard", "Cancel").ConfigureAwait(true))
        {
            CancelRequest.Request();
            Load();
        }
    }

    private void Save()
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

        saved = Lines.Select(static x => x.Values).ToList();
        foreach (var line in Lines)
        {
            line.AcceptChanges();
        }
        notPasted = 0;
        UpdateSummary();
    }

    private void Paste(IReadOnlyList<string> keys)
    {
        if ((CurrentItem is not OrderLine start) || !clipboardService.ContainsText())
        {
            return;
        }

        var rows = clipboardService.GetText().Split(LineSeparators, StringSplitOptions.None);
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
        new("EditableGrid:Revert", "Revert", "Editable grid", "\uE7A7", RevertCommand),
        new("EditableGrid:Save", "Save", "Editable grid", "\uE74E", SaveCommand)
    ];
}
