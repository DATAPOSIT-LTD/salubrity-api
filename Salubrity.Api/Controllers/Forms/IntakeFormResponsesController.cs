using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Forms;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/intake-form-responses")]
[Produces("application/json")]
[Tags("Intake Form Responses")]
public class IntakeFormResponsesController : BaseController
{
    private readonly IIntakeFormResponseService _service;
    private readonly IServiceService _serviceService;
    private readonly IBulkLabUploadService _bulkUploadService;

    public IntakeFormResponsesController(
        IIntakeFormResponseService service,
        IServiceService serviceService,
        IBulkLabUploadService bulkUploadService)
    {
        _service = service;
        _serviceService = serviceService;
        _bulkUploadService = bulkUploadService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit([FromBody] CreateIntakeFormResponseDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
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

    // ---------------------------------------------------------
    // Bulk Lab Results Endpoints
    // ---------------------------------------------------------
    /// <summary>
    /// Upload a CSV file with lab results. Server determines the form version from the file name.
    /// </summary>
    [HttpPost("bulk-upload/lab-results")]
    [ProducesResponseType(typeof(ApiResponse<BulkUploadResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadLabResultsCsv([FromForm] UploadLabResultsFormDto dto, CancellationToken ct = default)
    {
        if (dto.File == null || dto.File.Length == 0)
            return Failure("No file uploaded.", 422);

        var userId = GetCurrentUserId();

        var bulkDto = new CreateBulkLabUploadDto
        {
            CsvFile = dto.File,
            SubmittedByUserId = userId
        };

        var result = await _bulkUploadService.UploadCsvAsync(bulkDto, ct);
        return Success(result);
    }

    /// <summary>
    /// Download a CSV template for lab results. Headers match expected field mappings.
    /// </summary>
    [HttpGet("bulk-upload/lab-results/template")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadLabResultsTemplate([FromQuery] string sheetName, CancellationToken ct = default)
    {
        var templateStream = await _bulkUploadService.GenerateCsvTemplateAsync(sheetName, ct);
        var downloadName = $"LabResults_Template_{sheetName}.csv";
        return File(templateStream, "text/csv", downloadName);
    }

    /// <summary>
    /// Upload a CSV template with sample or test data.
    /// </summary>
    [HttpPost("bulk-upload/lab-results/template")]
    [ProducesResponseType(typeof(ApiResponse<BulkUploadResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadLabResultsWithTemplate([FromForm] UploadLabResultsFormDto dto, CancellationToken ct = default)
    {
        if (dto.File == null || dto.File.Length == 0)
            return Failure("No file uploaded.", 422);

        var userId = GetCurrentUserId();

        var bulkDto = new CreateBulkLabUploadDto
        {
            CsvFile = dto.File,
            SubmittedByUserId = userId
        };

        var result = await _bulkUploadService.UploadCsvAsync(bulkDto, ct);
        return Success(result);
    }
}
