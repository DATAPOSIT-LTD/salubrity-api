// File: Application/DTOs/Reports/ExcoCorporateReportDto.cs
namespace Salubrity.Application.DTOs.Reports;

public class ExcoCorporateReportDto
{
    // Meta
    public Guid CampId { get; set; }
    public string CampName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientContactName { get; set; } = string.Empty;
    public string ClientContactEmail { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }

    // Six KPI cards
    public ExcoKpiDto CampEngagementTurnout { get; set; } = new();
    public ExcoKpiDto VisionProductivity { get; set; } = new();
    public ExcoKpiDto CardiometabolicHighBp { get; set; } = new();
    public ExcoKpiDto CareNavigationCdmp { get; set; } = new();
    public ExcoKpiDto CardiometabolicPreDiabetes { get; set; } = new();
    public ExcoKpiDto MentalHealthFlags { get; set; } = new();

    // 1. One-Page Wellness Headlines for EXCO
    public ExcoWellnessHeadlinesDto WellnessHeadlines { get; set; } = new();

    // 2. Strategic Risk Snapshot
    public ExcoStrategicRiskSnapshotDto StrategicRiskSnapshot { get; set; } = new();

    // 3. Early positives and strengths to build on
    public ExcoEarlyPositivesDto EarlyPositives { get; set; } = new();
}

public class ExcoKpiDto
{
    public string Title { get; set; } = string.Empty;
    public string Percentage { get; set; } = "0%";
    public string Attendance { get; set; } = "0/0";
    public string Trend { get; set; } = "Stable";    // Increase | Decrease | Stable
    public string Notes { get; set; } = string.Empty; // Gemini-generated
}

public class ExcoWellnessHeadlinesDto
{
    public string Engagement { get; set; } = string.Empty;
    public string HealthRiskBurden { get; set; } = string.Empty;
    public string RiskIdentificationAndTriage { get; set; } = string.Empty;
    public string HealthRiskBurdenSecondary { get; set; } = string.Empty;
}

public class ExcoStrategicRiskSnapshotDto
{
    public string ParticipationSummary { get; set; } = string.Empty;
    public string AgeProfileSummary { get; set; } = string.Empty;
    public string HighestBurdenDomains { get; set; } = string.Empty; // formatted bullet text
    public string FutureMedicalCosts { get; set; } = string.Empty;
    public string PresenteeismAndOutput { get; set; } = string.Empty;
    public string SafetyAndQualityRisk { get; set; } = string.Empty;
    public string EmployerBrandRisk { get; set; } = string.Empty;
}

public class ExcoEarlyPositivesDto
{
    public string HigherFutureMedicalCosts { get; set; } = string.Empty;
    public string AgeProfile { get; set; } = string.Empty;
}
