using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.Services.Reporting;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Reports;

/// <summary>
/// Patient-facing and admin-facing report endpoints (PDF + JSON preview data).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reports")]
[Produces("application/json")]
[Tags("Reports")]
[Authorize]
public class ReportsController : BaseController
{
    private readonly IIndividualPreliminaryReportService _preliminaryService;

    public ReportsController(IIndividualPreliminaryReportService preliminaryService)
    {
        _preliminaryService = preliminaryService;
    }

    /// <summary>
    /// Returns the JSON payload that drives the Individual Preliminary Report preview.
    /// Use this to populate the on-screen layout before printing or downloading the PDF.
    /// </summary>
    [HttpGet("individual-preliminary/{participantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IndividualPreliminaryReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndividualPreliminary(Guid participantId, CancellationToken ct)
    {
        var dto = await _preliminaryService.BuildAsync(participantId, ct);
        return Success(dto);
    }
}
