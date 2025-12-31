using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Application.DTOs.Attendance;

public class PunchResponseDto
{
    public bool Success { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime PunchUtc { get; set; }
    public double? ActiveHours { get; set; }
    public int? MeetingsLogged { get; set; }
    public bool Flagged { get; set; }
}
