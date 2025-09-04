using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Application.Interfaces.Services.Users;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Forms;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/intake-form-responses")]
[Produces("application/json")]
[Tags("Intake Form Responses")]
public class IntakeFormResponsesController : BaseController
{
    private readonly IIntakeFormResponseService _service;
    private readonly IServiceService _serviceService;


    public IntakeFormResponsesController(IIntakeFormResponseService service, IServiceService serviceService)
    {
        _service = service;
        _serviceService = serviceService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit([FromBody] CreateIntakeFormResponseDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId(); // ← From your BaseController or JWT
        var responseId = await _service.SubmitResponseAsync(dto, userId, ct);
        return Success(responseId, "Response submitted successfully.");
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IntakeFormResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _serviceService.GetByIdAsync(id);
        return result is null
            ? Failure("Response not found.")
            : Success(result);
    }


    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<IntakeFormResponseDetailDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatientAndCamp([FromQuery] Guid patientId, [FromQuery] Guid healthCampId, CancellationToken ct)
    {
        var responses = await _service.GetResponsesByPatientAndCampIdAsync(patientId, healthCampId, ct);
        return Success(responses);
    }

}
