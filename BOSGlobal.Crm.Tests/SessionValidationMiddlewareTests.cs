using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using BOSGlobal.Crm.Infrastructure.Identity;
using BOSGlobal.Crm.Infrastructure.Services;

namespace BOSGlobal.Crm.Tests;

public class SessionValidationMiddlewareTests
{
    private CrmDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new CrmDbContext(options);
    }

    private UserManager<ApplicationUser> CreateUserManager(CrmDbContext db)
    {
        var store = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<ApplicationUser, ApplicationRole, CrmDbContext, string>(db);
        var pwdHasher = new PasswordHasher<ApplicationUser>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var pwdValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var services = null as IServiceProvider;
        var logger = NullLogger<UserManager<ApplicationUser>>.Instance;

        return new UserManager<ApplicationUser>(store, Options.Create(new IdentityOptions()), pwdHasher, userValidators, pwdValidators, keyNormalizer, errors, services, logger);
    }

    private class SimpleClaimsFactory : IUserClaimsPrincipalFactory<ApplicationUser>
    {
        public Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var identity = new ClaimsIdentity("TestAuth");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty));
            identity.AddClaim(new Claim("sessionId", user.SessionId ?? string.Empty));
            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult(principal);
        }
    }

    private class DummyUserConfirmation : IUserConfirmation<ApplicationUser>
    {
        public Task<bool> IsConfirmedAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return Task.FromResult(true);
        }
    }

    private class TestSignInManager : SignInManager<ApplicationUser>
    {
        public bool SignedOutCalled { get; private set; }

        public TestSignInManager(UserManager<ApplicationUser> userManager)
            : base(userManager,
                  new HttpContextAccessor(),
                  new SimpleClaimsFactory(),
                  Options.Create(new IdentityOptions()),
                  NullLoggerFactory.Instance.CreateLogger<SignInManager<ApplicationUser>>(),
                  new Microsoft.AspNetCore.Authentication.AuthenticationSchemeProvider(Options.Create(new Microsoft.AspNetCore.Authentication.AuthenticationOptions())),
                  new DummyUserConfirmation())
        {
        }

        public override Task SignOutAsync()
        {
            SignedOutCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Middleware_AllowsValidSession_UpdatesTimestamps()
    {
        var db = CreateInMemoryDb("validsession");
        var userManager = CreateUserManager(db);

        var user = new ApplicationUser { UserName = "testuser", Email = "a@b.com" };
        await userManager.CreateAsync(user, "Password123!");

        // set session id on user
        user.SessionId = "sess-123";
        await userManager.UpdateAsync(user);

        var signInManager = new TestSignInManager(userManager);

        var middleware = new SessionValidationMiddleware((innerHttp) => Task.CompletedTask);

        // create http context with principal matching session id
        var ctx = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("sessionId", "sess-123")
        }, "TestAuth");
        ctx.User = new ClaimsPrincipal(identity);

        await middleware.InvokeAsync(ctx, userManager, signInManager);

        var updated = await userManager.FindByIdAsync(user.Id);
        Assert.NotNull(updated.LastActivityUtc);
        Assert.NotNull(updated.SessionExpiresUtc);
        Assert.True(updated.SessionExpiresUtc > DateTime.UtcNow);
        Assert.False(signInManager.SignedOutCalled);
    }

    [Fact]
    public async Task Middleware_MismatchedSession_SignsOut()
    {
        var db = CreateInMemoryDb("mismatchsession");
        var userManager = CreateUserManager(db);

        var user = new ApplicationUser { UserName = "testuser2", Email = "c@d.com" };
        await userManager.CreateAsync(user, "Password123!");

        user.SessionId = "sess-abc";
        await userManager.UpdateAsync(user);

        var signInManager = new TestSignInManager(userManager);

        var middleware = new SessionValidationMiddleware((innerHttp) => Task.CompletedTask);

        var ctx = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("sessionId", "other-session")
        }, "TestAuth");
        ctx.User = new ClaimsPrincipal(identity);

        await middleware.InvokeAsync(ctx, userManager, signInManager);

        Assert.True(signInManager.SignedOutCalled);
    }
}
