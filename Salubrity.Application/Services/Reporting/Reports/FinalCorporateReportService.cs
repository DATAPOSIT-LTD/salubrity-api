// File: Application/Services/Reporting/Reports/FinalCorporateReportService.cs
// All chart sections sourced from real DB queries. Gemini narrates only.
// Trend/outlook data suppressed with "insufficient history" message until
// multiple camps exist for this organisation.

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

        // ── Real data from repository (P0 fix: no more hardcoded blocks) ──
        var ageBuckets   = await _finalRepo.GetAgeBucketsAsync(campId, ct);
        var eyeAgg       = await _finalRepo.GetEyeVisualHealthAsync(campId, ct);
        var topFindings  = (raw?.TopFindings ?? new List<TopFindingDto>())
            .Select(f => new FinalTopFindingDto { Code = f.Code, Name = f.Name, N = f.N, Pct = f.Pct, Level = f.Level })
            .ToList();
        var recommendations = await _finalRepo.GetRecommendationsAsync(campId, ct);

        // Lifestyle risk stratified from BMI + BP + blood sugar + cholesterol threshold crossings
        var lifestyleSlices = await _finalRepo.GetLifestyleRiskDistributionAsync(campId, ct);

        // Mental health from PHQ-style scoring
        var mentalCounts = await _finalRepo.GetMentalHealthCountsAsync(campId, ct);
        int mentalTotal  = Math.Max(mentalCounts.TotalMeasured, 1);
        // Score 0–10 weighted by distribution: Good→10, AtRisk→5, Low→2
        var mentalScore = mentalCounts.TotalMeasured > 0
            ? Math.Clamp((int)Math.Round(
                (mentalCounts.GoodCount * 10.0 + mentalCounts.AtRiskCount * 5.0 + mentalCounts.LowCount * 2.0)
                / mentalTotal), 0, 10)
            : 0;
        var mentalDistribution = mentalCounts.TotalMeasured > 0
            ? new List<RiskSliceDto>
            {
                new() { Label = "Low",    Value = (int)Math.Round(mentalCounts.LowCount    * 100.0 / mentalTotal) },
                new() { Label = "Good",   Value = (int)Math.Round(mentalCounts.GoodCount   * 100.0 / mentalTotal) },
                new() { Label = "At Risk",Value = (int)Math.Round(mentalCounts.AtRiskCount * 100.0 / mentalTotal) },
            }
            : new List<RiskSliceDto>
            {
                new() { Label = "Low", Value = 0 }, new() { Label = "Good", Value = 0 }, new() { Label = "At Risk", Value = 0 },
            };

        // Metabolic NCD risk gender-stratified from vitals
        var metabolicBuckets = await _finalRepo.GetMetabolicNcdRiskAsync(campId, ct);

        // Follow-up from ServiceReferrals
        var followUpCounts = await _finalRepo.GetFollowUpCountsAsync(campId, ct);
        int followUpDenom  = Math.Max(followUpCounts.TotalAttendees, totalAttendees);
        int followUpPercent = followUpDenom > 0
            ? Math.Min(100, (int)Math.Round(followUpCounts.ReferredCount * 100.0 / followUpDenom))
            : 0;

        // Abnormal findings from vital threshold classification
        var abnormalCount   = await _finalRepo.GetAbnormalPatientCountAsync(campId, ct);
        double abnormalPct  = totalAttendees > 0
            ? Math.Min(100, Math.Round(abnormalCount * 100.0 / totalAttendees, 1))
            : 0;

        // Lifestyle secondary: binary Low (Low+Medium) vs High (High+VeryHigh)
        int lifeLow  = (lifestyleSlices.FirstOrDefault(s => s.Label == "Low")?.Value ?? 0)
                     + (lifestyleSlices.FirstOrDefault(s => s.Label == "Medium")?.Value ?? 0);
        int lifeHigh = Math.Max(0, 100 - lifeLow);
        var lifestyleSecondary = new List<RiskSliceDto>
        {
            new() { Label = "Low",  Value = lifeLow  },
            new() { Label = "High", Value = lifeHigh },
        };

        // Systemic organ function derived from vital classification
        // AtRisk = patients with ≥1 Abnormal vital; RequiresMonitoring ≈ High lifestyle risk - AtRisk
        int organAtRisk = totalAttendees > 0 ? (int)Math.Round(abnormalCount * 100.0 / totalAttendees) : 0;
        int highRisk    = (lifestyleSlices.FirstOrDefault(s => s.Label == "High")?.Value ?? 0)
                        + (lifestyleSlices.FirstOrDefault(s => s.Label == "Very High")?.Value ?? 0);
        int organMonitor = Math.Max(0, Math.Min(100 - organAtRisk, highRisk - organAtRisk));
        int organNormal  = Math.Max(0, 100 - organAtRisk - organMonitor);

        // Pain assessment: not captured in the current screening protocol
        var painMale   = new List<RiskSliceDto>();
        var painFemale = new List<RiskSliceDto>();

        // Build Gemini narratives with the computed (real) data
        var narratives = await GenerateFinalNarrativesAsync(
            camp.Name, clientName, dateRange, expected, totalAttendees, participationRate, femalePct, malePct,
            ageBuckets, topFindings, recommendations, lifestyleSlices, metabolicBuckets, mentalScore,
            mentalDistribution, painMale, painFemale, lifestyleSecondary, organNormal, organMonitor, organAtRisk, eyeAgg, ct);

        const string noTrendData =
            "Trend analysis requires data from multiple health camps. " +
            "This report reflects the initial baseline screening; longitudinal comparisons will be available after subsequent camps.";
        const string noTrendDeclines  = "No prior camp data available to identify declines.";
        const string noTrendStable    = "No prior camp data available to identify stable areas.";

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
                AbnormalFindingsPercent = abnormalPct,
                FollowUpPercent = followUpPercent,
                ParticipationRateSecondary = participationRate,
            },
            ParticipationByAge = new ParticipationByAgeDto
            {
                Buckets = ageBuckets,
                Notes = narratives.ParticipationByAgeNotes,
            },
            LifestyleRiskOverall = new LifestyleRiskDto { Slices = lifestyleSlices, Summary = narratives.LifestyleRiskOverallSummary },
            MetabolicNcdRiskBars = new MetabolicNcdRiskDto
            {
                Buckets = metabolicBuckets,
                Notes = metabolicBuckets.Any(b => b.Female + b.Male > 0)
                    ? narratives.MetabolicNcdNotes
                    : "Metabolic NCD risk stratification data not available for this camp.",
            },
            MentalHealth = new MentalHealthDto
            {
                OverallScoreOutOfTen = mentalScore,
                Band = mentalScore >= 7 ? "Good" : mentalScore >= 5 ? "Moderate" : mentalCounts.TotalMeasured == 0 ? "No Data" : "Needs Attention",
                Distribution = mentalDistribution,
                Summary = mentalCounts.TotalMeasured > 0
                    ? narratives.MentalHealthSummary
                    : "Mental health assessment data not recorded for this camp.",
            },
            EyeVisualHealth = new EyeVisualHealthDto
            {
                LeftEye = eyeAgg.LeftEyeAcuity,
                RightEye = eyeAgg.RightEyeAcuity,
                Summary = eyeAgg.HasData
                    ? (string.IsNullOrWhiteSpace(narratives.EyeVisualHealthSummary)
                        ? $"Most common visual acuity — Left: {eyeAgg.LeftEyeAcuity} (n={eyeAgg.LeftEyeCount}), Right: {eyeAgg.RightEyeAcuity} (n={eyeAgg.RightEyeCount})."
                        : narratives.EyeVisualHealthSummary)
                    : "No eye exam responses recorded for this camp.",
            },
            PainAssessment = new PainAssessmentDto
            {
                Male = painMale,
                Female = painFemale,
                Notes = "Pain assessment was not administered during this screening cycle.",
            },
            LifestyleRiskSecondary = new LifestyleRiskDto { Slices = lifestyleSecondary, Summary = narratives.LifestyleRiskSecondarySummary },
            SystemicOrganFunction = new SystemicOrganFunctionDto
            {
                NormalFunctionPercent = organNormal,
                RequiresMonitoringPercent = organMonitor,
                AtRiskPercent = organAtRisk,
                Notes = totalAttendees > 0
                    ? narratives.SystemicOrganFunctionNotes
                    : "No vital data recorded for this camp.",
            },
            TopCriticalClinicalFindings = topFindings,
            TopCriticalFindingsSummary = topFindings.Count == 0
                ? "No critical findings recorded for this camp yet."
                : narratives.TopFindingsSummary,

            // Trend data requires multiple camps — suppress until historical data exists
            AnalysisOutlook = new AnalysisOutlookDto
            {
                Comparator = "Metabolic & NCD Risk",
                Series = new(),
                Summary = noTrendData,
            },
            OutlookPredictions = new OutlookPredictionsDto
            {
                OverallHealth    = new() { Label = "Overall Health Score",  Delta = "N/A", Direction = "stable", Caption = "Baseline camp" },
                AbnormalFindings = new() { Label = "Abnormal Findings",     Delta = "N/A", Direction = "stable", Caption = "Baseline camp" },
                FollowUpRate     = new() { Label = "Follow-up Compliance",  Delta = "N/A", Direction = "stable", Caption = "Baseline camp" },
            },
            TrendAnalysisSummary = new TrendAnalysisSummaryDto
            {
                Improvements = noTrendData,
                Declines     = noTrendDeclines,
                StableAreas  = noTrendStable,
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
        sb.AppendLine("Lifestyle Risk Stratification (% of employees with vital data):");
        if (lifestyle.Any(s => s.Value > 0))
            foreach (var s in lifestyle) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        else
            sb.AppendLine("  (lifestyle risk data not available for this camp)");
        sb.AppendLine();
        sb.AppendLine("Metabolic & NCD Risk (counts per band):");
        if (metabolic.Any(b => b.Female + b.Male > 0))
            foreach (var b in metabolic) sb.AppendLine($"  - {b.Label}: F {b.Female} | M {b.Male}");
        else
            sb.AppendLine("  (metabolic risk data not available for this camp)");
        sb.AppendLine();
        sb.AppendLine($"Mental Health overall score: {(mentalScore > 0 ? $"{mentalScore}/10" : "not captured")}");
        sb.AppendLine("Mental Health distribution (%):");
        if (mentalDist.Any(s => s.Value > 0))
            foreach (var s in mentalDist) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        else
            sb.AppendLine("  (mental health assessment data not captured)");
        sb.AppendLine();
        if (painMale.Count > 0)
        {
            sb.AppendLine("Pain Assessment — Male (%):");
            foreach (var s in painMale) sb.AppendLine($"  - {s.Label}: {s.Value}%");
            sb.AppendLine("Pain Assessment — Female (%):");
            foreach (var s in painFemale) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        }
        else
        {
            sb.AppendLine("Pain Assessment: not administered in this screening cycle.");
        }
        sb.AppendLine();
        sb.AppendLine("Lifestyle Risk binary view (%):");
        foreach (var s in lifestyle2) sb.AppendLine($"  - {s.Label}: {s.Value}%");
        sb.AppendLine();
        sb.AppendLine($"Systemic Organ Function — Normal: {organNormal}% | Requires Monitoring: {organMonitor}% | At Risk: {organAtRisk}%");
        sb.AppendLine();
        if (eyeAgg.HasData)
            sb.AppendLine($"Eye/Visual Health — Left eye most-common: {eyeAgg.LeftEyeAcuity} (n={eyeAgg.LeftEyeCount}); Right: {eyeAgg.RightEyeAcuity} (n={eyeAgg.RightEyeCount}).");
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
        sb.AppendLine("IMPORTANT: This is the first screening camp for this organisation. " +
                      "There is no prior-year data. Do NOT fabricate trend deltas or year-on-year comparisons. " +
                      "For trendImprovements/trendDeclines/trendStableAreas write: 'Baseline data established; longitudinal trends will be available after subsequent screenings.'");
        sb.AppendLine();
        sb.AppendLine("Generate the JSON narrative now. introduction is one short paragraph framing the report. executiveOverview / executiveClinicalFindings / executiveRecommendation each 2-4 sentences. conclusion is one closing paragraph. objectivesAndMethods describes purpose and methodology in 2-3 sentences.");

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
