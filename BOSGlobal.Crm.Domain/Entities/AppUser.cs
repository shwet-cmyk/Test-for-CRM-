namespace BOSGlobal.Crm.Domain.Entities;

using BOSGlobal.Crm.Domain.Enums;

public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Email { get; set; } = string.Empty;

    public List<UserRole> Roles { get; set; } = new();

    public bool IsActive { get; set; } = true;
}
