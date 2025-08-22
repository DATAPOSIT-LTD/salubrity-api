// Interface: IHealthAssessmentScoringService


using Salubrity.Domain.Entities.HealthAssesment;

// Interface
public interface IHealthAssessmentScoringService
{
    Task<HealthAssessmentScoreResultDto> ScoreAsync(HealthAssessment assessment, CancellationToken ct = default);
}