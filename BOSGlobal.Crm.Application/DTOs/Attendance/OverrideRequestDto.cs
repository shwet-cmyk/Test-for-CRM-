using System.ComponentModel.DataAnnotations;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Application.DTOs.Attendance;

public class OverrideRequestDto
{
    [Required]
    public string EmployeeUserId { get; set; } = string.Empty;
    [Required]
    public DateOnly AttendanceDate { get; set; }
    [Required]
    public AttendanceStatus Status { get; set; }
    [Required]
    public string Reason { get; set; } = string.Empty;
}
