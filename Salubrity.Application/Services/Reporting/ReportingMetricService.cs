using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Salubrity.Application.DTOs.Reporting;
using Salubrity.Application.Interfaces.Repositories.Reporting;
using Salubrity.Application.Interfaces.Services.Reporting;
using Salubrity.Domain.Entities.Reporting;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Reporting
{
    public class ReportingMetricService : IReportingMetricService
    {
        private readonly IReportingMetricRepository _repo;

        public ReportingMetricService(IReportingMetricRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ReportingMetricDto>> ListAsync(CancellationToken ct)
        {
            var data = await _repo.ListAsync(ct);

            return data.Select(x => new ReportingMetricDto
            {
                Id = x.Id,
                FieldId = x.FieldId,
                MetricCode = x.MetricCode,
                Label = x.Label,
                DataType = x.DataType
            }).ToList();
        }

        public async Task<ReportingMetricDto> CreateAsync(CreateReportingMetricDto dto, CancellationToken ct)
        {
            if (await _repo.ExistsByCodeAsync(dto.MetricCode, ct))
                throw new ValidationException(["Metric code already exists."]);

            var entity = new ReportingMetricMapping
            {
                FieldId = dto.FieldId,
                MetricCode = dto.MetricCode,
                Label = dto.Label,
                DataType = dto.DataType
            };

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);

            return new ReportingMetricDto
            {
                Id = entity.Id,
                FieldId = entity.FieldId,
                MetricCode = entity.MetricCode,
                Label = entity.Label,
                DataType = entity.DataType
            };
        }

        public async Task<ReportingMetricDto> GetByCodeAsync(string metricCode, CancellationToken ct)
        {
            var entity = await _repo.GetByCodeAsync(metricCode, ct);
            if (entity == null)
                throw new NotFoundException("ReportingMetric", metricCode);

            return new ReportingMetricDto
            {
                Id = entity.Id,
                FieldId = entity.FieldId,
                MetricCode = entity.MetricCode,
                Label = entity.Label,
                DataType = entity.DataType
            };
        }
    }
}
