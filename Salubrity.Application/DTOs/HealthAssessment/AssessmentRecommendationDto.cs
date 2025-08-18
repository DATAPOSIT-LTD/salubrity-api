
public sealed class AssessmentRecommendationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Priority { get; init; }
}
