using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Reporting;
using Salubrity.Domain.Entities.Reporting;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Reporting
{
    public class ReportingMetricRepository : IReportingMetricRepository
    {
        private readonly AppDbContext _db;

        public ReportingMetricRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ReportingMetricMapping>> ListAsync(CancellationToken ct)
        {
            return await _db.ReportingMetricMappings.AsNoTracking().ToListAsync(ct);
        }

        public async Task<ReportingMetricMapping?> GetByCodeAsync(string code, CancellationToken ct)
        {
            return await _db.ReportingMetricMappings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MetricCode == code, ct);
        }

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct)
        {
            return await _db.ReportingMetricMappings.AnyAsync(x => x.MetricCode == code, ct);
        }

        public async Task AddAsync(ReportingMetricMapping entity, CancellationToken ct)
        {
            await _db.ReportingMetricMappings.AddAsync(entity, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
