namespace Template.WindowsApp.Settings;

using Microsoft.Extensions.Options;

using Template.WindowsApp.Services;

public sealed class ClientSetting
{
    [Required]
    [ValidateObjectMembers]
    public GreetOption Greet { get; set; } = default!;
}
