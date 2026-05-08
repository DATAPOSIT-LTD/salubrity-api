using System.Text;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.Interfaces.AI;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Application.Interfaces.Services.Reporting;

namespace Salubrity.Application.Services.Clinical;

public sealed class ClinicalFieldDraftService : IClinicalFieldDraftService
{
    private readonly IIndividualPreliminaryReportService _report;
    private readonly IGeminiClient _gemini;

    public ClinicalFieldDraftService(
        IIndividualPreliminaryReportService report,
        IGeminiClient gemini)
    {
        _report = report;
        _gemini = gemini;
    }

    public async Task<string> GenerateAsync(GenerateClinicalFieldDraftRequestDto req, CancellationToken ct = default)
    {
        var dto = await _report.BuildAsync(req.ParticipantId, ct);
        var (system, body) = BuildPrompt(req, dto);

        var draft = await _gemini.GenerateAsync(
            systemInstruction: system,
            userPrompt: body,
            temperature: 0.3,
            maxOutputTokens: 250,
            ct: ct);

        return string.IsNullOrWhiteSpace(draft)
            ? "Unable to generate a draft at this time. Please write this section manually."
            : draft.Trim();
    }

    private static string SystemFor(string field) => field?.ToLowerInvariant() switch
    {
        "history" =>
            "You are writing the \"Pertinent History Findings\" section of a doctor's review for a health-camp patient. " +
            "Constraints: (1) use ONLY history-relevant facts from the data provided — lifestyle (smoking, alcohol), " +
            "self-reported conditions, and demographics; (2) DO NOT include screening measurements (those belong under " +
            "Clinical Findings); (3) 1-2 short sentences, plain text, no markdown, no first-person; " +
            "(4) if the data contains no clinically relevant history items, output exactly: " +
            "\"No significant past history reported during this screening.\"",

        "clinical" =>
            "You are writing the \"Pertinent Clinical Findings\" section of a doctor's review. " +
            "Constraints: (1) summarize ONLY the abnormal/borderline screening measurements provided; " +
            "(2) state each finding factually with its measured value where given (e.g. \"BP 150/95 mmHg, " +
            "elevated\"); (3) 2-3 sentences, plain text, no markdown, no first-person, no bullet lists; " +
            "(4) if there are no abnormal/borderline findings, output exactly: " +
            "\"All screened parameters were within normal limits.\"",

        "diagnostic" =>
            "You are writing the \"Diagnostic Impression\" section of a doctor's review. " +
            "Constraints: (1) translate the abnormal/borderline findings into clinical impressions only " +
            "(e.g. Stage 1 hypertension, prediabetic glycaemia, mild visual impairment); (2) up to three " +
            "comma-separated impressions in a single short sentence; (3) plain text, no markdown, no " +
            "first-person; (4) DO NOT diagnose anything not supported by the data; (5) if findings are all " +
            "within normal limits, output exactly: \"No clinical concerns identified at this screening.\"",

        "conclusion" =>
            "You are writing the \"Conclusion\" of a doctor's review. " +
            "Constraints: (1) one short sentence summarising the patient's overall state and the headline " +
            "next step (e.g. \"Generally well, with mildly elevated blood pressure that warrants lifestyle " +
            "follow-up.\"); (2) plain text, no markdown, no first-person; (3) be consistent with any " +
            "existing doctor notes provided.",

        _ => throw new ArgumentException($"Unknown field: {field}")
    };

    private static (string system, string body) BuildPrompt(
        GenerateClinicalFieldDraftRequestDto req,
        Salubrity.Application.DTOs.Reports.IndividualPreliminaryReportDto dto)
    {
        var sb = new StringBuilder();

        var age = dto.Demographics.DateOfBirth.HasValue
            ? ((int)((DateTime.UtcNow - dto.Demographics.DateOfBirth.Value).TotalDays / 365.25)).ToString()
            : "unknown";
        sb.AppendLine($"Patient: {dto.Demographics.Gender}, age {age}.");
        sb.AppendLine($"General Health Score: {dto.GeneralHealthScore.Score}% — {dto.GeneralHealthScore.Message}");

        if (dto.AbnormalFindings.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Abnormal findings:");
            foreach (var f in dto.AbnormalFindings) sb.AppendLine($"  - {f}");
        }
        if (dto.BorderlineFindings.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Borderline findings:");
            foreach (var f in dto.BorderlineFindings) sb.AppendLine($"  - {f}");
        }

        var metrics = new List<string>();
        foreach (var s in dto.ServiceSections)
        {
            foreach (var m in s.Metrics)
            {
                if (!string.Equals(m.Status, "Normal", StringComparison.OrdinalIgnoreCase))
                    metrics.Add($"{s.ServiceName} — {m.Label}: {m.Value} ({m.Status})");
            }
        }
        if (metrics.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Non-normal measurements:");
            foreach (var m in metrics) sb.AppendLine($"  - {m}");
        }

        // Include the OTHER three doctor-filled fields for consistency
        var others = new List<(string label, string? value)>
        {
            ("Pertinent History Findings", req.PertinentHistoryFindings),
            ("Pertinent Clinical Findings", req.PertinentClinicalFindings),
            ("Diagnostic Impression", req.DiagnosticImpression),
            ("Conclusion", req.Conclusion),
        };
        var fieldKey = req.Field?.ToLowerInvariant();
        var skipLabel = fieldKey switch
        {
            "history" => "Pertinent History Findings",
            "clinical" => "Pertinent Clinical Findings",
            "diagnostic" => "Diagnostic Impression",
            "conclusion" => "Conclusion",
            _ => null
        };
        var other = others
            .Where(o => o.label != skipLabel && !string.IsNullOrWhiteSpace(o.value))
            .ToList();
        if (other.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Other notes the doctor has already written (stay consistent with these):");
            foreach (var (label, value) in other) sb.AppendLine($"  - {label}: {value!.Trim()}");
        }

        sb.AppendLine();
        sb.AppendLine($"Now write the {req.Field} section per the system instructions.");

        return (SystemFor(req.Field ?? string.Empty), sb.ToString());
    }
}
