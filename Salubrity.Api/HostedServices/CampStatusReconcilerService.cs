// File: Salubrity.Api/HostedServices/CampStatusReconcilerService.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Api.HostedServices;

/// <summary>
/// Periodically reconciles HealthCamp.HealthCampStatusId against today's date so the
/// Upcoming / Ongoing / Completed buckets stay accurate without manual intervention.
/// Manually-managed statuses (Suspended, Incomplete, Ready) are left untouched.
/// Runs once at startup, then every hour.
/// </summary>
public sealed class CampStatusReconcilerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CampStatusReconcilerService> _log;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public CampStatusReconcilerService(IServiceProvider services, ILogger<CampStatusReconcilerService> log)
    {
        _services = services;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial run on startup, then every Interval.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Camp status reconciliation failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task ReconcileOnceAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sql = @"
WITH lookup AS (
  SELECT ""Id"", ""Name"" FROM ""HealthCampStatuses"" WHERE ""Name"" IN ('Upcoming','Ongoing','Completed')
),
derived AS (
  SELECT c.""Id"" AS camp_id,
    CASE
      WHEN c.""StartDate""::date > CURRENT_DATE THEN 'Upcoming'
      WHEN c.""EndDate"" IS NOT NULL AND c.""EndDate""::date < CURRENT_DATE THEN 'Completed'
      WHEN c.""StartDate""::date <= CURRENT_DATE
        AND (c.""EndDate"" IS NULL OR c.""EndDate""::date >= CURRENT_DATE) THEN 'Ongoing'
    END AS derived_name
  FROM ""HealthCamps"" c
  JOIN ""HealthCampStatuses"" s ON s.""Id"" = c.""HealthCampStatusId""
  WHERE s.""Name"" IN ('Upcoming','Ongoing','Completed')
)
UPDATE ""HealthCamps"" c
SET ""HealthCampStatusId"" = l.""Id"",
    ""UpdatedAt"" = NOW()
FROM derived d
JOIN lookup l ON l.""Name"" = d.derived_name
WHERE c.""Id"" = d.camp_id
  AND c.""HealthCampStatusId"" <> l.""Id"";
";

        var updated = await db.Database.ExecuteSqlRawAsync(sql, ct);
        if (updated > 0)
            _log.LogInformation("Camp status reconciler updated {Count} camps.", updated);
    }
}
