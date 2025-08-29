using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Salubrity.Application.Interfaces.Services.Users;

namespace Salubrity.Api.Controllers.HealthCamps;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health-camps")]
[Produces("application/json")]
[Tags("Health Camps Management")]
public class CampController : BaseController
{
    private readonly IHealthCampService _service;
    private readonly IUserService _userService;

    public CampController(IHealthCampService service, IUserService userService)
    {
        _service = service;
        _userService = userService;
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
    [Authorize(Roles = "Subcontractor,Admin")]
    [HttpGet("my/upcoming")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyUpcomingCampsAsync(
    [FromServices] ICurrentSubcontractorService current,
    CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var isAdmin = await _userService.IsInRoleAsync(userId, "Admin");

        // Admins have no subcontractorId → pass null to service
        var subcontractorId = isAdmin ? (Guid?)null : await current.GetSubcontractorIdOrThrowAsync(userId, ct);

        var result = await _service.GetMyUpcomingCampsAsync(subcontractorId);
        return Success(result);
    }

    [Authorize(Roles = "Subcontractor,Admin")]
    [HttpGet("my/complete")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCompleteCampsAsync(
        [FromServices] ICurrentSubcontractorService current,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var isAdmin = await _userService.IsInRoleAsync(userId, "Admin");
        var subcontractorId = isAdmin ? (Guid?)null : await current.GetSubcontractorIdOrThrowAsync(userId, ct);

        var result = await _service.GetMyCompleteCampsAsync(subcontractorId);
        return Success(result);
    }

    [Authorize(Roles = "Subcontractor,Admin")]
    [HttpGet("my/canceled")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCanceledCampsAsync(
        [FromServices] ICurrentSubcontractorService current,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var isAdmin = await _userService.IsInRoleAsync(userId, "Admin");
        var subcontractorId = isAdmin ? (Guid?)null : await current.GetSubcontractorIdOrThrowAsync(userId, ct);

        var result = await _service.GetMyCanceledCampsAsync(subcontractorId);
        return Success(result);
    }



    [HttpGet("{campId:guid}/participants")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsAll(
        Guid campId,
        [FromQuery] string? q,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetCampParticipantsAllAsync(campId, q, sort, page, pageSize);
        return Success(result);
    }

    [HttpGet("{campId:guid}/participants/served")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsServed(
        Guid campId,
        [FromQuery] string? q,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetCampParticipantsServedAsync(campId, q, sort, page, pageSize);
        return Success(result);
    }

    [HttpGet("{campId:guid}/participants/not-seen")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsNotSeen(
        Guid campId,
        [FromQuery] string? q,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetCampParticipantsNotSeenAsync(campId, q, sort, page, pageSize);
        return Success(result);
    }


    [AllowAnonymous] // or [Authorize(Roles = "Admin")] if restricted
    [HttpGet("{campId:guid}/posters/{kind}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCampPosterQr(Guid campId, string kind)
    {
        var folder = $"wwwroot/qrcodes/healthcamps/{campId:N}";
        if (!Directory.Exists(folder))
            return NotFound("QR code folder not found.");

        string? filePath = kind.ToLowerInvariant() switch
        {
            "participant" => Directory.GetFiles(folder, "participant_*.png").LastOrDefault(),
            "subcontractor" => Directory.GetFiles(folder, "subcontractor_*.png").LastOrDefault(),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            return NotFound("QR code file not found.");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(filePath);
        return File(bytes, "image/png", fileName);
    }


    [Authorize(Roles = "Subcontractor,Admin")]
    [HttpGet("my-camp-services/{status:regex(^upcoming$|^complete$|^canceled$)}")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampWithRolesDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCampsByStatus(
    string status,
    [FromServices] ICurrentSubcontractorService current,
    CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var subcontractorId = await current.GetSubcontractorIdOrThrowAsync(userId, ct);

        var result = await _service.GetMyCampsWithRolesByStatusAsync(subcontractorId, status.ToLowerInvariant(), ct);
        return Success(result);
    }


    [Authorize(Roles = "Subcontractor,Admin")]
    [HttpGet("{campId:guid}/patients/all")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampPatientDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampPatientsByStatus(
        Guid campId,
        [FromQuery] string filter = "all",
        [FromQuery] string? q = null,
        [FromQuery] string? sort = "newest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var patients = await _service.GetCampPatientsByStatusAsync(campId, filter, q, sort, page, pageSize, ct);
        return Success(patients);
    }

    [Authorize(Roles = "Subcontractor,Admin")]
    [HttpGet("{campId:guid}/patients/{participantId:guid}/detail-with-forms")]
    [ProducesResponseType(typeof(ApiResponse<CampPatientDetailWithFormsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampPatientDetailWithForms(
    Guid campId,
    Guid participantId,
    [FromServices] ICurrentSubcontractorService current,
    CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var subcontractorId = await current.GetSubcontractorIdOrThrowAsync(userId, ct);

        // Admin: your CurrentSubcontractorService returns Guid.Empty → pass null
        Guid? subIdOrNull = subcontractorId == Guid.Empty ? (Guid?)null : subcontractorId;

        var dto = await _service.GetCampPatientDetailWithFormsForCurrentAsync(
            campId, participantId, subIdOrNull, ct);

        return Success(dto);
    }

    [HttpGet("organization/{organizationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<OrganizationCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampsByOrganization( Guid organizationId, CancellationToken ct = default)
    {
        var result = await _service.GetCampsByOrganizationAsync(organizationId, ct);
        return Success(result);
    }
}
