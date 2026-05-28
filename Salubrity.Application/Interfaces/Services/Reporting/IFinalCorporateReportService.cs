// File: Application/Interfaces/Services/Reporting/IFinalCorporateReportService.cs
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Interfaces.Services.Reporting;

public interface IFinalCorporateReportService
{
    Task<FinalCorporateReportDto> BuildAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default);
    Task<byte[]> BuildPdfAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default);
    Task SendEmailAsync(Guid campId, SendCorporateReportEmailRequest request, CancellationToken ct = default);
}
