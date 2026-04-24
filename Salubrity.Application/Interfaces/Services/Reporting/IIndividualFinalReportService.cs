// File: Application/Interfaces/Services/Reporting/IIndividualFinalReportService.cs
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Interfaces.Services.Reporting;

public interface IIndividualFinalReportService
{
    Task<IndividualFinalReportDto> BuildAsync(Guid participantId, CancellationToken ct = default);

    Task<byte[]> BuildPdfAsync(Guid participantId, CancellationToken ct = default);
}
