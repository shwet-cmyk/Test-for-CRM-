using Microsoft.AspNetCore.Identity;

namespace BOSGlobal.Crm.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
}
