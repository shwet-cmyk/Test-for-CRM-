using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class AttendanceRule
{
    public Guid Id { get; set; }
    [MaxLength(128)]
    public string ScopeType { get; set; } = "Role"; // Role, Team, User
    [MaxLength(128)]
    public string ScopeValue { get; set; } = string.Empty;
    public double MinActiveHours { get; set; }
    public int MinMeetingsPerDay { get; set; }
    public int MinTasksPerDay { get; set; }
    public bool HybridAllowed { get; set; }
    public bool RequireGeoValidation { get; set; }
    public int GeoRadiusMeters { get; set; } = 150;
    public bool NightShiftRollover { get; set; }
    [MaxLength(128)]
    public string? ShiftAlias { get; set; }
    public DateTime EffectiveFromUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
