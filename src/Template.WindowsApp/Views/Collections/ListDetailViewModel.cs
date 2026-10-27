namespace Template.WindowsApp.Views.Collections;

using Smart.Mapper;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models.Entity;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

//--------------------------------------------------------------------------------
// Mapper
//--------------------------------------------------------------------------------

public static partial class ListDetailViewModelMapper
{
    [Mapper]
    public static partial void CopyTo(this CustomerEntity source, ListDetailViewModel destination);

    [Mapper]
    public static partial void CopyTo(this ListDetailViewModel source, CustomerEntity destination);
}

//--------------------------------------------------------------------------------
// ViewModel
//--------------------------------------------------------------------------------

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ListDetailViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string NewCustomerName = "New customer";

    private readonly IDialog dialog;

    private readonly DataService dataService;

    public ObservableCollection<CustomerEntity> Customers { get; }

    [ObservableProperty]
    public partial CustomerEntity? SelectedCustomer { get; set; }

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

    public ListDetailViewModel(IDialog dialog, DataService dataService)
    {
        this.dialog = dialog;
        this.dataService = dataService;
        Customers = [.. dataService.QueryCustomers()];
        Name = string.Empty;
        Company = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        City = string.Empty;

        SubscribeSelectedCustomer(Load);

        AddCommand = MakeAsyncCommand(AddAsync);
        DeleteCommand = MakeAsyncCommand(DeleteAsync, () => SelectedCustomer is not null);
        SaveCommand = MakeAsyncCommand(SaveAsync, IsChanged);
        CancelCommand = MakeDelegateCommand(() => Load(SelectedCustomer), IsChanged);

        SelectedCustomer = Customers.Count > 0 ? Customers[0] : null;
    }

    private void Load(CustomerEntity? customer)
    {
        if (customer is not null)
        {
            customer.CopyTo(this);
            return;
        }

        Name = string.Empty;
        Company = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        City = string.Empty;
    }

    private bool IsChanged() =>
        (SelectedCustomer is { } customer) &&
        ((Name != customer.Name) || (Company != customer.Company) || (Email != customer.Email) || (Phone != customer.Phone) || (City != customer.City));

    private async Task AddAsync()
    {
        var customer = await dataService.InsertCustomerAsync(NewCustomerName).ConfigureAwait(true);
        Customers.Add(customer);
        SelectedCustomer = customer;
    }

    private async Task DeleteAsync()
    {
        if ((SelectedCustomer is not { } customer) || !await dialog.ConfirmAsync("Delete this customer?").ConfigureAwait(true))
        {
            return;
        }

        await dataService.DeleteCustomerAsync(customer.Id).ConfigureAwait(true);
        var index = Customers.IndexOf(customer);
        Customers.RemoveAt(index);
        SelectedCustomer = Customers.Count > 0 ? Customers[Math.Min(index, Customers.Count - 1)] : null;
    }

    private async Task SaveAsync()
    {
        if (SelectedCustomer is not { } customer)
        {
            return;
        }

        var updated = new CustomerEntity { Id = customer.Id };
        this.CopyTo(updated);
        await dataService.UpdateCustomerAsync(updated).ConfigureAwait(true);
        Customers[Customers.IndexOf(customer)] = updated;
        SelectedCustomer = updated;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("ListDetail:Add", "Add", "List & detail", "\uE710", AddCommand),
        new("ListDetail:Delete", "Delete", "List & detail", "\uE74D", DeleteCommand),
        new("ListDetail:Save", "Save", "List & detail", "\uE74E", SaveCommand),
        new("ListDetail:Cancel", "Cancel", "List & detail", "\uE711", CancelCommand)
    ];
}
