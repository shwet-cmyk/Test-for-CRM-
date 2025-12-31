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
    public DbSet<BOSGlobal.Crm.Domain.Entities.OtpChallenge>? OtpChallenges { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.CaptchaVerificationLog>? CaptchaVerificationLogs { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.OtpAudit>? OtpAudits { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.WidgetLibrary>? WidgetLibraries { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.UserDashboardConfig>? UserDashboardConfigs { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.WidgetDataCache>? WidgetDataCaches { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.AttendanceLog>? AttendanceLogs { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.AttendanceRule>? AttendanceRules { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.EmployeeShift>? EmployeeShifts { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.AttendanceOverride>? AttendanceOverrides { get; set; }
    public DbSet<BOSGlobal.Crm.Domain.Entities.GeoMapping>? GeoMappings { get; set; }
}
