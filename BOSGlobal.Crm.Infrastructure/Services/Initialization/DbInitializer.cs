using BOSGlobal.Crm.Domain.Enums;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BOSGlobal.Crm.Infrastructure.Services.Initialization;

public class DbInitializer
{
    private readonly CrmDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public DbInitializer(CrmDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.MigrateAsync();

        foreach (var role in Enum.GetNames(typeof(UserRole)))
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        const string adminEmail = "admin@crm.local";
        const string adminPassword = "Admin@123";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create admin user: {string.Join(',', result.Errors.Select(e => e.Description))}");
            }
        }

        if (!await _userManager.IsInRoleAsync(adminUser, UserRole.Admin.ToString()))
        {
            await _userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
        }
    }
}
