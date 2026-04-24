// File: Api/Controllers/Clinical/ServiceReferralsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Clinical;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = "Subcontractor,Doctor,Admin")]
[Route("api/v{version:apiVersion}/service-referrals")]
[Produces("application/json")]
[Tags("Service Referrals")]
public class ServiceReferralsController : BaseController
{
    private readonly IServiceReferralService _service;

    public ServiceReferralsController(IServiceReferralService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceReferralResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Success(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ServiceReferralResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid healthCampId,
        [FromQuery] Guid? participantId,
        CancellationToken ct = default)
    {
        var result = participantId.HasValue
            ? await _service.GetByParticipantAndCampAsync(participantId.Value, healthCampId, ct)
            : await _service.GetByCampAsync(healthCampId, ct);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateServiceReferralDto dto, CancellationToken ct = default)
    {
        var providerId = GetCurrentUserId();
        var id = await _service.CreateAsync(dto, providerId, ct);
        return Success(id, "Referral created");
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceReferralDto dto, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _service.UpdateAsync(id, dto, userId, ct);
        return SuccessMessage("Referral updated");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _service.DeleteAsync(id, userId, ct);
        return SuccessMessage("Referral deleted");
    }
}
