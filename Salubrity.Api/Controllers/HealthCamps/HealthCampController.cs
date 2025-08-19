using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Salubrity.Api.Controllers.HealthCamps;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health-camps")]
[Produces("application/json")]
[Tags("Health Camps Management")]
public class CampController : BaseController
{
    private readonly IHealthCampService _service;

    public CampController(IHealthCampService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Success(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HealthCampDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HealthCampDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateHealthCampDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HealthCampDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHealthCampDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Success(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Success("Camp deleted.");
    }

    // === SUBCONTRACTOR ASSIGNMENTS ===

    [Authorize(Roles = "Subcontractor")]
    [HttpGet("my/upcoming")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyUpcomingCamps(
    [FromServices] ICurrentSubcontractorService current,
    CancellationToken ct)
    {
        var userId = User.GetUserId();
        var subcontractorId = await current.GetRequiredSubcontractorIdAsync(userId, ct);
        var result = await _service.GetMyUpcomingCampsAsync(subcontractorId);
        return Success(result);
    }

    [Authorize(Roles = "Subcontractor")]
    [HttpGet("my/complete")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCompleteCamps(
        [FromServices] ICurrentSubcontractorService current,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var subcontractorId = await current.GetRequiredSubcontractorIdAsync(userId, ct);
        var result = await _service.GetMyCompleteCampsAsync(subcontractorId);
        return Success(result);
    }

    [Authorize(Roles = "Subcontractor")]
    [HttpGet("my/canceled")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCanceledCamps(
        [FromServices] ICurrentSubcontractorService current,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var subcontractorId = await current.GetRequiredSubcontractorIdAsync(userId, ct);
        var result = await _service.GetMyCanceledCampsAsync(subcontractorId);
        return Success(result);
    }
    [HttpGet("{campId:guid}/participants")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsAll(Guid campId, [FromQuery] string? q, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetCampParticipantsAllAsync(campId, q, sort, page, pageSize);
        return Success(result);
    }

    [HttpGet("{campId:guid}/participants/served")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsServed(Guid campId, [FromQuery] string? q, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetCampParticipantsServedAsync(campId, q, sort, page, pageSize);
        return Success(result);
    }

    [HttpGet("{campId:guid}/participants/not-seen")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsNotSeen(Guid campId, [FromQuery] string? q, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetCampParticipantsNotSeenAsync(campId, q, sort, page, pageSize);
        return Success(result);
    }

}
