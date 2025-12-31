using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class RoleMaster
{
    public int Id { get; set; }
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(1024)]
    public string? Description { get; set; }
}
