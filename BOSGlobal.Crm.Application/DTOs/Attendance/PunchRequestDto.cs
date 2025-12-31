using System.ComponentModel.DataAnnotations;
using BOSGlobal.Crm.Domain.Enums;

namespace BOSGlobal.Crm.Application.DTOs.Attendance;

public class PunchRequestDto
{
    [Required]
    public PunchType PunchType { get; set; }
    [Required]
    public decimal Latitude { get; set; }
    [Required]
    public decimal Longitude { get; set; }
    public string? LocationLabel { get; set; }
}
