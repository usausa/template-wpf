namespace Template.WindowsApp.Views.Collections;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models.Sample;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ListDetailViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string NewCustomerName = "New customer";

    public ObservableCollection<Customer> Customers { get; } = [.. CustomerSample.LoadCustomers()];

    [ObservableProperty]
    public partial Customer? SelectedCustomer { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Company { get; set; }

    [ObservableProperty]
    public partial string Email { get; set; }

    [ObservableProperty]
    public partial string Phone { get; set; }

    [ObservableProperty]
    public partial string City { get; set; }

    public IObserveCommand AddCommand { get; }

    public IObserveCommand DeleteCommand { get; }

    public IObserveCommand SaveCommand { get; }

    public IObserveCommand CancelCommand { get; }

    public ListDetailViewModel(IDialog dialog)
    {
        Name = string.Empty;
        Company = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        City = string.Empty;

        SubscribeSelectedCustomer(Load);

        AddCommand = MakeDelegateCommand(Add);
        DeleteCommand = MakeAsyncCommand(() => DeleteAsync(dialog), () => SelectedCustomer is not null);
        SaveCommand = MakeDelegateCommand(Save, IsChanged);
        CancelCommand = MakeDelegateCommand(() => Load(SelectedCustomer), IsChanged);

        SelectedCustomer = Customers[0];
    }

    private void Load(Customer? customer)
    {
        Name = customer?.Name ?? string.Empty;
        Company = customer?.Company ?? string.Empty;
        Email = customer?.Email ?? string.Empty;
        Phone = customer?.Phone ?? string.Empty;
        City = customer?.City ?? string.Empty;
    }

    private bool IsChanged() =>
        (SelectedCustomer is { } customer) &&
        ((Name != customer.Name) || (Company != customer.Company) || (Email != customer.Email) || (Phone != customer.Phone) || (City != customer.City));

    private void Add()
    {
        var customer = new Customer(NewCustomerName, string.Empty, string.Empty, string.Empty, string.Empty);
        Customers.Add(customer);
        SelectedCustomer = customer;
    }

    private async Task DeleteAsync(IDialog dialog)
    {
        if ((SelectedCustomer is not { } customer) || !await dialog.ConfirmAsync("Delete this customer?").ConfigureAwait(true))
        {
            return;
        }

        var index = Customers.IndexOf(customer);
        Customers.RemoveAt(index);
        SelectedCustomer = Customers.Count > 0 ? Customers[Math.Min(index, Customers.Count - 1)] : null;
    }

    private void Save()
    {
        if (SelectedCustomer is not { } customer)
        {
            return;
        }

        customer.Name = Name;
        customer.Company = Company;
        customer.Email = Email;
        customer.Phone = Phone;
        customer.City = City;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("ListDetail:Add", "Add", "List & detail", "\uE710", AddCommand),
        new("ListDetail:Delete", "Delete", "List & detail", "\uE74D", DeleteCommand),
        new("ListDetail:Save", "Save", "List & detail", "\uE74E", SaveCommand),
        new("ListDetail:Cancel", "Cancel", "List & detail", "\uE711", CancelCommand)
    ];
}
