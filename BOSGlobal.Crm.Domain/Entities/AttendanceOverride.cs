using System.ComponentModel.DataAnnotations;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Domain.Entities;

public class AttendanceOverride
{
    public Guid Id { get; set; }
    [MaxLength(450)]
    public string AdminUserId { get; set; } = string.Empty;
    [MaxLength(450)]
    public string EmployeeUserId { get; set; } = string.Empty;
    public DateOnly AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    [MaxLength(512)]
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}
