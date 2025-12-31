namespace BOSGlobal.Crm.Application.DTOs;

public class RoleAccessProfileDto
{
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
    public string? DashboardPath { get; set; }
    public string? WelcomeMessage { get; set; }
    public bool HasAssignedRole { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; }
    public string? ErrorMessage { get; set; }
}
