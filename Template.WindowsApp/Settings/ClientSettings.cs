namespace Template.WindowsApp.Settings;

public sealed class ClientSettings
{
    [Required]
    public string Greeting { get; set; } = default!;
}
