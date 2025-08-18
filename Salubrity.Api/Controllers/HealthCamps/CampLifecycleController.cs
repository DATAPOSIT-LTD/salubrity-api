// File: Salubrity.Api/Controllers/HealthCamps/CampLifecycleController.cs
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health-camps")]
[Produces("application/json")]
[Tags("Health Camps")]
public class CampLifecycleController : BaseController
{
    private readonly IHealthCampService _svc;
    public CampLifecycleController(IHealthCampService svc) => _svc = svc;

    [HttpPost("{id:guid}/launch")]
    [ProducesResponseType(typeof(ApiResponse<LaunchHealthCampResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Launch(Guid id, [FromBody] LaunchHealthCampDto dto)
    {
        dto.HealthCampId = id;
        var res = await _svc.LaunchAsync(dto);
        return Success(res);
    }
}
