using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.HealthCamps;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health-camps/{healthCampId:guid}")]
[Produces("application/json")]
[Tags("Health Camps Management View")]
public class HealthCampManagementController : BaseController
{
    private readonly IHealthCampManagementService _service;

    public HealthCampManagementController(IHealthCampManagementService service)
    {
        _service = service;
    }

    [HttpGet("service-stations")]
    [ProducesResponseType(typeof(ApiResponse<List<ServiceStationSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceStations(Guid healthCampId)
    {
        var result = await _service.GetServiceStationsAsync(healthCampId);
        return Success(result);
    }

    [HttpGet("patients")]
    [ProducesResponseType(typeof(ApiResponse<List<CampPatientSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatients(Guid healthCampId)
    {
        var result = await _service.GetCampPatientsAsync(healthCampId);
        return Success(result);
    }

    [HttpGet("activity-summary")]
    [ProducesResponseType(typeof(ApiResponse<List<CampDailyActivityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivitySummary(Guid healthCampId)
    {
        var result = await _service.GetActivitySummaryAsync(healthCampId);
        return Success(result);
    }

    [HttpGet("billing")]
    [ProducesResponseType(typeof(ApiResponse<List<CampBillingSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingSummary(Guid healthCampId)
    {
        var result = await _service.GetBillingSummaryAsync(healthCampId);
        return Success(result);
    }
}
