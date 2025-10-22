using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.Users;
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
        Guid userId = GetCurrentUserId();
        await _service.DeleteAsync(id, userId);
        return Success("Camp deleted.");
    }

    // === SUBCONTRACTOR ASSIGNMENTS ===
    [Authorize(Roles = "Subcontractor,Doctor,Concierge,Admin")]
    [HttpGet("my/upcoming")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyUpcomingCampsAsync(
    [FromServices] ICurrentSubcontractorService current,
    CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var isAdmin = await _userService.IsInRoleAsync(userId, "Admin");
        var isConcierge = await _userService.IsInRoleAsync(userId, "Concierge");
        var isDoctor = await _userService.IsInRoleAsync(userId, "Doctor");

        // Admins have no subcontractorId → pass null to service
        var subcontractorId = (isAdmin || isConcierge || isDoctor) ? (Guid?)null : await current.GetSubcontractorIdOrThrowAsync(userId, ct);

        var result = await _service.GetMyUpcomingCampsAsync(subcontractorId);
        return Success(result);
    }

    [Authorize(Roles = "Concierge,Doctor,Subcontractor,Admin")]
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

    [Authorize(Roles = "Concierge,Doctor,Subcontractor,Admin")]
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

    [Authorize(Roles = "Concierge,Doctor,Subcontractor,Admin")]
    [HttpGet("my/ongoing")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthCampListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOgoingCampsAsync(
    [FromServices] ICurrentSubcontractorService current,
    CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var isConcierge = await _userService.IsInRoleAsync(userId, "Concierge");
        var isDoctor = await _userService.IsInRoleAsync(userId, "Doctor");
        var isAdmin = await _userService.IsInRoleAsync(userId, "Admin");

        var subcontractorId = (isConcierge || isDoctor || isAdmin)
            ? (Guid?)null
            : await current.GetSubcontractorIdOrThrowAsync(userId, ct);

        var result = await _service.GetMyOngoingCampsAsync(subcontractorId);
        return Success(result);
    }


    // [Authorize(Roles = "Admin")]
    [HttpPost("{campId:guid}/participants")]
    [ProducesResponseType(typeof(ApiResponse<CampLinkResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddParticipantToCamp(Guid campId, [FromBody] AddParticipantRequest dto, CancellationToken ct)
    {
        var result = await _service.LinkUserToCampByIdAsync(dto.UserId, campId, ct);
        return Success(result, "Participant processed.");
    }



    [HttpGet("{campId:guid}/participants")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsAll(
     Guid campId,
     [FromQuery] Guid? serviceAssignmentId,
     [FromQuery] string? q,
     [FromQuery] string? sort,
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 20,
     CancellationToken ct = default)
    {
        var result = await _service.GetCampParticipantsAllAsync(campId, serviceAssignmentId, q, sort, page, pageSize, ct);
        return Success(result);
    }

    [HttpGet("{campId:guid}/participants/served")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsServed(
        Guid campId,
        [FromQuery] Guid? serviceAssignmentId,
        [FromQuery] string? q,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _service.GetCampParticipantsServedAsync(campId, serviceAssignmentId, q, sort, page, pageSize, ct);
        return Success(result);
    }

    [HttpGet("{campId:guid}/participants/not-seen")]
    [ProducesResponseType(typeof(ApiResponse<List<CampParticipantListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampParticipantsNotSeen(
        Guid campId,
        [FromQuery] Guid? serviceAssignmentId,
        [FromQuery] string? q,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _service.GetCampParticipantsNotSeenAsync(campId, serviceAssignmentId, q, sort, page, pageSize, ct);
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

    [AllowAnonymous] // or restrict if needed
    [HttpPost("posters/details")]
    [ProducesResponseType(typeof(ApiResponse<QrEncodingDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPosterDetails([FromBody] DecodePosterTokenRequest request, CancellationToken ct)
    {
        var result = await _service.DecodePosterTokenAsync(request.Token, ct);
        return Success(result);
    }



    [Authorize(Roles = "Concierge,Subcontractor,Admin")]
    [HttpGet("my-camp-services/{status:regex(^upcoming$|^complete$|^canceled$|^ongoing$)}")]
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

    [Authorize(Roles = "Subcontractor,Doctor,Admin,Concierge")]
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
    public async Task<IActionResult> GetCampsByOrganization(Guid organizationId, CancellationToken ct = default)
    {
        var result = await _service.GetCampsByOrganizationAsync(organizationId, ct);
        return Success(result);
    }

    [HttpGet("organization/{organizationId:guid}/stats")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationStats(
    Guid organizationId,
    CancellationToken ct = default)
    {
        var result = await _service.GetOrganizationStatsAsync(organizationId, ct);
        return Success(result);
    }

    [HttpGet("camps/upcoming-dates")]
    [ProducesResponseType(typeof(ApiResponse<List<DateTime>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingCampDates(CancellationToken ct = default)
    {
        var result = await _service.GetUpcomingCampDatesAsync(ct);
        return Success(result);
    }

    [HttpPut("{campId:guid}/participant/{participantId:guid}/billing-status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateParticipantBillingStatus(Guid campId, Guid participantId, UpdateParticipantBillingStatusDto dto, CancellationToken ct)
    {
        await _service.UpdateParticipantBillingStatusAsync(campId, participantId, dto, ct);
        return Success("Participant billing status updated.");
    }

    [HttpGet("{campId:guid}/participant/{participantId:guid}/billing-status")]
    [ProducesResponseType(typeof(ApiResponse<ParticipantBillingStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParticipantBillingStatus(Guid campId, Guid participantId, CancellationToken ct)
    {
        var result = await _service.GetParticipantBillingStatusAsync(campId, participantId, ct);
        return Success(result);
    }

    // ───────────────────────────────────────────────
    // 🧱 Subcontractor Assignment Management
    // ───────────────────────────────────────────────
    [Authorize(Roles = "Admin,Concierge")]
    [HttpPost("{campId:guid}/subcontractors/add")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddSubcontractorToCamp(
        Guid campId,
        [FromBody] ModifySubcontractorCampDto dto,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _service.AddSubcontractorToCampAsync(campId, dto, userId);
        return Success("Subcontractor successfully added to camp.");
    }

    [Authorize(Roles = "Admin,Concierge")]
    [HttpDelete("{campId:guid}/subcontractors/{subcontractorId:guid}/remove")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveSubcontractorFromCamp(
        Guid campId,
        Guid subcontractorId,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _service.RemoveSubcontractorFromCampAsync(campId, subcontractorId, userId);
        return Success("Subcontractor removed from camp successfully.");
    }


    [Authorize(Roles = "Admin,Concierge")]
    [HttpPost("participants/assign-package")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignParticipantPackage([FromBody] AssignParticipantPackageDto dto, CancellationToken ct)
    {
        await _service.AssignPackageToParticipantAsync(dto, ct);
        return Success("Package assigned successfully.");
    }
    [Authorize(Roles = "Admin,Concierge,Doctor,FrontDesk")]
    [HttpGet("{campId:guid}/packages")]
    [ProducesResponseType(typeof(ApiResponse<List<Application.DTOs.HealthCamps.HealthCampPackageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPackagesByCamp(Guid campId, CancellationToken ct)
    {
        var result = await _service.GetAllPackagesByCampAsync(campId, ct);
        return Success(result);
    }


}
