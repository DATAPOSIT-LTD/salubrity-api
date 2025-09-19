// File: Salubrity.Api/Controllers/Camps/MyCampQueueController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.Camps;
using Salubrity.Application.Interfaces.Services.Users;
using Salubrity.Shared.Responses;
using System.Security.Claims;

namespace Salubrity.Api.Controllers.Camps;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = "Patient")] // adjust as needed
[Route("api/v{version:apiVersion}/camps/my")]
[Produces("application/json")]
[Tags("My Camp Queue")]
public class MyCampQueueController : BaseController
{
    private readonly ICampQueueService _service;
    public MyCampQueueController(ICampQueueService service) => _service = service;

    private Guid GetUserId() => GetCurrentUserId();

    [HttpPost("{campId:guid}/service-stations/{assignmentId:guid}/check-in")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckIn(Guid campId, Guid assignmentId, [FromBody] CheckInRequestDto? body, CancellationToken ct)
    {
        var userId = GetUserId();
        var dto = new CheckInRequestDto { CampId = campId, AssignmentId = assignmentId, Priority = body?.Priority };
        await _service.CheckInAsync(userId, dto, ct);
        return Success("Checked in.");
    }

    [HttpPost("{campId:guid}/cancel-check-in")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid campId, CancellationToken ct)
    {
        await _service.CancelMyCheckInAsync(GetUserId(), campId, ct);
        return Success("Canceled.");
    }

    [HttpGet("{campId:guid}/check-in-state")]
    [ProducesResponseType(typeof(ApiResponse<CheckInStateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MyState(Guid campId, CancellationToken ct)
    {
        var state = await _service.GetMyCheckInStateAsync(GetUserId(), campId, ct);
        return Success(state);
    }

    [HttpGet("{campId:guid}/service-stations/{assignmentId:guid}/position")]
    [ProducesResponseType(typeof(ApiResponse<QueuePositionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MyPosition(Guid campId, Guid assignmentId, CancellationToken ct)
    {
        var pos = await _service.GetMyPositionAsync(GetUserId(), campId, assignmentId, ct);
        return Success(pos);
    }

    [Authorize(Roles = "Subcontractor,Concierge,Admin")]
    [HttpGet("{campId:guid}/my-queue")]
    [ProducesResponseType(typeof(ApiResponse<List<MyQueuedParticipantDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyQueue(
    Guid campId,
    [FromServices] ICurrentSubcontractorService current,
    [FromServices] IUserService userService,
    CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        // Check if user is Admin or Concierge
        var isAdmin = await userService.IsInRoleAsync(userId, "Admin");
        var isConcierge = await userService.IsInRoleAsync(userId, "Concierge");

        // Subcontractor ID if required
        var subcontractorId = (isAdmin || isConcierge)
            ? (Guid?)null
            : await current.GetSubcontractorIdOrThrowAsync(userId, ct);

        var queue = await _service.GetMyQueueAsync(userId, campId, subcontractorId, ct);
        return Success(queue);
    }

}
