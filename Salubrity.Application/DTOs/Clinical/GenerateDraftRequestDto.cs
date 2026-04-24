// File: Application/DTOs/Clinical/GenerateDraftRequestDto.cs
namespace Salubrity.Application.DTOs.Clinical;

/// <summary>
/// Request body for POST /api/v1/doctor-recommendations/draft.
/// All doctor-input fields are optional — the service folds whatever is provided
/// into the Gemini prompt alongside auto-pulled findings.
/// </summary>
public class GenerateDraftRequestDto
{
    public Guid ParticipantId { get; set; }

    public string? PertinentHistoryFindings { get; set; }
    public string? PertinentClinicalFindings { get; set; }
    public string? DiagnosticImpression { get; set; }
    public string? Conclusion { get; set; }

    /// <summary>Display label of the selected follow-up recommendation (e.g. "Fit to work").</summary>
    public string? FollowUpRecommendation { get; set; }
    /// <summary>Display label of the selected recommendation type (e.g. "Normal Exam").</summary>
    public string? RecommendationType { get; set; }
}
