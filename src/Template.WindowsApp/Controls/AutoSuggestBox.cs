namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

[TemplatePart(Name = TextBoxPartName, Type = typeof(TextBox))]
[TemplatePart(Name = SuggestionsListPartName, Type = typeof(ListBox))]
[TemplatePart(Name = QueryButtonPartName, Type = typeof(ButtonBase))]
public sealed partial class AutoSuggestBox : Control
{
    private const string TextBoxPartName = "PART_TextBox";

    private const string SuggestionsListPartName = "PART_SuggestionsList";

    private const string QueryButtonPartName = "PART_QueryButton";

    private TextBox? textBox;

    private ListBox? suggestionsList;

    private ButtonBase? queryButton;

    private Window? window;

    private bool updatingText;

    private bool suggestionsRequested;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChanged = nameof(OnTextChanged))]
    public partial string? Text { get; set; }

    [DependencyProperty]
    public partial string? PlaceholderText { get; set; }

    [DependencyProperty]
    public partial string? QueryIcon { get; set; }

    [DependencyProperty]
    public partial string? NoResultsText { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnItemsSourceChanged))]
    public partial IEnumerable? ItemsSource { get; set; }

    [DependencyProperty]
    public partial DataTemplate? ItemTemplate { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChanged = nameof(OnIsSuggestionListOpenChanged))]
    public partial bool IsSuggestionListOpen { get; set; }

    [DependencyProperty]
    public partial ICommand? QuerySubmittedCommand { get; set; }

    static AutoSuggestBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSuggestBox), new FrameworkPropertyMetadata(typeof(AutoSuggestBox)));
    }

    public AutoSuggestBox()
    {
        Unloaded += OnUnloaded;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (textBox is not null)
        {
            textBox.TextChanged -= OnTextBoxTextChanged;
            textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
        }
        if (suggestionsList is not null)
        {
            suggestionsList.PreviewMouseLeftButtonUp -= OnSuggestionsListMouseUp;
        }
        if (queryButton is not null)
        {
            queryButton.Click -= OnQueryButtonClick;
        }

        textBox = GetTemplateChild(TextBoxPartName) as TextBox;
        suggestionsList = GetTemplateChild(SuggestionsListPartName) as ListBox;
        queryButton = GetTemplateChild(QueryButtonPartName) as ButtonBase;

        if (textBox is not null)
        {
            textBox.Text = Text ?? string.Empty;
            textBox.TextChanged += OnTextBoxTextChanged;
            textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
        }
        if (suggestionsList is not null)
        {
            suggestionsList.PreviewMouseLeftButtonUp += OnSuggestionsListMouseUp;
        }
        if (queryButton is not null)
        {
            queryButton.Click += OnQueryButtonClick;
        }
    }

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);
        if (!(bool)e.NewValue)
        {
            CloseSuggestions();
        }
    }

    private void OnTextChanged()
    {
        if ((textBox is null) || (textBox.Text == (Text ?? string.Empty)))
        {
            return;
        }

        updatingText = true;
        textBox.Text = Text ?? string.Empty;
        textBox.CaretIndex = textBox.Text.Length;
        updatingText = false;
    }

    private void OnItemsSourceChanged() => UpdateSuggestionList();

    private void OnIsSuggestionListOpenChanged()
    {
        if (window is not null)
        {
            window.PreviewMouseDown -= OnWindowMouseInput;
            window.PreviewMouseWheel -= OnWindowMouseInput;
            window.Deactivated -= OnWindowChanged;
            window.LocationChanged -= OnWindowChanged;
            window.SizeChanged -= OnWindowChanged;
            window = null;
        }

        if (IsSuggestionListOpen && (Window.GetWindow(this) is { } owner))
        {
            window = owner;
            window.PreviewMouseDown += OnWindowMouseInput;
            window.PreviewMouseWheel += OnWindowMouseInput;
            window.Deactivated += OnWindowChanged;
            window.LocationChanged += OnWindowChanged;
            window.SizeChanged += OnWindowChanged;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => CloseSuggestions();

    private void OnWindowMouseInput(object sender, MouseEventArgs e)
    {
        if (!IsMouseOver)
        {
            CloseSuggestions();
        }
    }

    private void OnWindowChanged(object? sender, EventArgs e) => CloseSuggestions();

    private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (updatingText || (textBox is null))
        {
            return;
        }

        suggestionsRequested = textBox.Text.Length > 0;
        suggestionsList?.SelectedIndex = -1;
        Text = textBox.Text;
        UpdateSuggestionList();
    }

    private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                e.Handled = Move(1);
                break;
            case Key.Up:
                e.Handled = Move(-1);
                break;
            case Key.Enter:
                Submit(IsSuggestionListOpen ? suggestionsList?.SelectedItem : null);
                e.Handled = true;
                break;
            case Key.Escape when IsSuggestionListOpen:
                CloseSuggestions();
                e.Handled = true;
                break;
        }
    }

    private void OnSuggestionsListMouseUp(object sender, MouseButtonEventArgs e)
    {
        if ((suggestionsList is not null) &&
            (e.OriginalSource is DependencyObject source) &&
            (ItemsControl.ContainerFromElement(suggestionsList, source) is ListBoxItem container))
        {
            Submit(suggestionsList.ItemContainerGenerator.ItemFromContainer(container));
            e.Handled = true;
        }
    }

    private void OnQueryButtonClick(object sender, RoutedEventArgs e) => Submit(null);

    private bool Move(int step)
    {
        if (!IsSuggestionListOpen)
        {
            suggestionsRequested = !String.IsNullOrEmpty(Text);
            UpdateSuggestionList();
            return IsSuggestionListOpen;
        }

        if ((suggestionsList is null) || (suggestionsList.Items.Count == 0))
        {
            return false;
        }

        var count = suggestionsList.Items.Count;
        var index = suggestionsList.SelectedIndex + step;
        suggestionsList.SelectedIndex = index < 0 ? count - 1 : index >= count ? 0 : index;
        suggestionsList.ScrollIntoView(suggestionsList.SelectedItem);
        return true;
    }

    private void Submit(object? suggestion)
    {
        CloseSuggestions();
        if (suggestion is not null)
        {
            Text = suggestion.ToString();
        }

        var query = new AutoSuggestBoxQuery(Text ?? string.Empty, suggestion);
        if ((QuerySubmittedCommand is { } command) && command.CanExecute(query))
        {
            command.Execute(query);
        }
    }

    private void CloseSuggestions()
    {
        suggestionsRequested = false;
        IsSuggestionListOpen = false;
        suggestionsList?.SelectedIndex = -1;
    }

    private void UpdateSuggestionList() =>
        IsSuggestionListOpen = suggestionsRequested && ((ItemsSource?.Cast<object>().Any() ?? false) || (NoResultsText is not null));
}
