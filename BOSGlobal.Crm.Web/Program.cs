using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Application.Services;
using BOSGlobal.Crm.Infrastructure.Identity;
using BOSGlobal.Crm.Infrastructure.Services;
using BOSGlobal.Crm.Infrastructure.Services.Initialization;
using BOSGlobal.Crm.Web.Authorization;
using BOSGlobal.Crm.Web.Middleware;
using BOSGlobal.Crm.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var erpConnection = builder.Configuration.GetConnectionString("ErpConnection");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Module:CRM", policy => policy.Requirements.Add(new HasModuleAccessRequirement("CRM")));
    options.AddPolicy("Module:Inventory", policy => policy.Requirements.Add(new HasModuleAccessRequirement("Inventory")));
    options.AddPolicy("Module:Books", policy => policy.Requirements.Add(new HasModuleAccessRequirement("Books")));
    options.AddPolicy("Module:HR", policy => policy.Requirements.Add(new HasModuleAccessRequirement("HR")));
});
builder.Services.AddHttpContextAccessor();
// Configure cookie options: 10 minute inactivity session as requested in UI text
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = true;
});

// Use configuration-aware extension so OTP provider (Twilio) is registered when configured.
builder.Services.AddCrmInfrastructure(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserQueryService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IAuthorizationHandler, HasModuleAccessHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedIdentityAsync(app.Services, app.Configuration);

    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

// Validate sessions on each request (single-active-session enforcement)
app.UseMiddleware<BOSGlobal.Crm.Infrastructure.Services.SessionValidationMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();
