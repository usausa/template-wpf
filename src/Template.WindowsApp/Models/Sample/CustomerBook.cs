namespace Template.WindowsApp.Models.Sample;

using Template.WindowsApp.Models.Entity;

public static class CustomerSample
{
    public static IReadOnlyList<CustomerEntity> LoadCustomers() =>
    [
        new() { Id = 1, Name = "Emma Wilson", Company = "Contoso", Email = "emma.wilson@contoso.com", Phone = "+1 555-0101", City = "Seattle" },
        new() { Id = 2, Name = "Liam Chen", Company = "Fabrikam", Email = "liam.chen@fabrikam.com", Phone = "+1 555-0102", City = "Austin" },
        new() { Id = 3, Name = "Olivia Martinez", Company = "Northwind Traders", Email = "olivia.martinez@northwindtraders.com", Phone = "+1 555-0103", City = "Boston" },
        new() { Id = 4, Name = "Noah Johnson", Company = "Adventure Works", Email = "noah.johnson@adventure-works.com", Phone = "+1 555-0104", City = "Denver" },
        new() { Id = 5, Name = "Ava Brown", Company = "Tailspin Toys", Email = "ava.brown@tailspintoys.com", Phone = "+1 555-0105", City = "Chicago" },
        new() { Id = 6, Name = "Ethan Davis", Company = "Woodgrove Bank", Email = "ethan.davis@woodgrovebank.com", Phone = "+1 555-0106", City = "New York" },
        new() { Id = 7, Name = "Sophia Garcia", Company = "Litware", Email = "sophia.garcia@litwareinc.com", Phone = "+1 555-0107", City = "Portland" },
        new() { Id = 8, Name = "Mason Miller", Company = "Proseware", Email = "mason.miller@proseware.com", Phone = "+1 555-0108", City = "San Diego" },
        new() { Id = 9, Name = "Isabella Lee", Company = "Wide World Importers", Email = "isabella.lee@wideworldimporters.com", Phone = "+1 555-0109", City = "Atlanta" },
        new() { Id = 10, Name = "Lucas Anderson", Company = "Alpine Ski House", Email = "lucas.anderson@alpineskihouse.com", Phone = "+1 555-0110", City = "Salt Lake City" },
        new() { Id = 11, Name = "Mia Thomas", Company = "Coho Winery", Email = "mia.thomas@cohowinery.com", Phone = "+1 555-0111", City = "Napa" },
        new() { Id = 12, Name = "James Taylor", Company = "Fourth Coffee", Email = "james.taylor@fourthcoffee.com", Phone = "+1 555-0112", City = "Seattle" },
        new() { Id = 13, Name = "Charlotte Moore", Company = "Lucerne Publishing", Email = "charlotte.moore@lucernepublishing.com", Phone = "+1 555-0113", City = "Boston" },
        new() { Id = 14, Name = "Benjamin Jackson", Company = "Margie's Travel", Email = "benjamin.jackson@margiestravel.com", Phone = "+1 555-0114", City = "Miami" },
        new() { Id = 15, Name = "Amelia White", Company = "Trey Research", Email = "amelia.white@treyresearch.net", Phone = "+1 555-0115", City = "Raleigh" },
        new() { Id = 16, Name = "Henry Harris", Company = "Blue Yonder Airlines", Email = "henry.harris@blueyonderairlines.com", Phone = "+1 555-0116", City = "Dallas" },
        new() { Id = 17, Name = "Harper Martin", Company = "City Power & Light", Email = "harper.martin@cpandl.com", Phone = "+1 555-0117", City = "Phoenix" },
        new() { Id = 18, Name = "Alexander Thompson", Company = "Graphic Design Institute", Email = "alexander.thompson@graphicdesigninstitute.com", Phone = "+1 555-0118", City = "Chicago" },
        new() { Id = 19, Name = "Evelyn Clark", Company = "School of Fine Art", Email = "evelyn.clark@fineartschool.net", Phone = "+1 555-0119", City = "Philadelphia" },
        new() { Id = 20, Name = "Daniel Lewis", Company = "Humongous Insurance", Email = "daniel.lewis@humongousinsurance.com", Phone = "+1 555-0120", City = "Minneapolis" }
    ];
}
