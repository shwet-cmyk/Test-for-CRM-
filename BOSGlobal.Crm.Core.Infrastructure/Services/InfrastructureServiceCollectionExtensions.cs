using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Infrastructure.Identity;
using BOSGlobal.Crm.Infrastructure.Repositories;
using BOSGlobal.Crm.Infrastructure.Services;
using BOSGlobal.Crm.Infrastructure.Services.Initialization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace BOSGlobal.Crm.Infrastructure.Services;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddCrmInfrastructure(this IServiceCollection services, string? defaultConnection, string? erpConnection)
    {
        return AddCrmInfrastructure(services, null, defaultConnection, erpConnection);
    }

    // New overload that accepts an IConfiguration instance. This allows conditional registration
    // of services that depend on configuration (for example Twilio for OTP delivery).
    public static IServiceCollection AddCrmInfrastructure(this IServiceCollection services, IConfiguration? configuration)
    {
        var defaultConnection = configuration?.GetConnectionString("DefaultConnection");
        var erpConnection = configuration?.GetConnectionString("ErpConnection");
        return AddCrmInfrastructure(services, configuration, defaultConnection, erpConnection);
    }

    private static IServiceCollection AddCrmInfrastructure(this IServiceCollection services, IConfiguration? configuration, string? defaultConnection, string? erpConnection)
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
    services.AddScoped<IRoleRightsRepository, RoleRightsRepository>();
    // Use our custom claims principal factory so the session id stored on the user becomes part of the auth cookie
    services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationClaimsPrincipalFactory>();
    services.AddScoped<ILoginAuditRepository, LoginAuditRepository>();
    services.AddScoped<IModuleAccessService, ModuleAccessService>();

        services.AddMemoryCache();

        // Register OTP provider: prefer Twilio Verify if configured, otherwise fall back to in-memory dev stub.
        var twilioSid = configuration?["Twilio:AccountSid"];
        var twilioAuth = configuration?["Twilio:AuthToken"];
        var twilioVerifySid = configuration?["Twilio:VerifyServiceSid"];

        if (!string.IsNullOrWhiteSpace(twilioSid) && !string.IsNullOrWhiteSpace(twilioAuth) && !string.IsNullOrWhiteSpace(twilioVerifySid))
        {
            services.AddSingleton<IPhoneOtpService, TwilioPhoneOtpService>();
        }
        else
        {
            // Dev-friendly fallback
            services.AddSingleton<IPhoneOtpService, InMemoryPhoneOtpService>();
        }

        // reCAPTCHA verification service
        services.AddHttpClient();
        services.AddScoped<IRecaptchaService, RecaptchaService>();
        services.AddScoped<DbInitializer>();

        return services;
    }
}
