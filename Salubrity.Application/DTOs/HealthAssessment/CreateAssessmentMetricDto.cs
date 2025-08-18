
public sealed class CreateAssessmentMetricDto
{
    public string Name { get; init; } = string.Empty;
    public decimal? Value { get; init; }
    public string? ReferenceRange { get; init; }
    public Guid? HealthMetricStatusId { get; init; }
}
