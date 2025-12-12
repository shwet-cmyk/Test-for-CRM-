using Microsoft.EntityFrameworkCore;

namespace BOSGlobal.Crm.Infrastructure.Identity;

public class ErpReadDbContext : DbContext
{
    public ErpReadDbContext(DbContextOptions<ErpReadDbContext> options)
        : base(options)
    {
    }
}
