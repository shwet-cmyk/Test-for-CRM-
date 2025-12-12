using Microsoft.AspNetCore.Identity;

namespace BOSGlobal.Crm.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    
    // ERP identifier from external system (optional)
    public string? ErpId { get; set; }

    // GST number (optional)
    public string? GstNumber { get; set; }
    
    // Session management to support single-active-session enforcement
    public string? SessionId { get; set; }
    public DateTime? SessionExpiresUtc { get; set; }
    public DateTime? LastActivityUtc { get; set; }
}
