namespace BOSGlobal.Crm.Domain.Entities;

public class RoleRight
{
    public int Id { get; set; }
    public int RoleMasterId { get; set; }
    public string PermissionKey { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}
