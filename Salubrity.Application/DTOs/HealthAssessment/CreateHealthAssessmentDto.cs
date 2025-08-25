#nullable enable

public sealed class CreateHealthAssessmentDto
{
    public Guid HealthCampId { get; init; }
    public Guid ParticipantId { get; init; }
    public int OverallScore { get; init; }
    public Guid? ReviewedById { get; init; }

    public IReadOnlyCollection<CreateAssessmentMetricDto> Metrics { get; init; } = Array.Empty<CreateAssessmentMetricDto>();
    public IReadOnlyCollection<CreateAssessmentRecommendationDto> Recommendations { get; init; } = Array.Empty<CreateAssessmentRecommendationDto>();

    public Guid? IntakeFormVersionId { get; init; }
    public IReadOnlyCollection<DynamicFieldResponseDto> DynamicResponses { get; init; } = Array.Empty<DynamicFieldResponseDto>();
}
