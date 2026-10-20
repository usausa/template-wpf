namespace Template.WindowsApp.Views.Collections;

public sealed partial class Customer : ObservableObject
{
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

    public Customer(string name, string company, string email, string phone, string city)
    {
        Name = name;
        Company = company;
        Email = email;
        Phone = phone;
        City = city;
    }
}
