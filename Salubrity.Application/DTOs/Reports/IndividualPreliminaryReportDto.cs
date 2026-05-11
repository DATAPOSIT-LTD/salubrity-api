namespace Salubrity.Application.DTOs.Reports;

/// <summary>
/// Top-level payload returned by GET /api/v1/reports/individual-preliminary/{participantId}.
/// Drives the entire Individual Preliminary Report layout (preview + PDF).
/// </summary>
public class IndividualPreliminaryReportDto
{
    public PatientDemographicsDto Demographics { get; set; } = new();
    public string CampName { get; set; } = string.Empty;
    public string CampDate { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }

    public ResultsAtGlanceDto ResultsAtGlance { get; set; } = new();
    public List<string> AbnormalFindings { get; set; } = new();
    public List<string> BorderlineFindings { get; set; } = new();

    public List<ServiceSectionDto> ServiceSections { get; set; } = new();

    public GeneralHealthScoreDto GeneralHealthScore { get; set; } = new();
    public List<RiskBarDto> RiskBars { get; set; } = new();

    /// <summary>True when every assigned station for this camp participant has Status == Completed.</summary>
    public bool AllStationsCompleted { get; set; }
    public int StationsCompletedCount { get; set; }
    public int StationsTotalCount { get; set; }
    /// <summary>Names of stations that are not yet Completed (Not served / Ongoing / Cancelled).</summary>
    public List<string> PendingStationNames { get; set; } = new();
}

public class PatientDemographicsDto
{
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = "—";
    public DateTime? DateOfBirth { get; set; }
    public string Phone { get; set; } = "—";
    public string Email { get; set; } = "—";
    public string Branch { get; set; } = "—";
    public string Nationality { get; set; } = "—";
}

public class ResultsAtGlanceDto
{
    public int ParametersTested { get; set; }
    public int NormalCount { get; set; }
    public int BorderlineCount { get; set; }
    public int AbnormalCount { get; set; }
}

public class ServiceSectionDto
{
    public string ServiceName { get; set; } = string.Empty;
    /// <summary>Hint used by the frontend to pick an icon (heart, eye, tooth, brain, etc.).</summary>
    public string IconKey { get; set; } = "default";
    public List<MetricDto> Metrics { get; set; } = new();
    /// <summary>Doctor / system summary text. Empty for now (Phase 3+).</summary>
    public string Summary { get; set; } = string.Empty;
}

public class MetricDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    /// <summary>"Normal", "Borderline", or "Abnormal".</summary>
    public string Status { get; set; } = "Normal";
}

public class GeneralHealthScoreDto
{
    public int Score { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RiskBarDto
{
    public string Name { get; set; } = string.Empty;
    /// <summary>"VeryLow", "Low", "Medium", "High", "VeryHigh".</summary>
    public string Level { get; set; } = "Medium";
    public string Value { get; set; } = string.Empty;
}
