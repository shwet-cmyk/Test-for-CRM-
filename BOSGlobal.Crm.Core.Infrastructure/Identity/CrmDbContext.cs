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
    public DbSet<BOSGlobal.Crm.Domain.Entities.Tenant>? Tenants { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.Module>? Modules { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.TenantSubscription>? TenantSubscriptions { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.UserTenant>? UserTenants { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.UserEntitlement>? UserEntitlements { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BOSGlobal.Crm.Domain.Entities.Module>()
            .HasIndex(m => m.Key)
            .IsUnique();

        builder.Entity<BOSGlobal.Crm.Domain.Entities.TenantSubscription>()
            .HasOne(ts => ts.Tenant)
            .WithMany()
            .HasForeignKey(ts => ts.TenantId);

        builder.Entity<BOSGlobal.Crm.Domain.Entities.TenantSubscription>()
            .HasOne(ts => ts.Module)
            .WithMany()
            .HasForeignKey(ts => ts.ModuleId);

        builder.Entity<BOSGlobal.Crm.Domain.Entities.UserTenant>()
            .HasOne(ut => ut.Tenant)
            .WithMany()
            .HasForeignKey(ut => ut.TenantId);

        builder.Entity<BOSGlobal.Crm.Domain.Entities.UserEntitlement>()
            .HasOne(ue => ue.Tenant)
            .WithMany()
            .HasForeignKey(ue => ue.TenantId);

        builder.Entity<BOSGlobal.Crm.Domain.Entities.UserEntitlement>()
            .HasOne(ue => ue.Module)
            .WithMany()
            .HasForeignKey(ue => ue.ModuleId);
    }
}
