namespace Salubrity.Application.DTOs.Clinical;

/// <summary>
/// Request body for POST /api/v1/doctor-recommendations/draft-field.
/// Generates ONE of the four Doctor's Review textareas. Pass the other
/// three values as context so the AI stays consistent with what the
/// doctor already wrote.
/// </summary>
public class GenerateClinicalFieldDraftRequestDto
{
    public Guid ParticipantId { get; set; }

    /// <summary>One of: history, clinical, diagnostic, conclusion.</summary>
    public string Field { get; set; } = string.Empty;

    public string? PertinentHistoryFindings { get; set; }
    public string? PertinentClinicalFindings { get; set; }
    public string? DiagnosticImpression { get; set; }
    public string? Conclusion { get; set; }
}
