using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Extensions;
using Salubrity.Shared.Responses;
using System.Security.Claims;

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

    [HttpGet("my/upcoming")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyUpcomingCamps()
    {
        var subcontractorId = User.GetSubcontractorId(); // assumes extension method exists
        var result = await _service.GetMyUpcomingCampsAsync(subcontractorId);
        return Success(result);
    }

    [HttpGet("my/complete")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCompleteCamps()
    {
        var subcontractorId = User.GetSubcontractorId();
        var result = await _service.GetMyCompleteCampsAsync(subcontractorId);
        return Success(result);
    }

    [HttpGet("my/canceled")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCanceledCamps()
    {
        var subcontractorId = User.GetSubcontractorId();
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
