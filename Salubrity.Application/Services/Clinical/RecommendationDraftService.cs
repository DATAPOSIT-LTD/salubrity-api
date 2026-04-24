// File: Application/Services/Clinical/RecommendationDraftService.cs
using System.Text;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.Interfaces.AI;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Application.Interfaces.Services.Reporting;

namespace Salubrity.Application.Services.Clinical;

public sealed class RecommendationDraftService : IRecommendationDraftService
{
    private readonly IIndividualPreliminaryReportService _report;
    private readonly IGeminiClient _gemini;

    public RecommendationDraftService(
        IIndividualPreliminaryReportService report,
        IGeminiClient gemini)
    {
        _report = report;
        _gemini = gemini;
    }

    public async Task<string> GenerateAsync(GenerateDraftRequestDto request, CancellationToken ct = default)
    {
        var dto = await _report.BuildAsync(request.ParticipantId, ct);

        var abnormal = dto.AbnormalFindings;
        var borderline = dto.BorderlineFindings;

        var relevantMetrics = new List<string>();
        foreach (var section in dto.ServiceSections)
        {
            foreach (var m in section.Metrics)
            {
                if (!string.Equals(m.Status, "Normal", StringComparison.OrdinalIgnoreCase))
                    relevantMetrics.Add($"{section.ServiceName} — {m.Label}: {m.Value} ({m.Status})");
            }
        }

        var hasDoctorNotes =
            !string.IsNullOrWhiteSpace(request.PertinentHistoryFindings) ||
            !string.IsNullOrWhiteSpace(request.PertinentClinicalFindings) ||
            !string.IsNullOrWhiteSpace(request.DiagnosticImpression) ||
            !string.IsNullOrWhiteSpace(request.Conclusion) ||
            !string.IsNullOrWhiteSpace(request.FollowUpRecommendation) ||
            !string.IsNullOrWhiteSpace(request.RecommendationType);

        if (abnormal.Count == 0 && borderline.Count == 0 && relevantMetrics.Count == 0 && !hasDoctorNotes)
        {
            return "All screened parameters are within normal limits. No specific clinical concerns identified at this screening. Routine wellness follow-up recommended.";
        }

        var systemInstruction =
            "You are writing the \"Recommendation (Specific Instructions)\" section of a patient's health-camp report AS the attending doctor. " +
            "This text will appear in the final report the patient reads, so write it directly TO the patient. Constraints: " +
            "(1) use ONLY the facts provided in the user prompt — never invent findings, diagnoses, medication doses, or values; " +
            "(2) when the doctor has already written history / clinical / diagnostic / conclusion notes, your recommendation MUST be consistent with them; " +
            "(3) write 2-4 concise sentences, plain text only (no markdown, no bullets, no headings); " +
            "(4) DO NOT use first-person pronouns (no 'I', 'we', 'my', 'our'). Use impersonal / imperative voice ('It is recommended...', 'Follow up with...', 'A repeat check is advised in 4 weeks'). You may address the patient as 'you' where natural. Tone: warm, clear, reassuring, jargon-light; " +
            "(5) give practical next steps (follow-up tests, referrals, lifestyle advice) calibrated to severity and consistent with the selected Recommendation Type and Follow-Up; " +
            "(6) do not label anything a final diagnosis; frame as clinical advice from the doctor to the patient.";

        var sb = new StringBuilder();

        var age = dto.Demographics.DateOfBirth.HasValue
            ? ((int)((DateTime.UtcNow - dto.Demographics.DateOfBirth.Value).TotalDays / 365.25)).ToString()
            : "unknown";
        sb.AppendLine($"Patient context: {dto.Demographics.Gender}, age {age}.");
        sb.AppendLine($"General Health Score: {dto.GeneralHealthScore.Score}% — {dto.GeneralHealthScore.Message}");

        if (hasDoctorNotes)
        {
            sb.AppendLine();
            sb.AppendLine("Doctor's current review-form notes (treat as authoritative context):");
            if (!string.IsNullOrWhiteSpace(request.PertinentHistoryFindings))
                sb.AppendLine($"  - Pertinent History Findings: {request.PertinentHistoryFindings!.Trim()}");
            if (!string.IsNullOrWhiteSpace(request.PertinentClinicalFindings))
                sb.AppendLine($"  - Pertinent Clinical Findings: {request.PertinentClinicalFindings!.Trim()}");
            if (!string.IsNullOrWhiteSpace(request.DiagnosticImpression))
                sb.AppendLine($"  - Diagnostic Impression: {request.DiagnosticImpression!.Trim()}");
            if (!string.IsNullOrWhiteSpace(request.Conclusion))
                sb.AppendLine($"  - Conclusion: {request.Conclusion!.Trim()}");
            if (!string.IsNullOrWhiteSpace(request.RecommendationType))
                sb.AppendLine($"  - Recommendation Type: {request.RecommendationType!.Trim()}");
            if (!string.IsNullOrWhiteSpace(request.FollowUpRecommendation))
                sb.AppendLine($"  - Follow-Up Recommendation: {request.FollowUpRecommendation!.Trim()}");
        }

        if (abnormal.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Abnormal findings (from screening):");
            foreach (var f in abnormal) sb.AppendLine($"  - {f}");
        }
        if (borderline.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Borderline findings:");
            foreach (var f in borderline) sb.AppendLine($"  - {f}");
        }
        if (relevantMetrics.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Supporting non-normal measurements:");
            foreach (var m in relevantMetrics) sb.AppendLine($"  - {m}");
        }

        sb.AppendLine();
        sb.AppendLine("Draft the Recommendation (Specific Instructions) paragraph — keep it consistent with the doctor's notes above if any.");

        var draft = await _gemini.GenerateAsync(
            systemInstruction: systemInstruction,
            userPrompt: sb.ToString(),
            temperature: 0.35,
            maxOutputTokens: 350,
            ct: ct);

        return string.IsNullOrWhiteSpace(draft)
            ? "Unable to generate a draft at this time. Please write the recommendation manually."
            : draft;
    }
}
