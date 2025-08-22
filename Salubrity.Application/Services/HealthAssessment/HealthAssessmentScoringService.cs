using Salubrity.Application.Interfaces.Repositories.HealthAssesment;

namespace Salubrity.Application.Services.HealthAssessment;

public class HealthAssessmentScoringService : IHealthAssessmentScoringService
{
    private readonly IHealthMetricThresholdRepository _thresholdRepo;

    public HealthAssessmentScoringService(IHealthMetricThresholdRepository thresholdRepo)
    {
        _thresholdRepo = thresholdRepo;
    }

    public async Task<HealthAssessmentScoreResultDto> ScoreAsync(Salubrity.Domain.Entities.HealthAssesment.HealthAssessment assessment, CancellationToken ct = default)
    {
        var result = new HealthAssessmentScoreResultDto();

        foreach (var metric in assessment.Metrics)
        {
            var dto = new HealthMetricScoreResultDto
            {
                MetricId = metric.Id,
                Name = metric.Name,
                Value = metric.Value,
                Score = 0,
                StatusLabel = "Unknown"
            };

            if (metric.Config != null && metric.Value.HasValue)
            {
                var thresholds = metric.Config.Thresholds?.ToList();

                if (thresholds is { Count: > 0 })
                {
                    var matched = thresholds.FirstOrDefault(t =>
                        metric.Value >= t.MinValue && metric.Value <= t.MaxValue);

                    if (matched != null)
                    {
                        dto.Score = matched.Score;
                        dto.StatusLabel = matched.StatusLabel;
                        dto.StatusId = matched.StatusId;
                    }
                }
            }

            result.MetricScores.Add(dto);
            result.TotalScore += dto.Score;
        }

        return result;
    }
}
