using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Rbac;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/role-permission-groups")]
[Produces("application/json")]
[Tags("Role–Permission Groups")]
public class RolePermissionGroupController : BaseController
{
    private readonly IRolePermissionGroupService _service;

    public RolePermissionGroupController(IRolePermissionGroupService service)
    {
        _service = service;
    }

    /// <summary> Get all permission groups assigned to a role </summary>
    [HttpGet("{roleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionGroupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionGroupsByRole(Guid roleId)
    {
        var result = await _service.GetPermissionGroupsByRoleAsync(roleId);
        return Success(result);
    }

    /// <summary> Assign permission groups to a role (bulk append) </summary>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignPermissionGroups([FromBody] AssignPermissionGroupsToRoleDto input)
    {
        await _service.AssignPermissionGroupsToRoleAsync(input);
        return Success("Permission groups assigned to role successfully.");
    }

    /// <summary> Unassign all permission groups from a role </summary>
    [HttpDelete("unassign-all/{roleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveAllPermissionGroups(Guid roleId)
    {
        await _service.RemoveAllPermissionGroupsFromRoleAsync(roleId);
        return Success("All permission groups removed from role.");
    }

    /// <summary> Replace all existing permission groups with a new set (bulk override) </summary>
    [HttpPut("replace")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReplacePermissionGroups([FromBody] AssignPermissionGroupsToRoleDto input)
    {
        await _service.ReplacePermissionGroupsForRoleAsync(input);
        return Success("Permission groups replaced successfully.");
    }
}
