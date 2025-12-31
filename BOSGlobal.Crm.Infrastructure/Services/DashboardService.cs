using System.Security.Cryptography;
using System.Text;
using BOSGlobal.Crm.Application.DTOs.Dashboard;
using BOSGlobal.Crm.Application.Interfaces;
using BOSGlobal.Crm.Domain.Entities;
using BOSGlobal.Crm.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly CrmDbContext _dbContext;
    private readonly IRoleAccessService _roleAccessService;
    private readonly ILogger<DashboardService> _logger;

    private static readonly Dictionary<string, string[]> DefaultRoleWidgets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = new[] { "leads-over-time", "conversion-rate", "lead-aging", "attendance-compliance", "geo-checkin" },
        ["Manager"] = new[] { "leads-over-time", "conversion-rate", "lead-aging", "funnel-stage", "incentive-booster" },
        ["SalesExecutive"] = new[] { "leads-over-time", "lead-aging", "followup-analysis", "funnel-stage" },
        ["Marketing"] = new[] { "leads-over-time", "conversion-rate", "campaign-performance", "lead-source" }
    };

    private static readonly WidgetLibrary[] SeedWidgets =
    [
        CreateWidget("leads-over-time", "Leads Captured Over Time", "Leads", "Line", "LeadsDataSource"),
        CreateWidget("conversion-rate", "Conversion Rate by Time", "Leads", "Bar", "ConversionsDataSource"),
        CreateWidget("lead-aging", "Lead Aging Breakdown", "Leads", "Pie", "LeadAgingDataSource"),
        CreateWidget("attendance-compliance", "Attendance Compliance", "Workforce", "Bar", "AttendanceDataSource"),
        CreateWidget("shift-adherence", "Shift Adherence", "Workforce", "Line", "AttendanceDataSource"),
        CreateWidget("geo-checkin", "Geo Check-In Map", "Workforce", "Map", "GeoCheckinDataSource"),
        CreateWidget("followup-analysis", "Follow-Up Type Analysis", "Leads", "Bar", "FollowupDataSource"),
        CreateWidget("funnel-stage", "Funnel Stage Progress", "Pipeline", "Funnel", "PipelineDataSource"),
        CreateWidget("incentive-booster", "Incentive Booster Status", "Compensation", "Bar", "IncentiveDataSource"),
        CreateWidget("campaign-performance", "Campaign Performance", "Marketing", "Bar", "CampaignDataSource"),
        CreateWidget("lead-source", "Lead Source Integration", "Marketing", "Pie", "LeadSourceDataSource")
    ];

    public DashboardService(CrmDbContext dbContext, IRoleAccessService roleAccessService, ILogger<DashboardService> logger)
    {
        _dbContext = dbContext;
        _roleAccessService = roleAccessService;
        _logger = logger;
    }

    public async Task<DashboardLayoutResponse> GetDashboardAsync(string userId, CancellationToken cancellationToken = default)
    {
        await EnsureSeedWidgetsAsync(cancellationToken);

        var configs = await _dbContext.UserDashboardConfigs!
            .Where(c => c.UserId == userId && !c.IsRemoved)
            .OrderBy(c => c.OrderIndex)
            .ToListAsync(cancellationToken);

        var library = await _dbContext.WidgetLibraries!.Where(w => w.IsEnabled).ToListAsync(cancellationToken);

        var usedDefault = false;
        if (configs.Count == 0)
        {
            usedDefault = true;
            var profile = await _roleAccessService.BuildAccessProfileAsync(userId, cancellationToken);
            var widgetsForRoles = profile.Roles?.SelectMany(r => DefaultRoleWidgets.TryGetValue(r, out var set) ? set : Array.Empty<string>()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
            if (widgetsForRoles.Count == 0)
            {
                widgetsForRoles.Add("leads-over-time");
            }

            configs = widgetsForRoles.Select((key, idx) =>
            {
                var lib = library.FirstOrDefault(l => l.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                return new UserDashboardConfig
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    WidgetId = lib?.Id ?? Guid.Empty,
                    OrderIndex = idx,
                    LayoutJson = null,
                    FiltersJson = null,
                    IsRemoved = false,
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow
                };
            }).ToList();
        }

        var widgets = configs
            .Select(c =>
            {
                var lib = library.FirstOrDefault(l => l.Id == c.WidgetId);
                if (lib == null)
                {
                    return null;
                }

                return new DashboardWidgetDto
                {
                    WidgetId = c.WidgetId,
                    Title = c.TitleOverride ?? lib.Title,
                    Category = lib.Category,
                    GraphType = lib.GraphType,
                    DataSource = lib.DataSource,
                    LayoutJson = c.LayoutJson ?? lib.DefaultConfigJson,
                    FiltersJson = c.FiltersJson ?? lib.DefaultFiltersJson,
                    OrderIndex = c.OrderIndex,
                    FromRoleDefault = usedDefault
                };
            })
            .Where(w => w != null)
            .ToList()!;

        return new DashboardLayoutResponse
        {
            Widgets = widgets,
            GlobalFilters = Array.Empty<string>(),
            UsedDefaultLayout = usedDefault
        };
    }

    public async Task<DashboardWidgetDto> SaveWidgetAsync(string userId, SaveWidgetConfigRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureSeedWidgetsAsync(cancellationToken);
        var libraryItem = await _dbContext.WidgetLibraries!.FirstOrDefaultAsync(w => w.Id == request.WidgetId && w.IsEnabled, cancellationToken)
            ?? throw new InvalidOperationException("Widget not found or disabled.");

        var existing = await _dbContext.UserDashboardConfigs!.FirstOrDefaultAsync(c => c.UserId == userId && c.WidgetId == request.WidgetId, cancellationToken);
        if (existing is null)
        {
            existing = new UserDashboardConfig
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WidgetId = request.WidgetId,
                CreatedUtc = DateTime.UtcNow,
                OrderIndex = request.OrderIndex ?? 0
            };
            _dbContext.UserDashboardConfigs.Add(existing);
        }

        existing.TitleOverride = request.Title;
        existing.LayoutJson = request.LayoutJson;
        existing.FiltersJson = request.FiltersJson;
        existing.IsRemoved = false;
        existing.OrderIndex = request.OrderIndex ?? existing.OrderIndex;
        existing.UpdatedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DashboardWidgetDto
        {
            WidgetId = existing.WidgetId,
            Title = existing.TitleOverride ?? libraryItem.Title,
            Category = libraryItem.Category,
            GraphType = libraryItem.GraphType,
            DataSource = libraryItem.DataSource,
            LayoutJson = existing.LayoutJson ?? libraryItem.DefaultConfigJson,
            FiltersJson = existing.FiltersJson ?? libraryItem.DefaultFiltersJson,
            OrderIndex = existing.OrderIndex,
            FromRoleDefault = false
        };
    }

    public async Task RemoveWidgetAsync(string userId, Guid widgetId, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.UserDashboardConfigs!.FirstOrDefaultAsync(c => c.UserId == userId && c.WidgetId == widgetId, cancellationToken);
        if (existing != null)
        {
            existing.IsRemoved = true;
            existing.UpdatedUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<string> GetWidgetDataAsync(string userId, Guid widgetId, string? filtersJson, CancellationToken cancellationToken = default)
    {
        var cacheKey = ComputeHash(widgetId, filtersJson ?? string.Empty);
        var now = DateTime.UtcNow;
        var cached = await _dbContext.WidgetDataCaches!
            .FirstOrDefaultAsync(c => c.WidgetId == widgetId && c.UserId == userId && c.FiltersHash == cacheKey && c.ExpiresUtc > now, cancellationToken);

        if (cached != null)
        {
            return cached.DataJson;
        }

        var data = BuildStubData(widgetId);
        var entity = new WidgetDataCache
        {
            Id = Guid.NewGuid(),
            WidgetId = widgetId,
            UserId = userId,
            FiltersHash = cacheKey,
            DataJson = data,
            CachedUtc = now,
            ExpiresUtc = now.AddMinutes(5)
        };
        _dbContext.WidgetDataCaches!.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return data;
    }

    private static WidgetLibrary CreateWidget(string key, string title, string category, string graphType, string dataSource)
    {
        return new WidgetLibrary
        {
            Id = Guid.NewGuid(),
            Key = key,
            Title = title,
            Category = category,
            GraphType = graphType,
            DataSource = dataSource,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
    }

    private async Task EnsureSeedWidgetsAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.WidgetLibraries!.AnyAsync(cancellationToken))
        {
            return;
        }

        _dbContext.WidgetLibraries!.AddRange(SeedWidgets);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeHash(Guid widgetId, string filters)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{widgetId}:{filters}"));
        return Convert.ToHexString(bytes);
    }

    private static string BuildStubData(Guid widgetId)
    {
        return """{"series":[{"name":"Value","data":[10,20,30]}]}""";
    }
}
