
public class HealthAssessmentDto
{
    public Guid Id { get; init; }
    public Guid HealthCampId { get; init; }
    public Guid ParticipantId { get; init; }
    public int OverallScore { get; init; }
    public ReviewerDto? ReviewedBy { get; init; }
    public IReadOnlyCollection<AssessmentMetricDto> Metrics { get; init; } = Array.Empty<AssessmentMetricDto>();
    public IReadOnlyCollection<AssessmentRecommendationDto> Recommendations { get; init; } = Array.Empty<AssessmentRecommendationDto>();
}
