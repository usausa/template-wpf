namespace Template.WindowsApp.Models.Entity;

using Smart.Data.Accessor.Attributes;

[Name("Customer")]
public sealed class CustomerEntity
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    public string Company { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Phone { get; set; } = default!;

    public string City { get; set; } = default!;
}
