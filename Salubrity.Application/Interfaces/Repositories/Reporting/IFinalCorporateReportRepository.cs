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

    // ── Data-integrity additions (P0 fix) ────────────────────────────────────
    Task<CampFollowUpCountsDto> GetFollowUpCountsAsync(Guid campId, CancellationToken ct = default);
    Task<int> GetAbnormalPatientCountAsync(Guid campId, CancellationToken ct = default);
    Task<List<RiskSliceDto>> GetLifestyleRiskDistributionAsync(Guid campId, CancellationToken ct = default);
    Task<CampMentalHealthCountsDto> GetMentalHealthCountsAsync(Guid campId, CancellationToken ct = default);
    Task<List<AgeBucketDto>> GetMetabolicNcdRiskAsync(Guid campId, CancellationToken ct = default);
}

public sealed class ExcoCategoryCountsDto
{
    public int TotalAttendees { get; set; }

    // Abnormal-finding counts (threshold-based, not station-visitor counts)
    public int Vision { get; set; }
    public int Bp { get; set; }
    public int PreDiabetes { get; set; }
    public int MentalHealth { get; set; }
    public int Cdmp { get; set; }

    // Measured-denominator counts (patients who had the test, regardless of result)
    public int VisionMeasured { get; set; }
    public int BpMeasured { get; set; }
    public int PreDiabetesMeasured { get; set; }
    public int MentalHealthMeasured { get; set; }
}

public sealed class EyeVisualAggregateDto
{
    public string LeftEyeAcuity { get; set; } = "-";
    public string RightEyeAcuity { get; set; } = "-";
    public int LeftEyeCount { get; set; }
    public int RightEyeCount { get; set; }
    public bool HasData => LeftEyeCount > 0 || RightEyeCount > 0;
}

public sealed class CampFollowUpCountsDto
{
    public int ReferredCount { get; set; }
    public int TotalAttendees { get; set; }
}

public sealed class CampMentalHealthCountsDto
{
    public int GoodCount { get; set; }
    public int AtRiskCount { get; set; }
    public int LowCount { get; set; }
    public int TotalMeasured { get; set; }
}
