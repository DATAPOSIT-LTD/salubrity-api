using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.HealthAssessments;
using Salubrity.Application.Interfaces.Services.HealthAssessments;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.HealthAssessments;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/health-assessments/forms")]
[Produces("application/json")]
[Tags("Health Assessment Forms")]
public class HealthAssessmentFormsController : BaseController
{
    private readonly IHealthAssessmentFormService _service;

    public HealthAssessmentFormsController(IHealthAssessmentFormService service)
    {
        _service = service;
    }

    [HttpPost("submit-section")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitFormSection(
        [FromBody] SubmitHealthAssessmentFormDto dto,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await _service.SubmitFormSectionAsync(dto, userId, ct);
        return Success(result);
    }

    [HttpGet("submitted-responses")]
    [ProducesResponseType(typeof(ApiResponse<List<HealthAssessmentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientResponses([FromQuery] Guid patientId, [FromQuery] Guid campId, CancellationToken ct = default)
    {
        var result = await _service.GetPatientAssessmentResponsesAsync(patientId, campId, ct);
        return Success(result);
    }

}
