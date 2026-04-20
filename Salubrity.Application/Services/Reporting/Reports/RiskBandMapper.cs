namespace Salubrity.Application.Services.Reporting.Reports;

/// <summary>
/// Five-band risk level shown on the General Health Score risk bars
/// (BMI, Blood Pressure, Blood Glucose, Cholesterol).
/// </summary>
public enum RiskLevel
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}

/// <summary>
/// Maps a numeric vital reading to one of five risk bands for the report's risk-bar visualization.
/// "High" / "VeryHigh" mean the value is above the safe range; "Low" / "VeryLow" mean below.
/// </summary>
public static class RiskBandMapper
{
    public static RiskLevel MapBmi(decimal bmi) => bmi switch
    {
        < 16 => RiskLevel.VeryLow,
        >= 16 and < 18.5m => RiskLevel.Low,
        >= 18.5m and <= 24.9m => RiskLevel.Medium,
        > 24.9m and <= 29.9m => RiskLevel.High,
        _ => RiskLevel.VeryHigh
    };

    public static RiskLevel MapSystolicBp(decimal systolic) => systolic switch
    {
        < 80 => RiskLevel.VeryLow,
        >= 80 and < 90 => RiskLevel.Low,
        >= 90 and <= 130 => RiskLevel.Medium,
        > 130 and <= 140 => RiskLevel.High,
        _ => RiskLevel.VeryHigh
    };

    public static RiskLevel MapBloodGlucose(decimal mmolPerLitre) => mmolPerLitre switch
    {
        < 3 => RiskLevel.VeryLow,
        >= 3 and < 4 => RiskLevel.Low,
        >= 4 and <= 7.8m => RiskLevel.Medium,
        > 7.8m and <= 11.1m => RiskLevel.High,
        _ => RiskLevel.VeryHigh
    };

    public static RiskLevel MapCholesterol(decimal mmolPerLitre) => mmolPerLitre switch
    {
        < 3 => RiskLevel.VeryLow,
        >= 3 and < 4 => RiskLevel.Low,
        >= 4 and <= 5.2m => RiskLevel.Medium,
        > 5.2m and <= 6.2m => RiskLevel.High,
        _ => RiskLevel.VeryHigh
    };
}
