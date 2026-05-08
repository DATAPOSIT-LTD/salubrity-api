// File: Application/Interfaces/Repositories/Reporting/IFinalCorporateReportRepository.cs
using Salubrity.Application.DTOs.Reports;

namespace Salubrity.Application.Interfaces.Repositories.Reporting;

public interface IFinalCorporateReportRepository
{
    Task<List<AgeBucketDto>> GetAgeBucketsAsync(Guid campId, CancellationToken ct = default);
    Task<List<FinalRecommendationDto>> GetRecommendationsAsync(Guid campId, CancellationToken ct = default);
    Task<EyeVisualAggregateDto> GetEyeVisualHealthAsync(Guid campId, CancellationToken ct = default);
    Task<ExcoCategoryCountsDto> GetExcoCategoryCountsAsync(Guid campId, CorporateReportFilters? filters = null, CancellationToken ct = default);
    Task<List<AgeBucketDto>> GetAgeBucketsFilteredAsync(Guid campId, CorporateReportFilters? filters = null, CancellationToken ct = default);
}

public sealed class ExcoCategoryCountsDto
{
    public int TotalAttendees { get; set; }
    public int Vision { get; set; }
    public int Bp { get; set; }
    public int PreDiabetes { get; set; }
    public int MentalHealth { get; set; }
    public int Cdmp { get; set; }
}

public sealed class EyeVisualAggregateDto
{
    public string LeftEyeAcuity { get; set; } = "-";
    public string RightEyeAcuity { get; set; } = "-";
    public int LeftEyeCount { get; set; }
    public int RightEyeCount { get; set; }
    public bool HasData => LeftEyeCount > 0 || RightEyeCount > 0;
}
