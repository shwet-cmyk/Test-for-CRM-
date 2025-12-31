namespace BOSGlobal.Crm.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();

    public bool IsActive { get; set; }
}
