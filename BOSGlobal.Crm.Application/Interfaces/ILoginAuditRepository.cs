using BOSGlobal.Crm.Domain.Entities;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface ILoginAuditRepository
{
    Task LogAsync(LoginAudit audit);
}
