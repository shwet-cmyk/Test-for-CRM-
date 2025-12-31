using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Application.Services;
using BOSGlobal.Crm.Infrastructure.Services;
using BOSGlobal.Crm.Infrastructure.Services.Initialization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var erpConnection = builder.Configuration.GetConnectionString("ErpConnection");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorizationCore();
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Configure cookie options: 30 minute inactivity session
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// Use configuration-aware extension so OTP provider (Twilio) is registered when configured.
builder.Services.AddCrmInfrastructure(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserQueryService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
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
app.UseAuthorization();

// Validate sessions on each request (single-active-session enforcement)
app.UseMiddleware<BOSGlobal.Crm.Infrastructure.Services.SessionValidationMiddleware>();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();
