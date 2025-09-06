using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Application.Interfaces.Services;
using Salubrity.Shared.Responses;
using Microsoft.AspNetCore.Http;

namespace Salubrity.Api.Controllers.Subcontractor;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subcontractors")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("Subcontractors Management")]
public class SubcontractorController : BaseController
{
    private readonly ISubcontractorService _service;

    public SubcontractorController(ISubcontractorService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubcontractorDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSubcontractorDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedSuccess("GetSubcontractorById", new { id = result.Id }, result, "Subcontractor created.");
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SubcontractorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Success(result);
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SubcontractorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubcontractorDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Success(result, "Subcontractor updated.");
    }

    [HttpGet("{id:guid}", Name = "GetSubcontractorById")]
    [ProducesResponseType(typeof(ApiResponse<SubcontractorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost("{id:guid}/assign-role")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignSubcontractorRoleDto dto)
    {
        await _service.AssignRoleAsync(id, dto);
        return SuccessMessage("Role(s) assigned to subcontractor.");
    }

    [HttpPost("{id:guid}/assign-specialty")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignSpecialty(Guid id, [FromBody] CreateSubcontractorSpecialtyDto dto)
    {
        await _service.AssignSpecialtyAsync(id, dto);
        return SuccessMessage("Specialty assigned to subcontractor.");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        Guid userId = GetCurrentUserId();
        await _service.DeleteAsync(id, userId, ct);
        return SuccessMessage("Subcontractor deleted (soft).");
    }

}
