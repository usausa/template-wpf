namespace Template.WindowsApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

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

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class CustomerSample
{
    public static IReadOnlyList<Customer> LoadCustomers() =>
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
}
