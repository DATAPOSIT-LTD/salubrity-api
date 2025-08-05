using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Employees;
using Salubrity.Application.Interfaces.Services.Employee;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Employee;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/employees")]
[Produces("application/json")]
[Tags("Employee Management")]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly IEmployeeTemplateService _employeeTemplateService;

    public EmployeeController(
        IEmployeeService employeeService,
        IEmployeeTemplateService employeeTemplateService)
    {
        _employeeService = employeeService;
        _employeeTemplateService = employeeTemplateService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _employeeService.GetAllAsync();
        return Success(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _employeeService.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] EmployeeRequestDto dto)
    {
        var result = await _employeeService.CreateAsync(dto);
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("upload-template")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadExcelTemplate([FromForm] EmployeeTemplateUploadRequestDto request)
    {
        var result = await _employeeTemplateService.ProcessAndUpsertTemplateAsync(request.File);
        return Success(result);
    }

    [HttpGet("template")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadTemplate()
    {
        var (content, fileName) = await _employeeTemplateService.GetTemplateAsync();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("bulk-upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BulkUploadResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpload([FromForm] BulkEmployeeUploadRequest request)
    {
        var result = await _employeeService.BulkCreateFromCsvAsync(request.File);
        return Ok(result);
    }

    [HttpGet("by-organization/{organizationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeLeanResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrganization(Guid organizationId)
    {
        var result = await _employeeService.GetByOrganizationAsync(organizationId);
        return Success(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmployeeRequestDto dto)
    {
        var result = await _employeeService.UpdateAsync(id, dto);
        return Success(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _employeeService.DeleteAsync(id);
        return Success("Employee deleted.");
    }
}
