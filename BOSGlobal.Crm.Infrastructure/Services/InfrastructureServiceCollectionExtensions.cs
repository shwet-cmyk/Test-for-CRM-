using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Infrastructure.Identity;
using BOSGlobal.Crm.Infrastructure.Repositories;
using BOSGlobal.Crm.Infrastructure.Services;
using BOSGlobal.Crm.Infrastructure.Services.Initialization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BOSGlobal.Crm.Infrastructure.Services;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddCrmInfrastructure(this IServiceCollection services, string? defaultConnection, string? erpConnection)
    {
        services.AddDbContext<CrmDbContext>(options => options.UseSqlServer(defaultConnection));
        services.AddDbContext<ErpReadDbContext>(options => options.UseSqlServer(erpConnection));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<CrmDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IIdentityGateway, IdentityGateway>();
        services.AddScoped<DbInitializer>();

        services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

        return services;
    }
}
