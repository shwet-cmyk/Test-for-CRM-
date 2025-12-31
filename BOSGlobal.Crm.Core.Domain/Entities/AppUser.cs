namespace BOSGlobal.Crm.Domain.Entities;

using BOSGlobal.Crm.Domain.Enums;

public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Email { get; set; } = string.Empty;

    public List<UserRole> Roles { get; set; } = new();

    public bool IsActive { get; set; } = true;

    // Optional ERP identifier and GST number copied from identity user
    public string? ErpId { get; set; }

    public string? GstNumber { get; set; }

    // Phone number (IdentityUser already stores phone number, but expose here for domain usage)
    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }
}
