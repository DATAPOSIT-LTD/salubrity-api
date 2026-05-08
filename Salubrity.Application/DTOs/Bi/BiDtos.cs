namespace Salubrity.Application.DTOs.Bi;

/// <summary>Power BI dimension row: organization.</summary>
public sealed record BiOrganizationDto(
    Guid OrgId,
    string OrgName,
    string? Industry,
    string? Status,
    DateTime CreatedAt
);

/// <summary>Power BI dimension row: camp.</summary>
public sealed record BiCampDto(
    Guid CampId,
    Guid OrgId,
    string OrgName,
    string CampName,
    int Year,
    string? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int ExpectedPatients,
    int AttendedPatients
);

/// <summary>
/// Power BI fact row: one per (patient, camp). Wide shape so PBI can slice
/// without joins. Numeric fields are nullable when not measured.
/// </summary>
public sealed record BiPatientRowDto(
    Guid PatientId,
    Guid CampId,
    Guid OrgId,
    string CampName,
    int Year,
    string? Department,
    int? Age,
    string? Gender,
    decimal? Bmi,
    int? BpSystolic,
    int? BpDiastolic,
    decimal? Rbs,
    decimal? Cholesterol,
    decimal? HbA1c,
    decimal? Creatinine,
    decimal? Ggt,
    string? AlcoholFactor,
    int? LifestyleRiskScore,
    bool MentalHealthFlag,
    bool VisualIssueFlag,
    bool JointIssueFlag,
    bool EarFlag,
    bool NoseFlag,
    bool SkinFlag,
    bool BreastExamFlag,
    bool MssBackFlag
);

/// <summary>Power BI fact row: one per finding (long format).</summary>
public sealed record BiFindingDto(
    Guid PatientId,
    Guid CampId,
    string Station,
    string? Code,
    string Name,
    string? Severity
);

/// <summary>Power BI fact row: one per lab result (long format).</summary>
public sealed record BiLabResultDto(
    Guid PatientId,
    Guid CampId,
    string TestCode,
    string TestName,
    string? Value,
    string? Unit,
    string? ReferenceRange,
    string? Flag
);
