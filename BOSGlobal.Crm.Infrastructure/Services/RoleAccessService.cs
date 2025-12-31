using BOSGlobal.Crm.Application.DTOs;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class RoleAccessService : IRoleAccessService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CrmDbContext _dbContext;
    private readonly ILogger<RoleAccessService> _logger;

    private static readonly Dictionary<string, RoleAccessTemplate> RoleTemplates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = new RoleAccessTemplate("/dashboards/admin", "Welcome, Admin", new[]
        {
            "*"
        }),
        ["Manager"] = new RoleAccessTemplate("/dashboards/manager", "Welcome, Manager", new[]
        {
            "Dashboard.View","TeamPerformance.View","Leads.Assign","Opportunities.Assign","Targets.Manage","Incentives.View","Escalations.Manage","Reports.View"
        }),
        ["SalesExecutive"] = new RoleAccessTemplate("/dashboards/sales", "Welcome, Sales Executive", new[]
        {
            "Leads.View","Leads.Update","Opportunities.View","Opportunities.Update","FollowUps.Manage","Attendance.Manage","Meetings.Manage","Incentives.View"
        }),
        ["Telecaller"] = new RoleAccessTemplate("/dashboards/telecaller", "Welcome, Telecaller", new[]
        {
            "Leads.View","Calls.Log","Calls.MarkDnd","Calls.MarkWrongNumber","CallingWindows.Access"
        }),
        ["Marketing"] = new RoleAccessTemplate("/dashboards/marketing", "Welcome, Marketing", new[]
        {
            "Campaigns.Manage","EmailTools.Use","LeadSource.Configure","Analytics.View"
        })
    };

    public RoleAccessService(
        UserManager<ApplicationUser> userManager,
        CrmDbContext dbContext,
        ILogger<RoleAccessService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<RoleAccessProfileDto> BuildAccessProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Access profile requested for missing user {UserId}", userId);
            return new RoleAccessProfileDto
            {
                HasAssignedRole = false,
                ErrorMessage = "User not found.",
                SessionTimeoutMinutes = 30
            };
        }

        var identityRoles = await _userManager.GetRolesAsync(user);

        // Include database-managed role mappings
        var mappedRoles = await _dbContext.UserRoleMappings!
            .Include(m => m.Role)
            .Where(m => m.UserId == user.Id)
            .Select(m => m.Role!.Name)
            .ToListAsync(cancellationToken);

        var roles = identityRoles.Concat(mappedRoles).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (!roles.Any())
        {
            _logger.LogWarning("Access denied for user {UserId} due to missing roles", user.Id);
            return new RoleAccessProfileDto
            {
                Roles = Array.Empty<string>(),
                Permissions = Array.Empty<string>(),
                HasAssignedRole = false,
                ErrorMessage = "Access denied. No role assigned. Contact Admin.",
                SessionTimeoutMinutes = 30
            };
        }

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? dashboard = null;
        string? welcome = null;

        foreach (var role in roles)
        {
            if (RoleTemplates.TryGetValue(role, out var template))
            {
                if (template.Permissions.Contains("*"))
                {
                    permissions.Add("*");
                }
                else
                {
                    foreach (var p in template.Permissions)
                    {
                        permissions.Add(p);
                    }
                }

                if (dashboard is null)
                {
                    dashboard = template.DashboardPath;
                    welcome = template.WelcomeMessage;
                }
            }
        }

        // If any role requests full access, expand it to a meaningful superset
        if (permissions.Contains("*"))
        {
            foreach (var template in RoleTemplates.Values)
            {
                foreach (var perm in template.Permissions.Where(p => p != "*"))
                {
                    permissions.Add(perm);
                }
            }
        }

        // Merge RoleRights toggles (only those explicitly allowed)
        var roleMasterIds = await _dbContext.UserRoleMappings!
            .Where(m => m.UserId == user.Id)
            .Select(m => m.RoleMasterId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (roleMasterIds.Count > 0)
        {
            var roleRights = await _dbContext.RoleRights!
                .Where(r => roleMasterIds.Contains(r.RoleMasterId) && r.IsAllowed)
                .Select(r => r.PermissionKey)
                .ToListAsync(cancellationToken);

            foreach (var right in roleRights)
            {
                permissions.Add(right);
            }
        }

        // If still no dashboard, pick a sensible default based on precedence
        dashboard ??= ResolveDefaultDashboard(roles);

        return new RoleAccessProfileDto
        {
            Roles = roles,
            Permissions = permissions,
            DashboardPath = dashboard,
            WelcomeMessage = welcome ?? "Welcome back",
            HasAssignedRole = true,
            SessionTimeoutMinutes = 30
        };
    }

    private static string ResolveDefaultDashboard(IEnumerable<string> roles)
    {
        if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase))) return "/dashboards/admin";
        if (roles.Any(r => r.Equals("Manager", StringComparison.OrdinalIgnoreCase))) return "/dashboards/manager";
        if (roles.Any(r => r.Equals("SalesExecutive", StringComparison.OrdinalIgnoreCase) || r.Equals("Sales", StringComparison.OrdinalIgnoreCase))) return "/dashboards/sales";
        if (roles.Any(r => r.Equals("Telecaller", StringComparison.OrdinalIgnoreCase))) return "/dashboards/telecaller";
        if (roles.Any(r => r.Equals("Marketing", StringComparison.OrdinalIgnoreCase))) return "/dashboards/marketing";
        return "/";
    }

    private record RoleAccessTemplate(string DashboardPath, string WelcomeMessage, IEnumerable<string> Permissions);
}
