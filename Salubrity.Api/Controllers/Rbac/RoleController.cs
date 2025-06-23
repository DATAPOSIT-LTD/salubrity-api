using Microsoft.AspNetCore.Mvc;
using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Shared.Responses;
using Salubrity.Api.Controllers.Common;

namespace Salubrity.Api.Controllers.Rbac;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("Roles Management")]
public class RoleController : BaseController
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleService.GetAllAsync();
        return Success(result.Data!, result.Message);
    }

    [HttpGet("{id:guid}", Name = nameof(GetById))] 
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return Success(result.Data!, result.Message);
    }


    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto input)
    {
        var result = await _roleService.CreateAsync(input);
        return CreatedSuccess(nameof(GetById), new { id = result.Data!.Id }, result.Data, result.Message);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto input)
    {
        var result = await _roleService.UpdateAsync(id, input);
        return SuccessMessage(result.Message);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _roleService.DeleteAsync(id);
        return SuccessMessage(result.Message);
    }
}
