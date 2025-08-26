public sealed class SubmitHealthAssessmentFormDto
{
    public Guid HealthAssessmentId { get; init; }
    public Guid FormTypeId { get; init; }
    public Guid? IntakeFormVersionId { get; init; }
    public IReadOnlyCollection<DynamicFieldResponseDto> DynamicResponses { get; init; } = Array.Empty<DynamicFieldResponseDto>();
}
