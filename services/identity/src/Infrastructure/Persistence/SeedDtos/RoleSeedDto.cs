namespace Vendo.IdentityManagement.Infrastructure.Persistence.SeedDtos;

public class RoleSeedDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; }
}
