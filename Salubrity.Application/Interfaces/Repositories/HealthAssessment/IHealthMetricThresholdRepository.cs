// Interface: IHealthMetricThresholdRepository


using Salubrity.Domain.Entities.HealthAssesment;


namespace Salubrity.Application.Interfaces.Repositories.HealthAssesment;


public interface IHealthMetricThresholdRepository
{
    Task<List<HealthMetricThreshold>> GetByMetricConfigIdAsync(Guid metricConfigId, CancellationToken ct = default);


    Task<HealthMetricThreshold?> GetMatchingThresholdAsync(Guid metricConfigId, decimal value, CancellationToken ct = default);
}