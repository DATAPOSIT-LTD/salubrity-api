using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Application.Services.Forms;
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
    private readonly IHealthCampService _campService;

    public IntakeFormResponsesController(
        IIntakeFormResponseService service,
        IServiceService serviceService,
        IBulkLabUploadService bulkUploadService,
        IHealthCampService campService)
    {
        _service = service;
        _serviceService = serviceService;
        _bulkUploadService = bulkUploadService;
        _campService = campService;
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
    // Bulk Lab Results (Excel) Endpoints
    // ---------------------------------------------------------


    [HttpGet("bulk-upload/lab-results/template")]
    public async Task<IActionResult> DownloadLabResultsTemplate(Guid campId, CancellationToken ct = default)
    {
        Guid userId = GetCurrentUserId();

        // Get camp details
        var camp = await _campService.GetByIdAsync(campId);
        if (camp == null)
            return Failure("Health camp not found.");

        var templateStream = await _bulkUploadService.GenerateLabTemplateForCampAsync(userId, campId, ct);

        // Sanitize camp name for file system
        var safeCampName = string.Join("_", camp.Name.Split(Path.GetInvalidFileNameChars()));

        return File(templateStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"LabResults_Template_{safeCampName}.xlsx");
    }




    /// <summary>
    /// Upload a completed Excel with lab results
    /// </summary>
    [HttpPost("bulk-upload/lab-results")]
    public async Task<IActionResult> UploadLabResultsExcel([FromForm] UploadLabResultsFormDto dto, CancellationToken ct = default)
    {
        if (dto.File == null || dto.File.Length == 0)
            return Failure("No file uploaded.", 422);

        var userId = GetCurrentUserId();

        var bulkDto = new CreateBulkLabUploadDto
        {
            ExcelFile = dto.File,
            SubmittedByUserId = userId
        };

        var result = await _bulkUploadService.UploadExcelAsync(bulkDto, ct);
        return Success(result);
    }

    // Download Findings Implmentation

    [HttpGet("export-camp-data/{campId}")]
    //[Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportCampData(Guid campId, CancellationToken ct)
    {
        var (excelData, campName, organizationName) = await _service.ExportCampDataToExcelAsync(campId, ct);

        // Clean both names for filename (remove invalid characters)
        var safeOrgName = string.Join("_", organizationName.Split(Path.GetInvalidFileNameChars()));
        var safeCampName = string.Join("_", campName.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{safeOrgName}_Camp_Data_Export_{safeCampName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}