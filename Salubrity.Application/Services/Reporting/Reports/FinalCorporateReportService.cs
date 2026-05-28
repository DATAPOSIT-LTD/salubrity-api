// File: Application/Services/Reporting/Reports/FinalCorporateReportService.cs
// Pass A + Gemini narratives.
// Real data: ParticipationRate, Age Demographics, Top Critical Findings, Recommendations.
// AI narratives via Gemini for all text sections (Introduction, Executive Summary, every section summary, Conclusion).
// Sample data still shown for: Lifestyle Risk, Metabolic & NCD bars, Mental Health distribution, Pain Assessment, Systemic Organ, Eye Visual, Outlook predictions, Trend line chart.

using QuestPDF.Fluent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.AI;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Reporting;
using Salubrity.Application.Interfaces.Services.Reporting;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Reporting.Reports;

public sealed class FinalCorporateReportService : IFinalCorporateReportService
{
    private readonly IHealthCampRepository _campRepo;
    private readonly ICorporateReportRepository _corpRepo;
    private readonly IFinalCorporateReportRepository _finalRepo;
    private readonly IGeminiClient _gemini;
    private readonly Salubrity.Application.Interfaces.IEmailService _email;
    private readonly ILogger<FinalCorporateReportService> _log;

    public FinalCorporateReportService(
        IHealthCampRepository campRepo,
        ICorporateReportRepository corpRepo,
        IFinalCorporateReportRepository finalRepo,
        IGeminiClient gemini,
        Salubrity.Application.Interfaces.IEmailService email,
        ILogger<FinalCorporateReportService> log)
    {
        _campRepo = campRepo;
        _corpRepo = corpRepo;
        _finalRepo = finalRepo;
        _gemini = gemini;
        _email = email;
        _log = log;
    }

    public async Task<byte[]> BuildPdfAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
        var dto = await BuildAsync(campId, filters, ct);
        var doc = new FinalCorporateReportDocument(dto);
        return doc.GeneratePdf();
    }

    public async Task SendEmailAsync(Guid campId, SendCorporateReportEmailRequest request, CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.Recipients is null || request.Recipients.Count == 0)
            throw new ValidationException(["At least one recipient is required."]);

        var dto = await BuildAsync(campId, null, ct);
        var pdfBytes = await BuildPdfAsync(campId, null, ct);

        var fileName = $"final-corporate-report-{(string.IsNullOrWhiteSpace(dto.CampName) ? campId.ToString("N") : dto.CampName.Replace(" ", "-").ToLowerInvariant())}.pdf";
        var attachments = new List<Salubrity.Application.DTOs.Email.EmailAttachment>
        {
            new Salubrity.Application.DTOs.Email.EmailAttachment
            {
                FileName = fileName,
                Content = pdfBytes,
                ContentType = "application/pdf",
            }
        };

        var subject = $"Final Corporate Report - {dto.CampName}";
        var model = new
        {
            contact_name = string.IsNullOrWhiteSpace(request.ContactName) ? "there" : request.ContactName,
            camp_name = dto.CampName,
            client_name = dto.ClientName,
            date_range = dto.DateRange,
            generated_at = dto.GeneratedAt.ToString("yyyy-MM-dd"),
            message = request.Message ?? string.Empty,
            has_message = !string.IsNullOrWhiteSpace(request.Message),
        };

        if (request.Recipients.Count == 1)
        {
            await _email.SendAsync(new Salubrity.Application.DTOs.Email.EmailRequestDto
            {
                ToEmail = request.Recipients[0],
                Subject = subject,
                TemplateKey = "CorporateReport",
                Model = model,
                Attachments = attachments,
            });
        }
        else
        {
            await _email.SendBatchAsync(new Salubrity.Application.DTOs.Email.BatchEmailRequest
            {
                ToEmails = request.Recipients,
                Subject = subject,
                TemplateKey = "CorporateReport",
                Model = model,
                Attachments = attachments,
            });
        }

        _log.LogInformation("Sent FINAL corporate report for camp {CampId} to {Count} recipients", campId, request.Recipients.Count);
    }

    public async Task<FinalCorporateReportDto> BuildAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
        var camp = await _campRepo.GetByIdAsync(campId)
            ?? throw new NotFoundException("Camp not found.");

        var clientName = camp.Organization?.BusinessName ?? string.Empty;
        var clientEmail = camp.Organization?.Email ?? string.Empty;
        var dateRange = camp.EndDate.HasValue && camp.EndDate.Value.Date != camp.StartDate.Date
            ? $"{camp.StartDate:dd MMM yyyy} – {camp.EndDate.Value:dd MMM yyyy}"
            : camp.StartDate.ToString("dd MMM yyyy");

        var raw = await _corpRepo.LoadAsync(campId, filters, ct);
        var totalAttendees = raw?.TotalAttendees ?? 0;
        var expected = raw?.ExpectedParticipants ?? 0;
        var participationRate = expected > 0 ? Math.Min(100, (int)Math.Round(totalAttendees * 100.0 / expected)) : 0;
        var female = raw?.Female ?? 0;
        var male = raw?.Male ?? 0;
        var femalePct = totalAttendees > 0 ? (int)Math.Round(female * 100.0 / totalAttendees) : 0;
        var malePct = totalAttendees > 0 ? Math.Max(0, 100 - femalePct) : 0;

        var ageBuckets = await _finalRepo.GetAgeBucketsAsync(campId, ct);
        var eyeAgg = await _finalRepo.GetEyeVisualHealthAsync(campId, ct);
        var topFindings = (raw?.TopFindings ?? new List<TopFindingDto>())
            .Select(f => new FinalTopFindingDto { Code = f.Code, Name = f.Name, N = f.N, Pct = f.Pct, Level = f.Level })
            .ToList();
        var recommendations = await _finalRepo.GetRecommendationsAsync(campId, ct);

        // Sample distributions used for chart sections still pending real-data wiring (so narratives match what is on screen)
        var lifestyleSlices = new List<RiskSliceDto>
        {
            new() { Label = "Low", Value = 56 },
            new() { Label = "Medium", Value = 22 },
            new() { Label = "High", Value = 14 },
            new() { Label = "Very High", Value = 8 },
        };
        var metabolicBuckets = new List<AgeBucketDto>
        {
            new() { Label = "Low risk", Female = 45, Male = 41 },
            new() { Label = "Moderate risk", Female = 28, Male = 30 },
            new() { Label = "High risk", Female = 11, Male = 14 },
        };
        var mentalScore = 6;
        var mentalDistribution = new List<RiskSliceDto>
        {
            new() { Label = "Low", Value = 22 },
            new() { Label = "Good", Value = 71 },
            new() { Label = "At Risk", Value = 7 },
        };
        var painMale = new List<RiskSliceDto>
        {
            new() { Label = "No Pain Reported", Value = 64 },
            new() { Label = "Mild Discomfort", Value = 26 },
            new() { Label = "Chronic Pain Risk", Value = 10 },
        };
        var painFemale = new List<RiskSliceDto>
        {
            new() { Label = "No Pain Reported", Value = 58 },
            new() { Label = "Mild Discomfort", Value = 30 },
            new() { Label = "Chronic Pain Risk", Value = 12 },
        };
        var lifestyleSecondary = new List<RiskSliceDto>
        {
            new() { Label = "Low", Value = 64 },
            new() { Label = "High", Value = 36 },
        };
        var organNormal = 60; var organMonitor = 30; var organAtRisk = 10;

        // Build Gemini prompt with everything that's on screen so narratives match
        var narratives = await GenerateFinalNarrativesAsync(
            camp.Name, clientName, dateRange, expected, totalAttendees, participationRate, femalePct, malePct,
            ageBuckets, topFindings, recommendations, lifestyleSlices, metabolicBuckets, mentalScore,
            mentalDistribution, painMale, painFemale, lifestyleSecondary, organNormal, organMonitor, organAtRisk, eyeAgg, ct);

        return new FinalCorporateReportDto
        {
            CampId = campId,
            CampName = camp.Name,
            ClientName = clientName,
            ClientContactName = clientName,
            ClientContactEmail = clientEmail,
            DateRange = dateRange,
            GeneratedAt = DateTime.UtcNow,

            Introduction = narratives.Introduction,
            ExecutiveSummaryGeneral = new ExecutiveSummaryGeneralOverviewDto
            {
                Overview = narratives.ExecutiveOverview,
                ClinicalFindings = narratives.ExecutiveClinicalFindings,
                Recommendation = narratives.ExecutiveRecommendation,
            },
            ExecutiveSummaryDetail = new ExecutiveSummaryDetailDto
            {
                ParticipationCoverage = $"Participation reached {participationRate}% of the expected target population (n={totalAttendees} of {expected}).",
                OverallHealthOfDisease = narratives.OverallHealthOfDisease,
                KeyRiskClusters = narratives.KeyRiskClusters,
            },
            ObjectivesAndMethods = narratives.ObjectivesAndMethods,
            ResultAtAGlance = new ResultAtAGlanceDto
            {
                ParticipationRate = participationRate,
                AbnormalFindingsPercent = 0,
                FollowUpPercent = 0,
                ParticipationRateSecondary = participationRate,
            },
            ParticipationByAge = new ParticipationByAgeDto
            {
                Buckets = ageBuckets,
                Notes = narratives.ParticipationByAgeNotes,
            },
            LifestyleRiskOverall = new LifestyleRiskDto { Slices = lifestyleSlices, Summary = narratives.LifestyleRiskOverallSummary },
            MetabolicNcdRiskBars = new MetabolicNcdRiskDto { Buckets = metabolicBuckets, Notes = narratives.MetabolicNcdNotes },
            MentalHealth = new MentalHealthDto
            {
                OverallScoreOutOfTen = mentalScore,
                Band = mentalScore >= 7 ? "Good" : mentalScore >= 5 ? "Moderate" : "Needs Attention",
                Distribution = mentalDistribution,
                Summary = narratives.MentalHealthSummary,
            },
            EyeVisualHealth = new EyeVisualHealthDto
            {
                LeftEye = eyeAgg.LeftEyeAcuity,
                RightEye = eyeAgg.RightEyeAcuity,
                Summary = eyeAgg.HasData
                    ? (string.IsNullOrWhiteSpace(narratives.EyeVisualHealthSummary)
                        ? $"Most common visual acuity recorded — Left: {eyeAgg.LeftEyeAcuity} (n={eyeAgg.LeftEyeCount}), Right: {eyeAgg.RightEyeAcuity} (n={eyeAgg.RightEyeCount})."
                        : narratives.EyeVisualHealthSummary)
                    : "No eye exam responses recorded for this camp.",
            },
            PainAssessment = new PainAssessmentDto { Male = painMale, Female = painFemale, Notes = narratives.PainAssessmentNotes },
            LifestyleRiskSecondary = new LifestyleRiskDto { Slices = lifestyleSecondary, Summary = narratives.LifestyleRiskSecondarySummary },
            SystemicOrganFunction = new SystemicOrganFunctionDto
            {
                NormalFunctionPercent = organNormal,
                RequiresMonitoringPercent = organMonitor,
                AtRiskPercent = organAtRisk,
                Notes = narratives.SystemicOrganFunctionNotes,
            },
            TopCriticalClinicalFindings = topFindings,
            TopCriticalFindingsSummary = topFindings.Count == 0
                ? "No critical findings recorded for this camp yet."
                : narratives.TopFindingsSummary,
            AnalysisOutlook = new AnalysisOutlookDto
            {
                Comparator = "Metabolic & NCD Risk",
                Series = new()
                {
                    new() { Year = "2024", Points = new() { new(){XLabel="Q1",Value=12}, new(){XLabel="Q2",Value=14}, new(){XLabel="Q3",Value=18}, new(){XLabel="Q4",Value=17} } },
                    new() { Year = "2025", Points = new() { new(){XLabel="Q1",Value=16}, new(){XLabel="Q2",Value=21}, new(){XLabel="Q3",Value=19}, new(){XLabel="Q4",Value=22} } },
                },
                Summary = narratives.AnalysisOutlookSummary,
            },
            OutlookPredictions = new OutlookPredictionsDto
            {
                OverallHealth     = new() { Label = "Overall Health Score",   Delta = "+4.2%", Direction = "improving",  Caption = "Improving" },
                AbnormalFindings  = new() { Label = "Abnormal Findings",      Delta = "12%",   Direction = "stable",     Caption = "Stable" },
                FollowUpRate      = new() { Label = "Follow-up Compliance",   Delta = "-21%",  Direction = "declining",  Caption = "Needs Attention" },
            },
            TrendAnalysisSummary = new TrendAnalysisSummaryDto
            {
                Improvements = narratives.TrendImprovements,
                Declines = narratives.TrendDeclines,
                StableAreas = narratives.TrendStableAreas,
            },
            Recommendations = recommendations,
            Conclusion = narratives.Conclusion,
        };
    }

    private sealed class FinalNarratives
    {
        public string Introduction { get; set; } = string.Empty;
        public string ExecutiveOverview { get; set; } = string.Empty;
        public string ExecutiveClinicalFindings { get; set; } = string.Empty;
        public string ExecutiveRecommendation { get; set; } = string.Empty;
        public string OverallHealthOfDisease { get; set; } = string.Empty;
        public string KeyRiskClusters { get; set; } = string.Empty;
        public string ObjectivesAndMethods { get; set; } = string.Empty;
        public string ParticipationByAgeNotes { get; set; } = string.Empty;
        public string LifestyleRiskOverallSummary { get; set; } = string.Empty;
        public string MetabolicNcdNotes { get; set; } = string.Empty;
        public string MentalHealthSummary { get; set; } = string.Empty;
        public string EyeVisualHealthSummary { get; set; } = string.Empty;
        public string PainAssessmentNotes { get; set; } = string.Empty;
        public string LifestyleRiskSecondarySummary { get; set; } = string.Empty;
        public string SystemicOrganFunctionNotes { get; set; } = string.Empty;
        public string TopFindingsSummary { get; set; } = string.Empty;
        public string AnalysisOutlookSummary { get; set; } = string.Empty;
        public string TrendImprovements { get; set; } = string.Empty;
        public string TrendDeclines { get; set; } = string.Empty;
        public string TrendStableAreas { get; set; } = string.Empty;
        public string Conclusion { get; set; } = string.Empty;
    }

    private async Task<FinalNarratives> GenerateFinalNarrativesAsync(
        string campName, string clientName, string dateRange,
        int expected, int totalAttendees, int participationRate, int femalePct, int malePct,
        List<AgeBucketDto> ageBuckets, List<FinalTopFindingDto> topFindings, List<FinalRecommendationDto> recs,
        List<RiskSliceDto> lifestyle, List<AgeBucketDto> metabolic, int mentalScore, List<RiskSliceDto> mentalDist,
        List<RiskSliceDto> painMale, List<RiskSliceDto> painFemale, List<RiskSliceDto> lifestyle2,
        int organNormal, int organMonitor, int organAtRisk, Salubrity.Application.Interfaces.Repositories.Reporting.EyeVisualAggregateDto eyeAgg, CancellationToken ct)
    {
        var system =
            "You are writing the narrative sections of a FINAL Corporate Health Report for a client organisation. " +
            "Constraints: " +
            "(1) use ONLY the facts in the user prompt; do not invent statistics or diagnoses; " +
            "(2) write each value in 2-4 plain-text sentences (no markdown, no bullets); " +
            "(3) tone: professional, factual, suitable for an HR/management audience; " +
            "(4) DO NOT use first-person pronouns; use impersonal voice; " +
            "(5) refer to the client by name where natural; " +
            "(6) RESPOND WITH A JSON OBJECT containing exactly these string keys: " +
            "introduction, executiveOverview, executiveClinicalFindings, executiveRecommendation, " +
            "overallHealthOfDisease, keyRiskClusters, objectivesAndMethods, participationByAgeNotes, " +
            "lifestyleRiskOverallSummary, metabolicNcdNotes, mentalHealthSummary, eyeVisualHealthSummary, " +
            "painAssessmentNotes, lifestyleRiskSecondarySummary, systemicOrganFunctionNotes, " +
            "topFindingsSummary, analysisOutlookSummary, trendImprovements, trendDeclines, trendStableAreas, conclusion. " +
            "No other keys, no surrounding text.";

        var sb = new StringBuilder();
        sb.AppendLine($"Client: {clientName}");
        sb.AppendLine($"Camp: {campName}");
        sb.AppendLine($"Date(s): {dateRange}");
        sb.AppendLine($"Expected participants: {expected}");
        sb.AppendLine($"Total attendees: {totalAttendees}");
        sb.AppendLine($"Participation rate: {participationRate}%");
        sb.AppendLine($"Gender split — Female: {femalePct}% | Male: {malePct}%");
        sb.AppendLine();
        sb.AppendLine("Age distribution (Female / Male per band):");
        foreach (var b in ageBuckets) sb.AppendLine($"  - {b.Label}: F {b.Female} | M {b.Male}");
        sb.AppendLine();
        sb.AppendLine("Lifestyle Risk Stratification (overall %):");
        foreach (var s in lifestyle) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        sb.AppendLine();
        sb.AppendLine("Metabolic & NCD Risk (counts per band):");
        foreach (var b in metabolic) sb.AppendLine($"  - {b.Label}: F {b.Female} | M {b.Male}");
        sb.AppendLine();
        sb.AppendLine($"Mental Health overall score: {mentalScore}/10");
        sb.AppendLine("Mental Health distribution (%):");
        foreach (var s in mentalDist) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        sb.AppendLine();
        sb.AppendLine("Pain Assessment — Male (%):");
        foreach (var s in painMale) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        sb.AppendLine("Pain Assessment — Female (%):");
        foreach (var s in painFemale) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        sb.AppendLine();
        sb.AppendLine("Lifestyle Risk binary view (%):");
        foreach (var s in lifestyle2) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        sb.AppendLine();
        sb.AppendLine($"Systemic Organ Function — Normal: {organNormal}% | Requires Monitoring: {organMonitor}% | At Risk: {organAtRisk}%");
        sb.AppendLine();
        if (eyeAgg.HasData)
            sb.AppendLine($"Eye/Visual Health — Left eye most-common: {eyeAgg.LeftEyeAcuity} (n={eyeAgg.LeftEyeCount}); Right eye most-common: {eyeAgg.RightEyeAcuity} (n={eyeAgg.RightEyeCount}).");
        else
            sb.AppendLine("Eye/Visual Health — No eye exam responses recorded for this camp.");
        sb.AppendLine();
        sb.AppendLine("Top Critical Clinical Findings (n / prevalence% / severity):");
        if (topFindings.Count == 0)
            sb.AppendLine("  (none recorded)");
        else
            foreach (var f in topFindings.Take(15))
                sb.AppendLine($"  - {f.Name}: n={f.N}, {f.Pct}%, severity={f.Level}");
        sb.AppendLine();
        sb.AppendLine($"Recommendations on file: {recs.Count}");
        sb.AppendLine();
        sb.AppendLine("Generate the JSON narrative now. introduction is one short paragraph framing the report. executiveOverview / executiveClinicalFindings / executiveRecommendation each 2-4 sentences. trendImprovements / trendDeclines / trendStableAreas each 1-3 sentences. conclusion is one closing paragraph. objectivesAndMethods describes purpose and methodology in 2-3 sentences.");

        string raw = string.Empty;
        try
        {
            raw = await _gemini.GenerateJsonAsync(system, sb.ToString(), temperature: 0.35, maxOutputTokens: 6000, ct: ct);
            var json = ExtractJsonObject(raw);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string read(string key) => root.TryGetProperty(key, out var v) ? (v.GetString() ?? string.Empty).Trim() : string.Empty;
            return new FinalNarratives
            {
                Introduction = read("introduction"),
                ExecutiveOverview = read("executiveOverview"),
                ExecutiveClinicalFindings = read("executiveClinicalFindings"),
                ExecutiveRecommendation = read("executiveRecommendation"),
                OverallHealthOfDisease = read("overallHealthOfDisease"),
                KeyRiskClusters = read("keyRiskClusters"),
                ObjectivesAndMethods = read("objectivesAndMethods"),
                ParticipationByAgeNotes = read("participationByAgeNotes"),
                LifestyleRiskOverallSummary = read("lifestyleRiskOverallSummary"),
                MetabolicNcdNotes = read("metabolicNcdNotes"),
                MentalHealthSummary = read("mentalHealthSummary"),
                EyeVisualHealthSummary = read("eyeVisualHealthSummary"),
                PainAssessmentNotes = read("painAssessmentNotes"),
                LifestyleRiskSecondarySummary = read("lifestyleRiskSecondarySummary"),
                SystemicOrganFunctionNotes = read("systemicOrganFunctionNotes"),
                TopFindingsSummary = read("topFindingsSummary"),
                AnalysisOutlookSummary = read("analysisOutlookSummary"),
                TrendImprovements = read("trendImprovements"),
                TrendDeclines = read("trendDeclines"),
                TrendStableAreas = read("trendStableAreas"),
                Conclusion = read("conclusion"),
            };
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to parse Gemini narrative JSON for Final Corporate Report. Raw: {Raw}", raw);
            const string F = "Narrative generation is currently unavailable. Please retry shortly or write this section manually.";
            return new FinalNarratives
            {
                Introduction = F, ExecutiveOverview = F, ExecutiveClinicalFindings = F, ExecutiveRecommendation = F,
                OverallHealthOfDisease = F, KeyRiskClusters = F, ObjectivesAndMethods = F,
                ParticipationByAgeNotes = F, LifestyleRiskOverallSummary = F, MetabolicNcdNotes = F,
                MentalHealthSummary = F, EyeVisualHealthSummary = F, PainAssessmentNotes = F,
                LifestyleRiskSecondarySummary = F, SystemicOrganFunctionNotes = F, TopFindingsSummary = F,
                AnalysisOutlookSummary = F, TrendImprovements = F, TrendDeclines = F, TrendStableAreas = F, Conclusion = F,
            };
        }
    }

    private static string ExtractJsonObject(string text)
    {
        var t = text?.Trim() ?? string.Empty;
        if (t.StartsWith("```"))
        {
            var firstNl = t.IndexOf('\n');
            if (firstNl > 0) t = t.Substring(firstNl + 1);
            if (t.EndsWith("```")) t = t.Substring(0, t.Length - 3);
            t = t.Trim();
        }
        var start = t.IndexOf('{');
        var end = t.LastIndexOf('}');
        if (start < 0 || end <= start) return t;
        return t.Substring(start, end - start + 1);
    }
}
