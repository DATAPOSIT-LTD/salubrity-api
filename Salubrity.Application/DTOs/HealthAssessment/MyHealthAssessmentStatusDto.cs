namespace Salubrity.Application.DTOs.HealthAssessment;

/// <summary>
/// Status of the logged-in patient's self-assessment progress.
/// FormTypeIds + SectionIds reflect what currently has live (non-deleted)
/// responses; LastSubmittedAt is the most recent CreatedAt across them.
/// </summary>
public class MyHealthAssessmentStatusDto
{
    public List<Guid> FormTypeIdsSubmitted { get; set; } = new();
    public List<Guid> SectionIdsSubmitted { get; set; } = new();
    public int SectionsSubmittedCount { get; set; }
    public DateTime? LastSubmittedAt { get; set; }
}
