using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class UserRoleMapping
{
    public int Id { get; set; }
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty; // ApplicationUser.Id
    public int RoleMasterId { get; set; }
    public RoleMaster? Role { get; set; }
}
