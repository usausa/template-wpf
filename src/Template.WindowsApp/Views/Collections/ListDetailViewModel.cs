namespace Template.WindowsApp.Views.Collections;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ListDetailViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string NewCustomerName = "New customer";

    public ObservableCollection<Customer> Customers { get; } =
    [
        new("Emma Wilson", "Contoso", "emma.wilson@contoso.com", "+1 555-0101", "Seattle"),
        new("Liam Chen", "Fabrikam", "liam.chen@fabrikam.com", "+1 555-0102", "Austin"),
        new("Olivia Martinez", "Northwind Traders", "olivia.martinez@northwindtraders.com", "+1 555-0103", "Boston"),
        new("Noah Johnson", "Adventure Works", "noah.johnson@adventure-works.com", "+1 555-0104", "Denver"),
        new("Ava Brown", "Tailspin Toys", "ava.brown@tailspintoys.com", "+1 555-0105", "Chicago"),
        new("Ethan Davis", "Woodgrove Bank", "ethan.davis@woodgrovebank.com", "+1 555-0106", "New York"),
        new("Sophia Garcia", "Litware", "sophia.garcia@litwareinc.com", "+1 555-0107", "Portland"),
        new("Mason Miller", "Proseware", "mason.miller@proseware.com", "+1 555-0108", "San Diego"),
        new("Isabella Lee", "Wide World Importers", "isabella.lee@wideworldimporters.com", "+1 555-0109", "Atlanta"),
        new("Lucas Anderson", "Alpine Ski House", "lucas.anderson@alpineskihouse.com", "+1 555-0110", "Salt Lake City"),
        new("Mia Thomas", "Coho Winery", "mia.thomas@cohowinery.com", "+1 555-0111", "Napa"),
        new("James Taylor", "Fourth Coffee", "james.taylor@fourthcoffee.com", "+1 555-0112", "Seattle"),
        new("Charlotte Moore", "Lucerne Publishing", "charlotte.moore@lucernepublishing.com", "+1 555-0113", "Boston"),
        new("Benjamin Jackson", "Margie's Travel", "benjamin.jackson@margiestravel.com", "+1 555-0114", "Miami"),
        new("Amelia White", "Trey Research", "amelia.white@treyresearch.net", "+1 555-0115", "Raleigh"),
        new("Henry Harris", "Blue Yonder Airlines", "henry.harris@blueyonderairlines.com", "+1 555-0116", "Dallas"),
        new("Harper Martin", "City Power & Light", "harper.martin@cpandl.com", "+1 555-0117", "Phoenix"),
        new("Alexander Thompson", "Graphic Design Institute", "alexander.thompson@graphicdesigninstitute.com", "+1 555-0118", "Chicago"),
        new("Evelyn Clark", "School of Fine Art", "evelyn.clark@fineartschool.net", "+1 555-0119", "Philadelphia"),
        new("Daniel Lewis", "Humongous Insurance", "daniel.lewis@humongousinsurance.com", "+1 555-0120", "Minneapolis")
    ];

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

    public ListDetailViewModel(IDialogService dialogService)
    {
        Name = string.Empty;
        Company = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        City = string.Empty;

        SubscribeSelectedCustomer(Load);

        AddCommand = MakeDelegateCommand(Add);
        DeleteCommand = MakeAsyncCommand(() => DeleteAsync(dialogService), () => SelectedCustomer is not null);
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

    private async Task DeleteAsync(IDialogService dialogService)
    {
        if ((SelectedCustomer is not { } customer) || !await dialogService.ConfirmAsync("Delete this customer?").ConfigureAwait(true))
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
