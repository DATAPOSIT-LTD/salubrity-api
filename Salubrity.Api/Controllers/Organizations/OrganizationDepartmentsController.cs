using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Organizations;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Organizations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/organizations/{orgId:guid}/departments")]
[Tags("OrganizationDepartments")]
public class OrganizationDepartmentsController : BaseController
{
    private readonly IOrganizationDepartmentService _svc;
    public OrganizationDepartmentsController(IOrganizationDepartmentService svc) => _svc = svc;

    /// <summary>List departments belonging to the organization (admin + onboarding patient).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<OrganizationDepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid orgId, CancellationToken ct = default)
    {
        var list = await _svc.ListAsync(orgId, ct);
        return Success(list);
    }

    /// <summary>Add a single department under the organization.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDepartmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Add(Guid orgId, [FromBody] CreateOrgDepartmentDto dto, CancellationToken ct = default)
    {
        var created = await _svc.AddAsync(orgId, dto, ct);
        return Success(created);
    }

    /// <summary>Bulk-upload departments via XLSX (column 1 = Name, column 2 = Description, header optional).</summary>
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<BulkDepartmentUploadResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload(Guid orgId, IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Upload an XLSX file with one Name per row." });
        await using var stream = file.OpenReadStream();
        var result = await _svc.UploadAsync(orgId, stream, ct);
        return Success(result);
    }

    /// <summary>Downloads the XLSX template the admin should fill in before bulk upload.</summary>
    [HttpGet("template")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> GetTemplate(Guid orgId, CancellationToken ct = default)
    {
        var bytes = await _svc.GenerateTemplateAsync(ct);
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "departments-template.xlsx"
        );
    }

    /// <summary>Soft-delete a department.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid orgId, Guid id, CancellationToken ct = default)
    {
        await _svc.DeleteAsync(id, ct);
        return NoContent();
    }
}
