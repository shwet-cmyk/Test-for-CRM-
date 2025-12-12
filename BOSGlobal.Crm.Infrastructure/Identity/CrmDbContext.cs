using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BOSGlobal.Crm.Infrastructure.Identity;

public class CrmDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
    {
    }
}
