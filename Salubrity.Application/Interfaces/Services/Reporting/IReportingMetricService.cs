using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Salubrity.Application.DTOs.Reporting;

namespace Salubrity.Application.Interfaces.Services.Reporting
{
    public interface IReportingMetricService
    {
        Task<List<ReportingMetricDto>> ListAsync(CancellationToken ct);
        Task<ReportingMetricDto> CreateAsync(CreateReportingMetricDto dto, CancellationToken ct);
        Task<ReportingMetricDto> GetByCodeAsync(string metricCode, CancellationToken ct);
    }
}
