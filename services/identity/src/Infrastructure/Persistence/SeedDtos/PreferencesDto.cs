namespace Vendo.IdentityManagement.Infrastructure.Persistence.SeedDtos;

public class PreferencesDto
{
    public bool Newsletter { get; set; }
    public bool SmsNotifications { get; set; }
    public bool EmailNotifications { get; set; }
    public string Language { get; set; } = "en-US";
    public string Timezone { get; set; } = "UTC";
}
