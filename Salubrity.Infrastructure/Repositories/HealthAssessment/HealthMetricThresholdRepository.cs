// Implementation: HealthMetricThresholdRepository


using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthAssesment;
using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Infrastructure.Persistence;


namespace Salubrity.Infrastructure.Repositories.HealthAssesment;


public class HealthMetricThresholdRepository : IHealthMetricThresholdRepository
{
    private readonly AppDbContext _db;


    public HealthMetricThresholdRepository(AppDbContext db)
    {
        _db = db;
    }


    public async Task<List<HealthMetricThreshold>> GetByMetricConfigIdAsync(Guid metricConfigId, CancellationToken ct = default)
    {
        return await _db.HealthMetricThresholds
        .Where(x => x.MetricConfigId == metricConfigId)
        .OrderBy(x => x.MinValue)
        .ToListAsync(ct);
    }


    public async Task<HealthMetricThreshold?> GetMatchingThresholdAsync(Guid metricConfigId, decimal value, CancellationToken ct = default)
    {
        return await _db.HealthMetricThresholds
        .Where(x => x.MetricConfigId == metricConfigId
        && x.MinValue <= value
        && x.MaxValue >= value)
        .FirstOrDefaultAsync(ct);
    }
}