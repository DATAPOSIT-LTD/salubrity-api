// File: Application/DTOs/Reports/CorporateReportDto.cs
namespace Salubrity.Application.DTOs.Reports;

public class CorporateReportDto
{
    public Guid CampId { get; set; }
    public Guid? OrganizationId { get; set; }
    public string CampName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public DateTime CampStartDate { get; set; }
    public DateTime? CampEndDate { get; set; }
    public DateTime GeneratedAt { get; set; }

    // KPIs
    public int ParticipationRate { get; set; }
    public int TotalAttendees { get; set; }
    public int TotalServices { get; set; }
    public int ParticipationTrend { get; set; }

    // Charts
    public CorporateAttendanceDto Attendance { get; set; } = new();
    public List<StationCompletionDto> StationCompletion { get; set; } = new();
    public List<TopFindingDto> TopFindings { get; set; } = new();

    // AI-generated narratives
    public CorporateNarrativesDto Narratives { get; set; } = new();

    // Cardiometabolic snapshot: average BP, BMI distribution, RBS bands.
    public CardiometabolicSnapshotDto Cardiometabolic { get; set; } = new();
}

public class CorporateAttendanceDto
{
    public int Female { get; set; }
    public int Male { get; set; }
    public int FemalePercent { get; set; }
    public int MalePercent { get; set; }
}

public class StationCompletionDto
{
    public string Name { get; set; } = string.Empty;
    public int Female { get; set; }
    public int Male { get; set; }
}

public class CorporateNarrativesDto
{
    public string Overview { get; set; } = string.Empty;
    public string ClinicalFindings { get; set; } = string.Empty;
    public string AttendanceNotes { get; set; } = string.Empty;
    public string CriticalFindingsNotes { get; set; } = string.Empty;
    public string StationSummary { get; set; } = string.Empty;
    public string TopFindingsSummary { get; set; } = string.Empty;
    public string OverallConclusion { get; set; } = string.Empty;
    public string WhatHappensNext { get; set; } = string.Empty;
}


public class TopFindingDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int N { get; set; }
    public int Pct { get; set; }
    /// <summary>"low", "med" or "high" — used by the frontend to color the bar.</summary>
    public string Level { get; set; } = "low";
}


public class CardiometabolicSnapshotDto
{
    /// <summary>Mean systolic BP (mmHg) across all readings in the camp.</summary>
    public int AverageSystolic { get; set; }
    /// <summary>Mean diastolic BP (mmHg) across all readings in the camp.</summary>
    public int AverageDiastolic { get; set; }
    public int BloodPressureSamples { get; set; }

    /// <summary>BMI distribution counts (kg/m²): <18.5 / 18.5-24.9 / 25-29.9 / >=30.</summary>
    public int BmiUnderweight { get; set; }
    public int BmiNormal { get; set; }
    public int BmiOverweight { get; set; }
    public int BmiObese { get; set; }
    public int BmiSamples { get; set; }

    /// <summary>RBS bands (mmol/L): &lt;7.8 normal, 7.8-11.0 impaired, &gt;=11.1 diabetic.</summary>
    public int RbsNormal { get; set; }
    public int RbsImpaired { get; set; }
    public int RbsDiabetic { get; set; }
    public int RbsSamples { get; set; }
}
