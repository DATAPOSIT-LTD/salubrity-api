namespace Salubrity.Application.Services.Reporting.Reports;

/// <summary>
/// Status of a single vital reading after classification against clinical thresholds.
/// </summary>
public enum VitalStatus
{
    Normal,
    Borderline,
    Abnormal
}

/// <summary>
/// Clinical reference ranges used to classify a vital reading as Normal, Borderline, or Abnormal.
/// Centralized so the medical team only edits one file when ranges change.
/// </summary>
public static class VitalThresholds
{
    /// <summary>
    /// Classify a single numeric value by its field label. Returns Normal for unknown labels.
    /// Match is case-insensitive and looks for known keywords in the label.
    /// </summary>
    public static VitalStatus Classify(string label, decimal value)
    {
        var l = (label ?? string.Empty).ToLowerInvariant();

        if (l.Contains("blood pressure") && (l.Contains("systolic") || !l.Contains("diastolic")))
        {
            return value switch
            {
                >= 90 and <= 130 => VitalStatus.Normal,
                // Fix: exactly 140 is Abnormal (Stage 2 HTN threshold), not Borderline
                (> 130 and < 140) or (>= 80 and < 90) => VitalStatus.Borderline,
                _ => VitalStatus.Abnormal
            };
        }

        if (l.Contains("blood pressure") && l.Contains("diastolic"))
        {
            return value switch
            {
                >= 60 and <= 85 => VitalStatus.Normal,
                (> 85 and <= 90) or (>= 55 and < 60) => VitalStatus.Borderline,
                _ => VitalStatus.Abnormal
            };
        }

        if (l.Contains("heart rate") || l.Contains("pulse"))
        {
            return value switch
            {
                >= 60 and <= 100 => VitalStatus.Normal,
                (> 100 and <= 120) or (>= 50 and < 60) => VitalStatus.Borderline,
                _ => VitalStatus.Abnormal
            };
        }

        if (l.Contains("temperature"))
        {
            return value switch
            {
                >= 36.1m and <= 37.5m => VitalStatus.Normal,
                (> 37.5m and <= 38m) or (>= 35m and < 36.1m) => VitalStatus.Borderline,
                _ => VitalStatus.Abnormal
            };
        }

        if (l.Contains("oxygen") || l.Contains("spo"))
        {
            return value switch
            {
                >= 95 => VitalStatus.Normal,
                >= 90 and < 95 => VitalStatus.Borderline,
                _ => VitalStatus.Abnormal
            };
        }

        if (l.Contains("bmi") || l.Contains("body mass"))
        {
            return value switch
            {
                >= 18.5m and <= 24.9m => VitalStatus.Normal,
                (> 24.9m and <= 29.9m) or (>= 17m and < 18.5m) => VitalStatus.Borderline,
                _ => VitalStatus.Abnormal
            };
        }

        if (l.Contains("blood sugar") || l.Contains("glucose") || l.Contains("rbs"))
        {
            return value switch
            {
                >= 4 and <= 7.8m => VitalStatus.Normal,
                > 7.8m and <= 11.1m => VitalStatus.Borderline,
                _ => VitalStatus.Abnormal
            };
        }

        if (l.Contains("cholesterol"))
        {
            return value switch
            {
                <= 5.2m => VitalStatus.Normal,
                > 5.2m and <= 6.2m => VitalStatus.Borderline,
                _ => VitalStatus.Abnormal
            };
        }

        // Unknown metric — don't penalize the patient's score, treat as Normal.
        return VitalStatus.Normal;
    }

    /// <summary>
    /// Classify a blood pressure string of the form "120/80" into systolic + diastolic statuses.
    /// Returns the worst of the two (Abnormal &gt; Borderline &gt; Normal).
    /// Returns null if the string can't be parsed.
    /// </summary>
    public static VitalStatus? ClassifyBloodPressure(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !raw.Contains('/'))
            return null;

        var parts = raw.Split('/', 2);
        if (!decimal.TryParse(parts[0].Trim(), out var sys) ||
            !decimal.TryParse(parts[1].Trim(), out var dia))
            return null;

        var sysStatus = Classify("blood pressure systolic", sys);
        var diaStatus = Classify("blood pressure diastolic", dia);

        // Worst of the two wins
        if (sysStatus == VitalStatus.Abnormal || diaStatus == VitalStatus.Abnormal)
            return VitalStatus.Abnormal;
        if (sysStatus == VitalStatus.Borderline || diaStatus == VitalStatus.Borderline)
            return VitalStatus.Borderline;
        return VitalStatus.Normal;
    }
}
