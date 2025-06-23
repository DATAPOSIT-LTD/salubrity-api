using Microsoft.AspNetCore.Mvc;
using Salubrity.Shared.Responses;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user-roles")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("User Roles Management")]
public class UserRoleController : BaseController
{
    private readonly IUserRoleService _service;

    public UserRoleController(IUserRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserRoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllUserRolesAsync();
        return Success(result);
    }

    [HttpGet("{id:guid}", Name = "GetUserRoleById")]
    [ProducesResponseType(typeof(ApiResponse<UserRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetUserRoleByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserRoleDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUserRoleDto input)
    {
        var result = await _service.CreateUserRoleAsync(input);
        return CreatedSuccess("GetUserRoleById", new { id = result.Id }, result, "User role assigned.");
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRoleDto input)
    {
        await _service.UpdateUserRoleAsync(id, input);
        return SuccessMessage("User role updated.");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteUserRoleAsync(id);
        return SuccessMessage("User role removed.");
    }
}
