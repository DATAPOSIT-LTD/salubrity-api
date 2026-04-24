// File: Application/Interfaces/Services/Reporting/ICorporateReportService.cs
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Interfaces.Services.Reporting;

public interface ICorporateReportService
{
    /// <summary>
    /// Builds the corporate-level report payload for an admin: KPIs, gender split,
    /// per-station completion, and AI-generated narratives.
    /// </summary>
    Task<CorporateReportDto> BuildAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default);
}
