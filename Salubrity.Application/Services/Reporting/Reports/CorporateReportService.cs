using QuestPDF.Fluent;
// File: Application/Services/Reporting/Reports/CorporateReportService.cs
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.AI;
using Salubrity.Application.Interfaces.Repositories.Reporting;
using Salubrity.Application.Interfaces.Services.Reporting;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Reporting.Reports;

/// <summary>
/// Aggregates a camp's high-level corporate report metrics via the repo and asks
/// Gemini for the narrative blocks in a single JSON-mode call.
/// Note: Critical Findings split + Top Findings prevalence are not yet computed.
/// </summary>
public sealed class CorporateReportService : ICorporateReportService
{
    private readonly ICorporateReportRepository _repo;
    private readonly IGeminiClient _gemini;
    private readonly Salubrity.Application.Interfaces.IEmailService _email;
    private readonly ILogger<CorporateReportService> _log;

    public CorporateReportService(
        ICorporateReportRepository repo,
        IGeminiClient gemini,
        Salubrity.Application.Interfaces.IEmailService email,
        ILogger<CorporateReportService> log)
    {
        _repo = repo;
        _gemini = gemini;
        _email = email;
        _log = log;
    }

    public async Task SendEmailAsync(Guid campId, SendCorporateReportEmailRequest request, CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.Recipients is null || request.Recipients.Count == 0)
            throw new ValidationException(["At least one recipient is required."]);

        var dto = await BuildAsync(campId, null, ct);
        var pdfBytes = await BuildPdfAsync(campId, null, ct);

        var slug = string.IsNullOrWhiteSpace(dto.CampName)
            ? campId.ToString("N")
            : dto.CampName.Replace(" ", "-").ToLowerInvariant();
        var fileName = $"corporate-report-{slug}.pdf";
        var attachments = new List<Salubrity.Application.DTOs.Email.EmailAttachment>
        {
            new Salubrity.Application.DTOs.Email.EmailAttachment
            {
                FileName = fileName,
                Content = pdfBytes,
                ContentType = "application/pdf",
            }
        };

        var subject = $"Preliminary Corporate Report - {dto.CampName}";
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

        _log.LogInformation("Sent PRELIMINARY corporate report for camp {CampId} to {Count} recipients", campId, request.Recipients.Count);
    }

    public async Task<CorporateReportDto> BuildAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
        var raw = await _repo.LoadAsync(campId, filters, ct)
            ?? throw new NotFoundException("Camp not found");

        var totalAttendees = raw.TotalAttendees;
        var expected = raw.ExpectedParticipants;
        var participationRate = expected > 0
            ? Math.Min(100, (int)Math.Round(totalAttendees * 100.0 / expected))
            : 0;

        var femalePct = totalAttendees > 0
            ? (int)Math.Round(raw.Female * 100.0 / totalAttendees) : 0;
        var malePct = totalAttendees > 0 ? Math.Max(0, 100 - femalePct) : 0;

        var dateRange = raw.EndDate.HasValue && raw.EndDate.Value.Date != raw.StartDate.Date
            ? $"{raw.StartDate:dd MMM} - {raw.EndDate.Value:dd MMM, yyyy}"
            : raw.StartDate.ToString("dd MMM, yyyy");

        var narratives = await GenerateNarrativesAsync(
            clientName: string.IsNullOrWhiteSpace(raw.ClientName) ? "the organisation" : raw.ClientName,
            campName: raw.CampName,
            packageName: raw.PackageName,
            dateRange: dateRange,
            totalAttendees: totalAttendees,
            expected: expected,
            participationRate: participationRate,
            femalePct: femalePct,
            malePct: malePct,
            totalServices: raw.TotalServices,
            stationCompletion: raw.StationCompletion,
            ct: ct);

        return new CorporateReportDto
        {
            CampId = campId,
            CampName = raw.CampName,
            ClientName = raw.ClientName,
            PackageName = raw.PackageName,
            Venue = raw.Venue,
            DateRange = dateRange,
            CampStartDate = raw.StartDate,
            CampEndDate = raw.EndDate,
            GeneratedAt = DateTime.UtcNow,
            ParticipationRate = participationRate,
            TotalAttendees = totalAttendees,
            TotalServices = raw.TotalServices,
            ParticipationTrend = 0, // TODO Phase 3: compare to previous camp by same client
            Attendance = new CorporateAttendanceDto
            {
                Female = raw.Female,
                Male = raw.Male,
                FemalePercent = femalePct,
                MalePercent = malePct,
            },
            StationCompletion = raw.StationCompletion,
            TopFindings = raw.TopFindings,
            Narratives = narratives,
        };
    }

    private async Task<CorporateNarrativesDto> GenerateNarrativesAsync(
        string clientName, string campName, string packageName, string dateRange,
        int totalAttendees, int expected, int participationRate,
        int femalePct, int malePct, int totalServices,
        List<StationCompletionDto> stationCompletion,
        CancellationToken ct)
    {
        var systemInstruction =
            "You are writing the narrative sections of a corporate occupational-health screening report " +
            "for a client organisation. Constraints: " +
            "(1) use ONLY the facts in the user prompt — do not invent statistics or diagnoses; " +
            "(2) write each section in 2-4 plain-text sentences (no markdown, no bullets unless explicitly requested); " +
            "(3) tone: professional, factual, suitable for an HR/management audience; " +
            "(4) DO NOT use first-person pronouns (no 'I', 'we', 'my', 'our'); use impersonal voice; " +
            "(5) refer to the client by name where natural; " +
            "(6) RESPOND WITH A JSON OBJECT containing exactly these string keys: " +
            "overview, clinicalFindings, attendanceNotes, criticalFindingsNotes, stationSummary, topFindingsSummary, overallConclusion, whatHappensNext. " +
            "No other keys, no surrounding text.";

        var sb = new StringBuilder();
        sb.AppendLine($"Client: {clientName}");
        sb.AppendLine($"Camp: {campName}");
        sb.AppendLine($"Package: {packageName}");
        sb.AppendLine($"Date(s): {dateRange}");
        sb.AppendLine($"Expected participants: {expected}");
        sb.AppendLine($"Total attendees: {totalAttendees}");
        sb.AppendLine($"Participation rate: {participationRate}%");
        sb.AppendLine($"Gender split — Female: {femalePct}% | Male: {malePct}%");
        sb.AppendLine($"Total service stations: {totalServices}");
        sb.AppendLine();
        sb.AppendLine("Per-station completion (% of each gender that visited the station):");
        foreach (var s in stationCompletion.Take(15))
            sb.AppendLine($"  - {s.Name}: Female {s.Female}% | Male {s.Male}%");
        sb.AppendLine();
        sb.AppendLine("Generate the JSON narrative now. Each value must be 2-4 sentences except whatHappensNext which can be a single paragraph listing 2-4 next-step items in plain prose (no bullets/numbers).");

        string raw = string.Empty;
        try
        {
            raw = await _gemini.GenerateJsonAsync(systemInstruction, sb.ToString(), temperature: 0.35, maxOutputTokens: 4000, ct: ct);
            var json = ExtractJsonObject(raw);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string read(string key) => root.TryGetProperty(key, out var v) ? (v.GetString() ?? string.Empty).Trim() : string.Empty;

            return new CorporateNarrativesDto
            {
                Overview = read("overview"),
                ClinicalFindings = read("clinicalFindings"),
                AttendanceNotes = read("attendanceNotes"),
                CriticalFindingsNotes = read("criticalFindingsNotes"),
                StationSummary = read("stationSummary"),
                TopFindingsSummary = read("topFindingsSummary"),
                OverallConclusion = read("overallConclusion"),
                WhatHappensNext = read("whatHappensNext"),
            };
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to parse Gemini narrative JSON. Raw response was: {Raw}", raw);
            return new CorporateNarrativesDto
            {
                Overview = "Narrative generation is currently unavailable. Please retry shortly or write the overview manually.",
                ClinicalFindings = "Narrative generation is currently unavailable. Please retry shortly or write the clinical findings manually.",
                AttendanceNotes = "Notes unavailable.",
                CriticalFindingsNotes = "Notes unavailable.",
                StationSummary = "Summary unavailable.",
                TopFindingsSummary = "Summary unavailable.",
                OverallConclusion = "Conclusion unavailable.",
                WhatHappensNext = "Next-step narrative unavailable.",
            };
        }
    }

    /// <summary>
    /// Best-effort extraction of a JSON object from a model response: strips ```json fences
    /// if present, then narrows to the first '{' through the matching last '}'.
    /// </summary>
    private static string ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "{}";
        var t = text.Trim();
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
    public async Task<byte[]> BuildPdfAsync(Guid campId, Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
        var dto = await BuildAsync(campId, filters, ct);
        var doc = new CorporateReportDocument(dto);
        return doc.GeneratePdf();
    }

}