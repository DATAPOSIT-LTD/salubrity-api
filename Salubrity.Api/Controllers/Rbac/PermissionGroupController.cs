using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Rbac;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/permission-groups")]
[Produces("application/json")]
[Tags("Permission Groups")]
public class PermissionGroupController : BaseController
{
    private readonly IPermissionGroupService _service;

    public PermissionGroupController(IPermissionGroupService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionGroupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Success(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PermissionGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PermissionGroupDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePermissionGroupDto input)
    {
        var result = await _service.CreateAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PermissionGroupDto>.CreateSuccess(result));
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePermissionGroupDto input)
    {
        await _service.UpdateAsync(id, input);
        return Success("Permission group updated.");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Success("Permission group deleted.");
    }
}
