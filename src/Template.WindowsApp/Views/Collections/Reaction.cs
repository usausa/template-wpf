namespace Template.WindowsApp.Views.Collections;

public sealed partial class Reaction : ObservableObject
{
    public ReactionKind Kind { get; }

    [ObservableProperty]
    public partial int Count { get; set; }

    [ObservableProperty]
    public partial bool IsMine { get; set; }

    public Reaction(ReactionKind kind, int count, bool isMine)
    {
        Kind = kind;
        Count = count;
        IsMine = isMine;
    }

    public override string ToString() => Kind.ToString();
}
