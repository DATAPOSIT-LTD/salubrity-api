using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;
using System.Security.Claims;

namespace Salubrity.Api.Controllers.Camps;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/camps/my")]
[Produces("application/json")]
[Tags("My Camps")]
public class MyCampsController : BaseController
{
    private readonly IMyCampQueryService _service;

    public MyCampsController(IMyCampQueryService service)
    {
        _service = service;
    }

    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MyCampListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcoming(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {

        var userId = GetCurrentUserId();

        var result = await _service.GetUpcomingForUserAsync(userId, page, pageSize, search, ct);
        return Success(result);
    }
}
