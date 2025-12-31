using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class EmployeeShift
{
    public Guid Id { get; set; }
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    [MaxLength(128)]
    public string ShiftAlias { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int GraceMinutes { get; set; }
    [MaxLength(32)]
    public string WeeklyOffPattern { get; set; } = "Sun";
    public bool NightShiftRollover { get; set; }
    public DateTime CreatedUtc { get; set; }
}
