using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BOSGlobal.Crm.Infrastructure.Identity;

public class CrmDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
    {
    }

    // Role/permission tables
    public DbSet<BOSGlobal.Crm.Domain.Entities.RoleMaster>? RoleMasters { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.UserRoleMapping>? UserRoleMappings { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.LoginAudit>? LoginAudits { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.RoleRight>? RoleRights { get; set; }
}
