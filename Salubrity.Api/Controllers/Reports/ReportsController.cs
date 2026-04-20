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
    private readonly IHealthCampParticipantRepository _participantRepo;

    public ReportsController(
        IIndividualPreliminaryReportService preliminaryService,
        IHealthCampParticipantRepository participantRepo)
    {
        _preliminaryService = preliminaryService;
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
}
