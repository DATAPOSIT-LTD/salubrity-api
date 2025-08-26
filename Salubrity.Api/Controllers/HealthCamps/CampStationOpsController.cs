// File: Salubrity.Api/Controllers/Camps/CampStationOpsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.Interfaces.Services.Camps;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = "CampCoordinator,Staff")] // adjust policy
[Route("api/v{version:apiVersion}/camps/stations")]
[Produces("application/json")]
[Tags("Camp Station Ops")]
public class CampStationOpsController : BaseController
{
    private readonly ICampQueueService _service;
    public CampStationOpsController(ICampQueueService service) => _service = service;

    private Guid GetUserId() => GetCurrentUserId();

    [HttpPost("{checkInId:guid}/start")]
    public async Task<IActionResult> Start(Guid checkInId, CancellationToken ct)
    {
        await _service.StartServiceAsync(GetUserId(), checkInId, ct);
        return Success("Started.");
    }

    [HttpPost("{checkInId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid checkInId, CancellationToken ct)
    {
        await _service.CompleteServiceAsync(GetUserId(), checkInId, ct);
        return Success("Completed.");
    }
}
