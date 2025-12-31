using BOSGlobal.Crm.Application.DTOs.Attendance;

namespace BOSGlobal.Crm.Application.Interfaces;

public interface IAttendanceService
{
    Task<PunchResponseDto> PunchAsync(string userId, PunchRequestDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRuleDto>> GetRulesAsync(string userId, CancellationToken cancellationToken = default);
    Task<AttendanceRuleDto> SaveRuleAsync(AttendanceRuleDto request, string adminUserId, CancellationToken cancellationToken = default);
    Task OverrideAsync(string adminUserId, OverrideRequestDto request, CancellationToken cancellationToken = default);
}
