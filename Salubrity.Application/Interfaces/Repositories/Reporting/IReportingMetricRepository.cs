using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Salubrity.Domain.Entities.Reporting;

namespace Salubrity.Application.Interfaces.Repositories.Reporting
{
    public interface IReportingMetricRepository
    {
        Task<List<ReportingMetricMapping>> ListAsync(CancellationToken ct);
        Task<ReportingMetricMapping?> GetByCodeAsync(string code, CancellationToken ct);
        Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
        Task AddAsync(ReportingMetricMapping entity, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
