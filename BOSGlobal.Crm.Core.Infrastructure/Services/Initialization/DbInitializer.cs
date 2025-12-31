using BOSGlobal.Crm.Domain.Enums;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BOSGlobal.Crm.Infrastructure.Services.Initialization;

public class DbInitializer
{
    private readonly CrmDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _configuration;

    public DbInitializer(
        CrmDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        var roleMasters = _context.RoleMasters;
        if (roleMasters is null)
        {
            return;
        }

        foreach (var role in Enum.GetNames(typeof(UserRole)))
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        // ensure RoleMaster table has the same canonical roles for business logic and permissions
        foreach (var role in Enum.GetNames(typeof(UserRole)))
        {
            if (!await roleMasters.AnyAsync(r => r.Name == role))
            {
                roleMasters.Add(new BOSGlobal.Crm.Domain.Entities.RoleMaster { Name = role });
            }
        }
        await _context.SaveChangesAsync();

        // Seed canonical modules
        var modulesSet = _context.Modules;
        if (modulesSet is null)
        {
            return;
        }

        var seedModules = new[] { "CRM", "Inventory", "Books", "HR" };
        foreach (var moduleKey in seedModules)
        {
            if (!await modulesSet.AnyAsync(m => m.Key == moduleKey))
            {
                modulesSet.Add(new BOSGlobal.Crm.Domain.Entities.Module
                {
                    Key = moduleKey,
                    Name = moduleKey,
                    Description = $"{moduleKey} module"
                });
            }
        }
        await _context.SaveChangesAsync();

        var adminEmail = _configuration["Seed:AdminEmail"] ?? "admin@bosglobal.local";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            // Identity seed is handled separately; nothing else to do for tenant bootstrap without the admin user
            return;
        }

        // Bootstrap a default tenant for admin to make the system usable
        var tenantsSet = _context.Tenants;
        if (tenantsSet is null || _context.UserTenants is null || _context.TenantSubscriptions is null || _context.UserEntitlements is null)
        {
            return;
        }

        var tenant = await tenantsSet.FirstOrDefaultAsync(t => t.Name == "Default Tenant");
        if (tenant is null)
        {
            tenant = new BOSGlobal.Crm.Domain.Entities.Tenant { Name = "Default Tenant" };
            tenantsSet.Add(tenant);
            await _context.SaveChangesAsync();
        }

        if (!await _context.UserTenants.AnyAsync(ut => ut.UserId == adminUser.Id && ut.TenantId == tenant.Id))
        {
            _context.UserTenants.Add(new BOSGlobal.Crm.Domain.Entities.UserTenant
            {
                TenantId = tenant.Id,
                UserId = adminUser.Id,
                Realm = Domain.Enums.Realm.User,
                IsAdmin = true
            });
            await _context.SaveChangesAsync();
        }

        var moduleIds = await modulesSet.Select(m => new { m.Id, m.Key }).ToListAsync();
        foreach (var module in moduleIds)
        {
            if (!await _context.TenantSubscriptions.AnyAsync(ts => ts.TenantId == tenant.Id && ts.ModuleId == module.Id))
            {
                _context.TenantSubscriptions.Add(new BOSGlobal.Crm.Domain.Entities.TenantSubscription { TenantId = tenant.Id, ModuleId = module.Id, IsActive = true });
            }
            if (!await _context.UserEntitlements.AnyAsync(ue => ue.TenantId == tenant.Id && ue.ModuleId == module.Id && ue.UserId == adminUser.Id))
            {
                _context.UserEntitlements.Add(new BOSGlobal.Crm.Domain.Entities.UserEntitlement { TenantId = tenant.Id, ModuleId = module.Id, UserId = adminUser.Id, GrantedByUserId = adminUser.Id });
            }
        }

        await _context.SaveChangesAsync();
    }

    public static async Task SeedIdentityAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@bosglobal.local";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin@12345!";

        if (!await roleManager.RoleExistsAsync(UserRole.Admin.ToString()))
        {
            await roleManager.CreateAsync(new ApplicationRole(UserRole.Admin.ToString()));
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, UserRole.Admin.ToString()))
        {
            await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
        }
    }
}
