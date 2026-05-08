using QuestPDF.Fluent;
// File: Application/Services/Reporting/Reports/ExcoCorporateReportService.cs
// Real KPIs derived from CorporateReportRepository.LoadAsync (TotalAttendees / Expected / TopFindings)
// + age & gender from FinalCorporateReportRepository. All narratives via Gemini.

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

public sealed class ExcoCorporateReportService : IExcoCorporateReportService
{
    private readonly IHealthCampRepository _campRepo;
    private readonly ICorporateReportRepository _corpRepo;
    private readonly IFinalCorporateReportRepository _finalRepo;
    private readonly IGeminiClient _gemini;
    private readonly Salubrity.Application.Interfaces.IEmailService _email;
    private readonly ILogger<ExcoCorporateReportService> _log;

    public ExcoCorporateReportService(
        IHealthCampRepository campRepo,
        ICorporateReportRepository corpRepo,
        IFinalCorporateReportRepository finalRepo,
        IGeminiClient gemini,
        Salubrity.Application.Interfaces.IEmailService email,
        ILogger<ExcoCorporateReportService> log)
    {
        _campRepo = campRepo;
        _corpRepo = corpRepo;
        _finalRepo = finalRepo;
        _gemini = gemini;
        _email = email;
        _log = log;
    }

    public async Task<byte[]> BuildPdfAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, IEnumerable<string>? excludedSections = null, CancellationToken ct = default)
    {
        var dto = await BuildAsync(campId, filters, ct);
        var doc = new ExcoCorporateReportDocument(dto, excludedSections);
        return doc.GeneratePdf();
    }

    public async Task SendEmailAsync(Guid campId, SendCorporateReportEmailRequest request, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, IEnumerable<string>? excludedSections = null, CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.Recipients is null || request.Recipients.Count == 0)
            throw new ValidationException(["At least one recipient is required."]);

        var dto = await BuildAsync(campId, filters, ct);
        var pdfBytes = await BuildPdfAsync(campId, filters, excludedSections, ct);

        var fileName = $"exco-corporate-report-{(string.IsNullOrWhiteSpace(dto.CampName) ? campId.ToString("N") : dto.CampName.Replace(" ", "-").ToLowerInvariant())}.pdf";
        var attachments = new List<Salubrity.Application.DTOs.Email.EmailAttachment>
        {
            new Salubrity.Application.DTOs.Email.EmailAttachment
            {
                FileName = fileName,
                Content = pdfBytes,
                ContentType = "application/pdf",
            }
        };

        var subject = $"EXCO Corporate Report - {dto.CampName}";
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

        _log.LogInformation("Sent EXCO corporate report for camp {CampId} to {Count} recipients", campId, request.Recipients.Count);
    }

    public async Task<ExcoCorporateReportDto> BuildAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default)
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
        var topFindings = raw?.TopFindings ?? new List<TopFindingDto>();
        var ageBuckets = await _finalRepo.GetAgeBucketsFilteredAsync(campId, filters, ct);

        // Real per-station attendance: count distinct patients who completed at least one form
        // matching the station's keywords for this camp. Replaces the old TopFindings-keyword approach
        // which returned 0 whenever the station's name wasn't already in the top findings.
        var counts = await _finalRepo.GetExcoCategoryCountsAsync(campId, filters, ct);
        int denom = Math.Max(counts.TotalAttendees, expected);
        int Pct(int n) => denom > 0 ? (int)Math.Round(n * 100.0 / denom) : 0;

        ExcoKpiDto StationKpi(string title, int n) => new ExcoKpiDto
        {
            Title = title,
            Percentage = $"{Pct(n)}%",
            Attendance = $"{n}/{denom}",
            Trend = "Stable",
            Notes = string.Empty,
        };

        var kpiEngagement = new ExcoKpiDto
        {
            Title = "Camp Engagement & Turnout",
            Percentage = $"{participationRate}%",
            Attendance = $"{totalAttendees}/{expected}",
            Trend = "Stable",
            Notes = string.Empty,
        };
        var kpiVision    = StationKpi("Vision & productivity Visual issues", counts.Vision);
        var kpiHighBp    = StationKpi("Cardiometabolic (High BP)",          counts.Bp);
        var kpiCdmp      = StationKpi("Care navigation CDMP recommended",   counts.Cdmp);
        var kpiPreDiab   = StationKpi("Cardiometabolic (Pre-diabetes)",     counts.PreDiabetes);
        var kpiMh        = StationKpi("Mental health (MH Flags)",           counts.MentalHealth);

        var narratives = await GenerateExcoNarrativesAsync(
            camp.Name, clientName, dateRange, expected, totalAttendees, participationRate, femalePct, malePct,
            ageBuckets, topFindings,
            kpiEngagement, kpiVision, kpiHighBp, kpiCdmp, kpiPreDiab, kpiMh, ct);

        // Apply Gemini-generated trend + notes back to each KPI
        kpiEngagement.Notes = narratives.KpiEngagementNotes;
        kpiEngagement.Trend = narratives.KpiEngagementTrend;
        kpiVision.Notes = narratives.KpiVisionNotes;
        kpiVision.Trend = narratives.KpiVisionTrend;
        kpiHighBp.Notes = narratives.KpiHighBpNotes;
        kpiHighBp.Trend = narratives.KpiHighBpTrend;
        kpiCdmp.Notes = narratives.KpiCdmpNotes;
        kpiCdmp.Trend = narratives.KpiCdmpTrend;
        kpiPreDiab.Notes = narratives.KpiPreDiabetesNotes;
        kpiPreDiab.Trend = narratives.KpiPreDiabetesTrend;
        kpiMh.Notes = narratives.KpiMentalHealthNotes;
        kpiMh.Trend = narratives.KpiMentalHealthTrend;

        return new ExcoCorporateReportDto
        {
            CampId = campId,
            CampName = camp.Name,
            ClientName = clientName,
            ClientContactName = clientName,
            ClientContactEmail = clientEmail,
            DateRange = dateRange,
            GeneratedAt = DateTime.UtcNow,

            CampEngagementTurnout = kpiEngagement,
            VisionProductivity = kpiVision,
            CardiometabolicHighBp = kpiHighBp,
            CareNavigationCdmp = kpiCdmp,
            CardiometabolicPreDiabetes = kpiPreDiab,
            MentalHealthFlags = kpiMh,

            WellnessHeadlines = new ExcoWellnessHeadlinesDto
            {
                Engagement = narratives.HeadlineEngagement,
                HealthRiskBurden = narratives.HeadlineHealthRiskBurden,
                RiskIdentificationAndTriage = narratives.HeadlineRiskIdentification,
                HealthRiskBurdenSecondary = narratives.HeadlineHealthRiskBurdenSecondary,
            },

            StrategicRiskSnapshot = new ExcoStrategicRiskSnapshotDto
            {
                ParticipationSummary = narratives.ParticipationSummary,
                AgeProfileSummary = narratives.AgeProfileSummary,
                HighestBurdenDomains = narratives.HighestBurdenDomains,
                FutureMedicalCosts = narratives.FutureMedicalCosts,
                PresenteeismAndOutput = narratives.PresenteeismAndOutput,
                SafetyAndQualityRisk = narratives.SafetyAndQualityRisk,
                EmployerBrandRisk = narratives.EmployerBrandRisk,
            },

            EarlyPositives = new ExcoEarlyPositivesDto
            {
                HigherFutureMedicalCosts = narratives.EarlyPositiveFutureMedical,
                AgeProfile = narratives.EarlyPositiveAgeProfile,
            },
        };
    }

    // ---- Gemini ----

    private sealed class ExcoNarratives
    {
        public string KpiEngagementNotes { get; set; } = string.Empty;
        public string KpiEngagementTrend { get; set; } = "Stable";
        public string KpiVisionNotes { get; set; } = string.Empty;
        public string KpiVisionTrend { get; set; } = "Stable";
        public string KpiHighBpNotes { get; set; } = string.Empty;
        public string KpiHighBpTrend { get; set; } = "Stable";
        public string KpiCdmpNotes { get; set; } = string.Empty;
        public string KpiCdmpTrend { get; set; } = "Stable";
        public string KpiPreDiabetesNotes { get; set; } = string.Empty;
        public string KpiPreDiabetesTrend { get; set; } = "Stable";
        public string KpiMentalHealthNotes { get; set; } = string.Empty;
        public string KpiMentalHealthTrend { get; set; } = "Stable";

        public string HeadlineEngagement { get; set; } = string.Empty;
        public string HeadlineHealthRiskBurden { get; set; } = string.Empty;
        public string HeadlineRiskIdentification { get; set; } = string.Empty;
        public string HeadlineHealthRiskBurdenSecondary { get; set; } = string.Empty;

        public string ParticipationSummary { get; set; } = string.Empty;
        public string AgeProfileSummary { get; set; } = string.Empty;
        public string HighestBurdenDomains { get; set; } = string.Empty;
        public string FutureMedicalCosts { get; set; } = string.Empty;
        public string PresenteeismAndOutput { get; set; } = string.Empty;
        public string SafetyAndQualityRisk { get; set; } = string.Empty;
        public string EmployerBrandRisk { get; set; } = string.Empty;

        public string EarlyPositiveFutureMedical { get; set; } = string.Empty;
        public string EarlyPositiveAgeProfile { get; set; } = string.Empty;
    }

    private async Task<ExcoNarratives> GenerateExcoNarrativesAsync(
        string campName, string clientName, string dateRange,
        int expected, int totalAttendees, int participationRate, int femalePct, int malePct,
        List<AgeBucketDto> ageBuckets, List<TopFindingDto> topFindings,
        ExcoKpiDto kEng, ExcoKpiDto kVis, ExcoKpiDto kBp, ExcoKpiDto kCdmp, ExcoKpiDto kPre, ExcoKpiDto kMh,
        CancellationToken ct)
    {
        var system =
            "You are writing the narrative sections of an EXCO (executive committee) Corporate Health Report for a client organisation. " +
            "Audience: senior leadership/HR/executive readers. Tone: concise, business-impact focused, factual. " +
            "Constraints: " +
            "(1) use ONLY the facts in the user prompt; do not invent statistics or diagnoses; " +
            "(2) write each value in 2-4 plain-text sentences (no markdown, no bullet characters except where noted); " +
            "(3) DO NOT use first-person pronouns; use impersonal voice; " +
            "(4) refer to the client by name where natural; " +
            "(5) for the *kpi…Trend keys, output exactly one of: Increase, Decrease, Stable. Default to Stable when no trend basis is provided; " +
            "(6) for highestBurdenDomains, output a newline-separated bullet list using a leading dash '- ' on each line, ranked by prevalence; " +
            "(7) RESPOND WITH A JSON OBJECT containing exactly these string keys: " +
            "kpiEngagementNotes, kpiEngagementTrend, kpiVisionNotes, kpiVisionTrend, kpiHighBpNotes, kpiHighBpTrend, " +
            "kpiCdmpNotes, kpiCdmpTrend, kpiPreDiabetesNotes, kpiPreDiabetesTrend, kpiMentalHealthNotes, kpiMentalHealthTrend, " +
            "headlineEngagement, headlineHealthRiskBurden, headlineRiskIdentification, headlineHealthRiskBurdenSecondary, " +
            "participationSummary, ageProfileSummary, highestBurdenDomains, " +
            "futureMedicalCosts, presenteeismAndOutput, safetyAndQualityRisk, employerBrandRisk, " +
            "earlyPositiveFutureMedical, earlyPositiveAgeProfile. " +
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
        sb.AppendLine("KPIs computed from the data:");
        void K(ExcoKpiDto k) => sb.AppendLine($"  - {k.Title}: {k.Percentage} ({k.Attendance})");
        K(kEng); K(kVis); K(kBp); K(kCdmp); K(kPre); K(kMh);
        sb.AppendLine();
        sb.AppendLine("Top clinical findings recorded for this camp (n / prevalence% / severity):");
        if (topFindings.Count == 0)
            sb.AppendLine("  (none recorded — KPIs above are zero by default)");
        else
            foreach (var f in topFindings.Take(15))
                sb.AppendLine($"  - {f.Name}: n={f.N}, {f.Pct}%, severity={f.Level}");
        sb.AppendLine();
        sb.AppendLine("Generate the JSON narrative now. " +
                      "Each kpi…Notes value: 2-3 sentences explaining what the KPI means for the workforce and what action it implies. " +
                      "headline* values: punchy executive-readable sentences. " +
                      "businessRisk* and earlyPositive* values: 2-3 sentences each linking the data to a specific business consequence or strength.");

        string raw = string.Empty;
        try
        {
            raw = await _gemini.GenerateJsonAsync(system, sb.ToString(), temperature: 0.35, maxOutputTokens: 6000, ct: ct);
            var json = ExtractJsonObject(raw);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string read(string key) => root.TryGetProperty(key, out var v) ? (v.GetString() ?? string.Empty).Trim() : string.Empty;
            string trend(string key) {
                var t = read(key);
                return t == "Increase" || t == "Decrease" || t == "Stable" ? t : "Stable";
            }
            return new ExcoNarratives
            {
                KpiEngagementNotes = read("kpiEngagementNotes"),     KpiEngagementTrend = trend("kpiEngagementTrend"),
                KpiVisionNotes = read("kpiVisionNotes"),               KpiVisionTrend = trend("kpiVisionTrend"),
                KpiHighBpNotes = read("kpiHighBpNotes"),               KpiHighBpTrend = trend("kpiHighBpTrend"),
                KpiCdmpNotes = read("kpiCdmpNotes"),                   KpiCdmpTrend = trend("kpiCdmpTrend"),
                KpiPreDiabetesNotes = read("kpiPreDiabetesNotes"),     KpiPreDiabetesTrend = trend("kpiPreDiabetesTrend"),
                KpiMentalHealthNotes = read("kpiMentalHealthNotes"),   KpiMentalHealthTrend = trend("kpiMentalHealthTrend"),

                HeadlineEngagement = read("headlineEngagement"),
                HeadlineHealthRiskBurden = read("headlineHealthRiskBurden"),
                HeadlineRiskIdentification = read("headlineRiskIdentification"),
                HeadlineHealthRiskBurdenSecondary = read("headlineHealthRiskBurdenSecondary"),

                ParticipationSummary = read("participationSummary"),
                AgeProfileSummary = read("ageProfileSummary"),
                HighestBurdenDomains = read("highestBurdenDomains"),
                FutureMedicalCosts = read("futureMedicalCosts"),
                PresenteeismAndOutput = read("presenteeismAndOutput"),
                SafetyAndQualityRisk = read("safetyAndQualityRisk"),
                EmployerBrandRisk = read("employerBrandRisk"),

                EarlyPositiveFutureMedical = read("earlyPositiveFutureMedical"),
                EarlyPositiveAgeProfile = read("earlyPositiveAgeProfile"),
            };
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "EXCO Gemini narratives parse failure. Raw: {Raw}", raw);
            const string F = "Narrative generation is currently unavailable. Please retry shortly or write this section manually.";
            return new ExcoNarratives
            {
                KpiEngagementNotes = F, KpiVisionNotes = F, KpiHighBpNotes = F, KpiCdmpNotes = F,
                KpiPreDiabetesNotes = F, KpiMentalHealthNotes = F,
                HeadlineEngagement = F, HeadlineHealthRiskBurden = F, HeadlineRiskIdentification = F, HeadlineHealthRiskBurdenSecondary = F,
                ParticipationSummary = F, AgeProfileSummary = F, HighestBurdenDomains = F,
                FutureMedicalCosts = F, PresenteeismAndOutput = F, SafetyAndQualityRisk = F, EmployerBrandRisk = F,
                EarlyPositiveFutureMedical = F, EarlyPositiveAgeProfile = F,
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
