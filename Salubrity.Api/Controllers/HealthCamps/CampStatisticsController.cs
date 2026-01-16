using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.HealthCamps;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health-camps/{campId:guid}/stats")]
[Produces("application/json")]
[Tags("Health Camp Statistics")]
[Authorize(Roles = "Concierge,Doctor,Subcontractor,Admin")]
public class CampStatsController : BaseController
{
    private readonly ICampStatisticsService _service;

    public CampStatsController(ICampStatisticsService service)
    {
        _service = service;
    }

    // =========================================================
    // OVERALL CAMP STATISTICS (Dashboard / Donut)
    // =========================================================
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CampStatsResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampStats(
        Guid campId,
        CancellationToken ct = default)
    {
        var result = await _service.GetCampStatsAsync(campId, ct);
        return Success(result);
    }
}
