// File: Application/Interfaces/Repositories/Reporting/ICorporateReportRepository.cs
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Interfaces.Repositories.Reporting;

public interface ICorporateReportRepository
{
    Task<CorporateRawDataDto?> LoadAsync(Guid campId, CorporateReportFilters? filters = null, CancellationToken ct = default);
}

/// <summary>
/// Flat snapshot of everything the CorporateReportService needs from the DB,
/// returned in one shot so the service stays free of EF references.
/// </summary>
public class CorporateRawDataDto
{
    public string CampName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ExpectedParticipants { get; set; }

    public int TotalAttendees { get; set; }
    public int Female { get; set; }
    public int Male { get; set; }

    public int TotalServices { get; set; }
    public List<StationCompletionDto> StationCompletion { get; set; } = new();
    public List<TopFindingDto> TopFindings { get; set; } = new();
}


public class CorporateReportFilters
{
    public string? Gender { get; set; }
    /// <summary>e.g. "18-30", "31-45", "46-60", "60+"</summary>
    public string? AgeBucket { get; set; }
    /// <summary>1-based day number relative to CampStartDate</summary>
    public int? Day { get; set; }
}
