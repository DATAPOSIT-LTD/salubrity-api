public class HealthMetricScoreResultDto
{
    public Guid MetricId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public int Score { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public Guid? StatusId { get; set; }
}