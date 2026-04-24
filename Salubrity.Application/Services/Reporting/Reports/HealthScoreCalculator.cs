namespace Salubrity.Application.Services.Reporting.Reports;

/// <summary>
/// Result of computing a patient's overall General Health Score.
/// </summary>
public record HealthScoreResult(int Score, string Message);

/// <summary>
/// Computes a 0-100 General Health Score from a patient's classified vital readings.
/// Each Borderline counts as half a Normal; each Abnormal counts as zero.
/// </summary>
public static class HealthScoreCalculator
{
    public static HealthScoreResult Compute(
        int normalCount,
        int borderlineCount,
        int abnormalCount)
    {
        var total = normalCount + borderlineCount + abnormalCount;
        if (total == 0)
            return new HealthScoreResult(0, "No data available to calculate a score.");

        var score = (decimal)((normalCount + 0.5 * borderlineCount) / total * 100);
        var rounded = (int)System.Math.Round(score);

        var message = rounded switch
        {
            >= 90 => "Excellent health. No risks detected.",
            >= 75 => "You are in good health. Minor areas to monitor.",
            >= 60 => "Moderate health. Some areas need attention.",
            _ => "Multiple findings need follow-up. Please consult a healthcare provider."
        };

        return new HealthScoreResult(rounded, message);
    }
}
