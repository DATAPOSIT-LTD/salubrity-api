// File: Application/Interfaces/Services/Reporting/IExcoCorporateReportService.cs
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Interfaces.Services.Reporting;

public interface IExcoCorporateReportService
{
    Task<ExcoCorporateReportDto> BuildAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default);
    Task<byte[]> BuildPdfAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, IEnumerable<string>? excludedSections = null, CancellationToken ct = default);
    Task SendEmailAsync(Guid campId, SendCorporateReportEmailRequest request, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, IEnumerable<string>? excludedSections = null, CancellationToken ct = default);
}
