using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Application.DTOs.Attendance;

public class AttendanceRuleDto
{
    public Guid? Id { get; set; }
    [Required]
    public string ScopeType { get; set; } = "Role";
    [Required]
    public string ScopeValue { get; set; } = string.Empty;
    public double MinActiveHours { get; set; }
    public int MinMeetingsPerDay { get; set; }
    public int MinTasksPerDay { get; set; }
    public bool HybridAllowed { get; set; }
    public bool RequireGeoValidation { get; set; }
    public int GeoRadiusMeters { get; set; } = 150;
    public bool NightShiftRollover { get; set; }
    public string? ShiftAlias { get; set; }
    public bool IsActive { get; set; } = true;
}
