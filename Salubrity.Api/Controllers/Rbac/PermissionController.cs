using Microsoft.AspNetCore.Mvc;
using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Application.Services.Rbac;
using Salubrity.Shared.Responses;
using Salubrity.Api.Controllers.Common;

namespace Salubrity.Api.Controllers.Rbac;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/permissions")]
[Produces("application/json")]
[Tags("Permissions Management")]
public class PermissionController : BaseController
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _permissionService.GetAllPermissionsAsync();
        return Success(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _permissionService.GetPermissionByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PermissionCreateDto input)
    {
        var created = await _permissionService.CreatePermissionAsync(input);
        return CreatedSuccess(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PermissionUpdateDto input)
    {
        await _permissionService.UpdatePermissionAsync(id, input);
        return Success("Permission updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _permissionService.DeletePermissionAsync(id);
        return Success("Permission deleted successfully.");
    }

    [HttpGet("by-role/{roleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRole(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _permissionService.GetPermissionsByRoleAsync(roleId, cancellationToken);
        return Success(result);
    }
}
