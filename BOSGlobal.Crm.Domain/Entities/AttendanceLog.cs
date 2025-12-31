using System.ComponentModel.DataAnnotations;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Domain.Entities;

public class AttendanceLog
{
    public Guid Id { get; set; }
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    public PunchType PunchType { get; set; }
    public DateTime PunchUtc { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public AttendanceStatus Status { get; set; }
    [MaxLength(128)]
    public string? ShiftAlias { get; set; }
    public TimeSpan? ShiftStart { get; set; }
    public TimeSpan? ShiftEnd { get; set; }
    public int? GraceMinutes { get; set; }
    public int? MeetingsLogged { get; set; }
    public int? TasksLogged { get; set; }
    public double? ActiveHours { get; set; }
    [MaxLength(256)]
    public string? Reason { get; set; }
    [MaxLength(256)]
    public string? LocationLabel { get; set; }
    public bool FlaggedForReview { get; set; }
    public DateTime CreatedUtc { get; set; }
}
