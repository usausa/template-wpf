namespace Template.WindowsApp.Models.Entity;

using Smart.Data.Accessor.Attributes;

using Template.WindowsApp.Helpers.Data;

[Name("Data")]
public sealed class DataEntity
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    [TypeHandler(typeof(DateTimeTicksConverter))]
    public DateTime CreateAt { get; set; }
}
