using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BOSGlobal.Crm.Infrastructure.Services;

public class AttendanceComplianceJob : BackgroundService
{
    private readonly ILogger<AttendanceComplianceJob> _logger;

    public AttendanceComplianceJob(ILogger<AttendanceComplianceJob> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Placeholder: recalculate attendance compliance for missed validations.
                _logger.LogInformation("Running attendance compliance job at {Time}", DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Attendance compliance job failed.");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
