namespace BOSGlobal.Crm.Domain.Entities;

public class UserRoleMapping
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty; // ApplicationUser.Id
    public int RoleMasterId { get; set; }
    public RoleMaster? Role { get; set; }
}
