public class HealthAssessmentScoreResultDto
{
    public int TotalScore { get; set; }
    public List<HealthMetricScoreResultDto> MetricScores { get; set; } = [];
}