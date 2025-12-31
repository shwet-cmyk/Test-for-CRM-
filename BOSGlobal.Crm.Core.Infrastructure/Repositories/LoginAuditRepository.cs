using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Entities;
using BOSGlobal.Crm.Infrastructure.Identity;

namespace BOSGlobal.Crm.Infrastructure.Repositories;

public class LoginAuditRepository : ILoginAuditRepository
{
    private readonly CrmDbContext _db;

    public LoginAuditRepository(CrmDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(LoginAudit audit)
    {
        audit.Timestamp = DateTime.UtcNow;
        _db.LoginAudits!.Add(audit);
        await _db.SaveChangesAsync();
    }
}
