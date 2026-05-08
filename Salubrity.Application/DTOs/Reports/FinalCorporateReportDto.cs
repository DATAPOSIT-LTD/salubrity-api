// File: Application/DTOs/Reports/FinalCorporateReportDto.cs
namespace Salubrity.Application.DTOs.Reports;

public class FinalCorporateReportDto
{
    // Meta
    public Guid CampId { get; set; }
    public string CampName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientContactName { get; set; } = string.Empty;
    public string ClientContactEmail { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }

    // Sections
    public string Introduction { get; set; } = string.Empty;
    public ExecutiveSummaryGeneralOverviewDto ExecutiveSummaryGeneral { get; set; } = new();
    public ExecutiveSummaryDetailDto ExecutiveSummaryDetail { get; set; } = new();
    public string ObjectivesAndMethods { get; set; } = string.Empty;

    public ResultAtAGlanceDto ResultAtAGlance { get; set; } = new();
    public ParticipationByAgeDto ParticipationByAge { get; set; } = new();
    public LifestyleRiskDto LifestyleRiskOverall { get; set; } = new();
    public MetabolicNcdRiskDto MetabolicNcdRiskBars { get; set; } = new();
    public MentalHealthDto MentalHealth { get; set; } = new();
    public EyeVisualHealthDto EyeVisualHealth { get; set; } = new();
    public PainAssessmentDto PainAssessment { get; set; } = new();
    public LifestyleRiskDto LifestyleRiskSecondary { get; set; } = new();
    public SystemicOrganFunctionDto SystemicOrganFunction { get; set; } = new();
    public List<FinalTopFindingDto> TopCriticalClinicalFindings { get; set; } = new();
    public string TopCriticalFindingsSummary { get; set; } = string.Empty;

    public AnalysisOutlookDto AnalysisOutlook { get; set; } = new();
    public OutlookPredictionsDto OutlookPredictions { get; set; } = new();
    public TrendAnalysisSummaryDto TrendAnalysisSummary { get; set; } = new();

    public List<FinalRecommendationDto> Recommendations { get; set; } = new();
    public string Conclusion { get; set; } = string.Empty;
}

public class ExecutiveSummaryGeneralOverviewDto
{
    public string Overview { get; set; } = string.Empty;
    public string ClinicalFindings { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class ExecutiveSummaryDetailDto
{
    public string ParticipationCoverage { get; set; } = string.Empty;
    public string OverallHealthOfDisease { get; set; } = string.Empty;
    public string KeyRiskClusters { get; set; } = string.Empty;
}

public class ResultAtAGlanceDto
{
    public int ParticipationRate { get; set; }
    public double AbnormalFindingsPercent { get; set; }
    public int FollowUpPercent { get; set; }
    public int ParticipationRateSecondary { get; set; }
}

public class ParticipationByAgeDto
{
    public List<AgeBucketDto> Buckets { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

public class AgeBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int Female { get; set; }
    public int Male { get; set; }
}

public class LifestyleRiskDto
{
    public List<RiskSliceDto> Slices { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class RiskSliceDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class MetabolicNcdRiskDto
{
    public List<AgeBucketDto> Buckets { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

public class MentalHealthDto
{
    public int OverallScoreOutOfTen { get; set; }
    public string Band { get; set; } = "Good";
    public List<RiskSliceDto> Distribution { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class EyeVisualHealthDto
{
    public string LeftEye { get; set; } = "20/20";
    public string RightEye { get; set; } = "20/20";
    public string Summary { get; set; } = string.Empty;
}

public class PainAssessmentDto
{
    public List<RiskSliceDto> Male { get; set; } = new();
    public List<RiskSliceDto> Female { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

public class SystemicOrganFunctionDto
{
    public int NormalFunctionPercent { get; set; }
    public int RequiresMonitoringPercent { get; set; }
    public int AtRiskPercent { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class FinalTopFindingDto
{
    public string Code { get; set; } = string.Empty;       // ICD-10 (Phase 4)
    public string Name { get; set; } = string.Empty;
    public int N { get; set; }
    public int Pct { get; set; }
    public string Level { get; set; } = "low";              // low|med|high
}

public class AnalysisOutlookDto
{
    public string Comparator { get; set; } = "Metabolic & NCD Risk";
    public List<TrendSeriesDto> Series { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class TrendSeriesDto
{
    public string Year { get; set; } = string.Empty;
    public List<TrendPointDto> Points { get; set; } = new();
}

public class TrendPointDto
{
    public string XLabel { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class OutlookPredictionsDto
{
    public PredictionCardDto OverallHealth { get; set; } = new();
    public PredictionCardDto AbnormalFindings { get; set; } = new();
    public PredictionCardDto FollowUpRate { get; set; } = new();
}

public class PredictionCardDto
{
    public string Label { get; set; } = string.Empty;
    public string Delta { get; set; } = string.Empty;     // e.g. "+4.2%"
    public string Direction { get; set; } = "improving";  // improving|declining|stable
    public string Caption { get; set; } = string.Empty;
}

public class TrendAnalysisSummaryDto
{
    public string Improvements { get; set; } = string.Empty;
    public string Declines { get; set; } = string.Empty;
    public string StableAreas { get; set; } = string.Empty;
}

public class FinalRecommendationDto
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string ImplementationNote { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";       // Low|Medium|High
    public bool IsRiskBased { get; set; } = true;
}
