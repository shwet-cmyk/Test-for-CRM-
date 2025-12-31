using BOSGlobal.Crm.Application.DTOs.Attendance;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Entities;
using BOSGlobal.Crm.Domain.Enums;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly CrmDbContext _dbContext;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(CrmDbContext dbContext, ILogger<AttendanceService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PunchResponseDto> PunchAsync(string userId, PunchRequestDto request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var rules = await ResolveRulesAsync(userId, cancellationToken);
        var shift = await _dbContext.EmployeeShifts!.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        var geoZones = await _dbContext.GeoMappings!.Where(g => g.UserId == userId || g.UserId == null).ToListAsync(cancellationToken);

        var status = AttendanceStatus.Pending;
        var message = "Pending validation";
        var flagged = false;

        if (rules.RequireGeoValidation && !IsWithinGeoFence(request.Latitude, request.Longitude, geoZones, rules.GeoRadiusMeters))
        {
            status = AttendanceStatus.OutOfZone;
            message = "Not within allowed zone.";
            flagged = true;
        }
        else if (shift != null && !IsWithinShiftWindow(now, shift, rules.NightShiftRollover))
        {
            status = AttendanceStatus.OutsideShiftWindow;
            message = "Outside shift window.";
            flagged = true;
        }
        else
        {
            status = AttendanceStatus.Present;
            message = "Punch recorded.";
        }

        var log = new AttendanceLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PunchType = request.PunchType,
            PunchUtc = now,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = status,
            ShiftAlias = shift?.ShiftAlias,
            ShiftStart = shift?.StartTime,
            ShiftEnd = shift?.EndTime,
            GraceMinutes = shift?.GraceMinutes,
            Reason = status != AttendanceStatus.Present ? message : null,
            LocationLabel = request.LocationLabel,
            FlaggedForReview = flagged,
            CreatedUtc = now
        };

        _dbContext.AttendanceLogs!.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PunchResponseDto
        {
            Success = status == AttendanceStatus.Present,
            Status = status,
            Message = message,
            PunchUtc = now,
            Flagged = flagged
        };
    }

    public async Task<IEnumerable<AttendanceRuleDto>> GetRulesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var roleRules = await _dbContext.AttendanceRules!
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.EffectiveFromUtc)
            .ToListAsync(cancellationToken);

        return roleRules.Select(MapRule);
    }

    public async Task<AttendanceRuleDto> SaveRuleAsync(AttendanceRuleDto request, string adminUserId, CancellationToken cancellationToken = default)
    {
        ValidateRule(request);
        AttendanceRule entity;
        if (request.Id.HasValue)
        {
            entity = await _dbContext.AttendanceRules!.FirstAsync(r => r.Id == request.Id.Value, cancellationToken);
        }
        else
        {
            entity = new AttendanceRule
            {
                Id = Guid.NewGuid(),
                CreatedUtc = DateTime.UtcNow,
                EffectiveFromUtc = DateTime.UtcNow
            };
            _dbContext.AttendanceRules!.Add(entity);
        }

        entity.ScopeType = request.ScopeType;
        entity.ScopeValue = request.ScopeValue;
        entity.MinActiveHours = request.MinActiveHours;
        entity.MinMeetingsPerDay = request.MinMeetingsPerDay;
        entity.MinTasksPerDay = request.MinTasksPerDay;
        entity.HybridAllowed = request.HybridAllowed;
        entity.RequireGeoValidation = request.RequireGeoValidation;
        entity.GeoRadiusMeters = request.GeoRadiusMeters;
        entity.NightShiftRollover = request.NightShiftRollover;
        entity.ShiftAlias = request.ShiftAlias;
        entity.IsActive = request.IsActive;
        entity.UpdatedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapRule(entity);
    }

    public async Task OverrideAsync(string adminUserId, OverrideRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException("Override reason is required.");
        }

        var entity = new AttendanceOverride
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            EmployeeUserId = request.EmployeeUserId,
            AttendanceDate = request.AttendanceDate,
            Status = request.Status,
            Reason = request.Reason,
            CreatedUtc = DateTime.UtcNow
        };
        _dbContext.AttendanceOverrides!.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AttendanceRule> ResolveRulesAsync(string userId, CancellationToken cancellationToken)
    {
        var rule = await _dbContext.AttendanceRules!
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.EffectiveFromUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return rule ?? new AttendanceRule
        {
            ScopeType = "Global",
            ScopeValue = "*",
            MinActiveHours = 0,
            MinMeetingsPerDay = 0,
            MinTasksPerDay = 0,
            HybridAllowed = true,
            RequireGeoValidation = true,
            GeoRadiusMeters = 150,
            NightShiftRollover = false,
            EffectiveFromUtc = DateTime.UtcNow,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
    }

    private static bool IsWithinGeoFence(decimal lat, decimal lng, IEnumerable<GeoMapping> mappings, int defaultRadiusMeters)
    {
        foreach (var map in mappings)
        {
            var radius = map.RadiusMeters > 0 ? map.RadiusMeters : defaultRadiusMeters;
            if (HaversineMeters(lat, lng, map.Latitude, map.Longitude) <= radius)
            {
                return true;
            }
        }
        return !mappings.Any();
    }

    private static bool IsWithinShiftWindow(DateTime utcNow, EmployeeShift shift, bool nightRollover)
    {
        var today = utcNow.Date;
        var start = today.Add(shift.StartTime);
        var end = today.Add(shift.EndTime);
        if (nightRollover && shift.EndTime < shift.StartTime)
        {
            end = end.AddDays(1);
        }
        var graceEnd = end.AddMinutes(shift.GraceMinutes);
        return utcNow >= start.AddMinutes(-shift.GraceMinutes) && utcNow <= graceEnd;
    }

    private static double HaversineMeters(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        double R = 6371000; // meters
        var dLat = ToRad((double)(lat2 - lat1));
        var dLon = ToRad((double)(lon2 - lon1));
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad((double)lat1)) * Math.Cos(ToRad((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180.0;

    private static AttendanceRuleDto MapRule(AttendanceRule rule) => new AttendanceRuleDto
    {
        Id = rule.Id,
        ScopeType = rule.ScopeType,
        ScopeValue = rule.ScopeValue,
        MinActiveHours = rule.MinActiveHours,
        MinMeetingsPerDay = rule.MinMeetingsPerDay,
        MinTasksPerDay = rule.MinTasksPerDay,
        HybridAllowed = rule.HybridAllowed,
        RequireGeoValidation = rule.RequireGeoValidation,
        GeoRadiusMeters = rule.GeoRadiusMeters,
        NightShiftRollover = rule.NightShiftRollover,
        ShiftAlias = rule.ShiftAlias,
        IsActive = rule.IsActive
    };

    private static void ValidateRule(AttendanceRuleDto request)
    {
        if (request.MinActiveHours <= 0 && request.MinMeetingsPerDay <= 0 && request.MinTasksPerDay <= 0)
        {
            throw new InvalidOperationException("At least one rule requirement must be greater than zero.");
        }
    }
}
