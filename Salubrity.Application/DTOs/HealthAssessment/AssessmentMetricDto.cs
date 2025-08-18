
public sealed class AssessmentMetricDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal? Value { get; init; }
    public string? ReferenceRange { get; init; }
    public Guid? HealthMetricStatusId { get; init; }
    public string? HealthMetricStatusName { get; init; }
}
