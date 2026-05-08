using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.Services.Reporting;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Reports;

/// <summary>
/// Patient-facing and admin-facing report endpoints (PDF + JSON preview data).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reports")]
[Tags("Reports")]
[Authorize]
public class ReportsController : BaseController
{
    private readonly IIndividualPreliminaryReportService _preliminaryService;
    private readonly IIndividualFinalReportService _finalService;
    private readonly IHealthCampParticipantRepository _participantRepo;

    public ReportsController(
        IIndividualPreliminaryReportService preliminaryService,
        IIndividualFinalReportService finalService,
        IHealthCampParticipantRepository participantRepo)
    {
        _preliminaryService = preliminaryService;
        _finalService = finalService;
        _participantRepo = participantRepo;
    }

    // ─── JSON preview payloads ─────────────────────────────────────────

    /// <summary>
    /// Returns the JSON payload that drives the Individual Preliminary Report preview.
    /// Use this when you already know the participantId (admin / staff flow).
    /// </summary>
    [HttpGet("individual-preliminary/{participantId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<IndividualPreliminaryReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndividualPreliminary(Guid participantId, CancellationToken ct)
    {
        var dto = await _preliminaryService.BuildAsync(participantId, ct);
        return Success(dto);
    }

    /// <summary>
    /// Same payload as above but resolves the participant from the current user + camp.
    /// </summary>
    [HttpGet("individual-preliminary/by-camp/{campId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<IndividualPreliminaryReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyIndividualPreliminary(Guid campId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var participantId = await _participantRepo.GetParticipantIdByUserAndCampAsync(userId, campId, ct)
            ?? throw new NotFoundException("You are not enrolled as a participant in this camp.");

        var dto = await _preliminaryService.BuildAsync(participantId, ct);
        return Success(dto);
    }

    /// <summary>
    /// JSON payload for the Individual Final Report — composes Preliminary data
    /// with doctor recommendations, referrals, body-map, and signature.
    /// </summary>
    [HttpGet("individual-final/{participantId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<IndividualFinalReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndividualFinal(Guid participantId, CancellationToken ct)
    {
        var dto = await _finalService.BuildAsync(participantId, ct);
        return Success(dto);
    }

    /// <summary>
    /// Same payload as above but resolves the participant from camp id + current user / patient.
    /// When called by an admin/doctor, pass ?participantId=... to target a specific participant.
    /// </summary>
    [HttpGet("individual-final/by-camp/{campId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<IndividualFinalReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndividualFinalByCamp(
        Guid campId,
        [FromQuery] Guid? participantId,
        [FromServices] Salubrity.Application.Interfaces.Repositories.HealthCamps.IHealthCampRepository campRepo,
        CancellationToken ct = default)
    {
        Guid resolvedParticipantId;
        if (participantId.HasValue)
        {
            resolvedParticipantId = participantId.Value;
        }
        else
        {
            // Patient self-view — gate on the camp’s publish flag.
            var camp = await campRepo.GetByIdAsync(campId)
                ?? throw new NotFoundException("Camp not found.");
            if (camp.FinalReportsPublishedAt is null)
                throw new NotFoundException("Final report has not been released for this camp yet.");

            var userId = GetCurrentUserId();
            resolvedParticipantId = await _participantRepo.GetParticipantIdByUserAndCampAsync(userId, campId, ct)
                ?? throw new NotFoundException("You are not enrolled as a participant in this camp.");
        }
        var dto = await _finalService.BuildAsync(resolvedParticipantId, ct);
        return Success(dto);
    }

    // ─── PDF downloads ────────────────────────────────────────────────

    /// <summary>
    /// Returns the rendered Individual Preliminary Report as a PDF download.
    /// </summary>
    [HttpGet("individual-preliminary/{participantId:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndividualPreliminaryPdf(Guid participantId, CancellationToken ct)
    {
        var bytes = await _preliminaryService.BuildPdfAsync(participantId, ct);
        return File(bytes, "application/pdf", "Individual_Preliminary_Report.pdf");
    }

    /// <summary>
    /// PDF variant of the by-camp endpoint — patient downloads their own report.
    /// </summary>
    [HttpGet("individual-preliminary/by-camp/{campId:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyIndividualPreliminaryPdf(Guid campId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var participantId = await _participantRepo.GetParticipantIdByUserAndCampAsync(userId, campId, ct)
            ?? throw new NotFoundException("You are not enrolled as a participant in this camp.");

        var bytes = await _preliminaryService.BuildPdfAsync(participantId, ct);
        return File(bytes, "application/pdf", "Individual_Preliminary_Report.pdf");
    }

    /// <summary>
    /// Rendered Individual Final Report as a PDF download (specific participant).
    /// </summary>
    [HttpGet("individual-final/{participantId:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndividualFinalPdf(Guid participantId, CancellationToken ct)
    {
        var bytes = await _finalService.BuildPdfAsync(participantId, ct);
        return File(bytes, "application/pdf", "Individual_Final_Report.pdf");
    }

    /// <summary>
    /// PDF variant of the by-camp Final endpoint — participant resolved from JWT unless
    /// participantId is passed explicitly by an admin/doctor.
    /// </summary>
    [HttpGet("individual-final/by-camp/{campId:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndividualFinalPdfByCamp(
        Guid campId,
        [FromQuery] Guid? participantId,
        [FromServices] Salubrity.Application.Interfaces.Repositories.HealthCamps.IHealthCampRepository campRepo,
        CancellationToken ct = default)
    {
        Guid resolved;
        if (participantId.HasValue)
        {
            resolved = participantId.Value;
        }
        else
        {
            var camp = await campRepo.GetByIdAsync(campId)
                ?? throw new NotFoundException("Camp not found.");
            if (camp.FinalReportsPublishedAt is null)
                throw new NotFoundException("Final report has not been released for this camp yet.");

            var userId = GetCurrentUserId();
            resolved = await _participantRepo.GetParticipantIdByUserAndCampAsync(userId, campId, ct)
                ?? throw new NotFoundException("You are not enrolled as a participant in this camp.");
        }
        var bytes = await _finalService.BuildPdfAsync(resolved, ct);
        return File(bytes, "application/pdf", "Individual_Final_Report.pdf");
    }

    /// <summary>
    /// Corporate report payload (admin-facing) for an entire camp.
    /// Aggregates KPIs, gender split, station completion + AI-generated narratives.
    /// </summary>
    [HttpGet("corporate/{campId:guid}")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<CorporateReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCorporateReport(
        Guid campId,
        [FromQuery] string? gender,
        [FromQuery] string? age,
        [FromQuery] int? day,
        [FromServices] Salubrity.Application.Interfaces.Services.Reporting.ICorporateReportService svc,
        CancellationToken ct = default)
    {
        var filters = new Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters
        {
            Gender = string.IsNullOrWhiteSpace(gender) || gender == "Any" ? null : gender,
            AgeBucket = string.IsNullOrWhiteSpace(age) || age == "Any" ? null : age,
            Day = day,
        };
        var dto = await svc.BuildAsync(campId, filters, ct);
        return Success(dto);
    }
    /// <summary>Send the preliminary corporate report to one or more recipients with the PDF attached.</summary>
    [HttpPost("corporate/{campId:guid}/send-email")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendCorporateReportEmail(
        Guid campId,
        [FromBody] Salubrity.Application.DTOs.Reports.SendCorporateReportEmailRequest request,
        [FromServices] Salubrity.Application.Interfaces.Services.Reporting.ICorporateReportService svc,
        CancellationToken ct = default)
    {
        await svc.SendEmailAsync(campId, request, ct);
        return Success<object>(new { sent = request?.Recipients?.Count ?? 0 });
    }

    /// <summary>Send the FINAL corporate report to one or more recipients with the PDF attached.</summary>
    [HttpPost("corporate/{campId:guid}/final/send-email")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendFinalCorporateReportEmail(
        Guid campId,
        [FromBody] Salubrity.Application.DTOs.Reports.SendCorporateReportEmailRequest request,
        [FromServices] Salubrity.Application.Interfaces.Services.Reporting.IFinalCorporateReportService svc,
        CancellationToken ct = default)
    {
        await svc.SendEmailAsync(campId, request, ct);
        return Success<object>(new { sent = request?.Recipients?.Count ?? 0 });
    }

    /// <summary>Download corporate report as PDF.</summary>
    [HttpGet("corporate/{campId:guid}/pdf")]
    [Authorize(Roles = "Admin")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetCorporateReportPdf(
        Guid campId,
        [FromQuery] string? gender,
        [FromQuery] string? age,
        [FromQuery] int? day,
        [FromServices] Salubrity.Application.Interfaces.Services.Reporting.ICorporateReportService svc,
        CancellationToken ct = default)
    {
        var filters = new Salubrity.Application.Interfaces.Repositories.Reporting.CorporateReportFilters
        {
            Gender = string.IsNullOrWhiteSpace(gender) || gender == "Any" ? null : gender,
            AgeBucket = string.IsNullOrWhiteSpace(age) || age == "Any" ? null : age,
            Day = day,
        };
        var pdf = await svc.BuildPdfAsync(campId, filters, ct);
        return File(pdf, "application/pdf", $"corporate-report-{campId:N}.pdf");
    }

}