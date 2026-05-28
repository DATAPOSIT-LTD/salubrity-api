// File: Application/DTOs/Reports/IndividualFinalReportDto.cs
using Salubrity.Application.DTOs.Lookups;

namespace Salubrity.Application.DTOs.Reports;

/// <summary>
/// Full Individual Final Report payload. Inherits every field of the Preliminary
/// report and adds doctor recommendations, referrals, body-map data, and signature.
/// </summary>
public class IndividualFinalReportDto : IndividualPreliminaryReportDto
{
    /// <summary>Null when no doctor has reviewed this participant+camp yet.</summary>
    public FinalReportDoctorRecommendationDto? DoctorRecommendation { get; set; }

    public List<FinalReportReferralDto> Referrals { get; set; } = new();

    /// <summary>One entry per service station the participant went through.</summary>
    public List<BodyMapEntryDto> BodyMap { get; set; } = new();

    public ReportSignatureDto Signature { get; set; } = new();
}

public class FinalReportDoctorRecommendationDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;

    public string? PertinentHistoryFindings { get; set; }
    public string? PertinentClinicalFindings { get; set; }
    public string? DiagnosticImpression { get; set; }
    public string? Conclusion { get; set; }
    public string? Instructions { get; set; }

    public BaseLookupResponse? FollowUpRecommendation { get; set; }
    public BaseLookupResponse? RecommendationType { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class FinalReportReferralDto
{
    public Guid Id { get; set; }
    public Guid ServiceAssignmentId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceProviderName { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public BaseLookupResponse Urgency { get; set; } = default!;
    public BaseLookupResponse FollowUpSchedule { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class BodyMapEntryDto
{
    public string ServiceName { get; set; } = string.Empty;
    /// <summary>Hint used by the frontend to pick an icon / position on the silhouette.</summary>
    public string IconKey { get; set; } = "default";
    /// <summary>"Normal", "Borderline", or "Abnormal" — worst status across the section's metrics.</summary>
    public string Status { get; set; } = "Normal";
    /// <summary>Clinical conclusion (e.g. "Hypertension", "Obesity") derived from the section's metrics. Falls back to Status when no specific term matches.</summary>
    public string Conclusion { get; set; } = "Normal";
}

public class ReportSignatureDto
{
    public Guid? PreparedById { get; set; }
    public string PreparedByName { get; set; } = string.Empty;
    public DateTime? PreparedAt { get; set; }
}
