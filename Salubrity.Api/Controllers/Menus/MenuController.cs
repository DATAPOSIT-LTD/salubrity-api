using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Menus;
using Salubrity.Application.Interfaces.Services.Menus;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Menus;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/menus")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("Menu Management")]
public class MenuController : BaseController
{
    private readonly IMenuService _menuService;
    private readonly IMenuRoleService _menuRoleService;

    public MenuController(IMenuService menuService, IMenuRoleService menuRoleService)
    {
        _menuService = menuService;
        _menuRoleService = menuRoleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<MenuResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _menuService.GetAllAsync();
        return Success(result);
    }


    [HttpGet("{id:guid}", Name = "GetMenuById")]
    [ProducesResponseType(typeof(ApiResponse<MenuResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _menuService.GetByIdAsync(id);
        return Success(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MenuResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] MenuCreateDto input)
    {
        var result = await _menuService.CreateAsync(input);
        return CreatedSuccess(nameof(GetById), new { id = result.Id }, result);
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MenuResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] MenuUpdateDto input)
    {
        var result = await _menuService.UpdateAsync(id, input);
        return Success(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _menuService.DeleteAsync(id); // Fix: Removed assignment to a variable since DeleteAsync returns void.
        return SuccessMessage("Menu deleted successfully.");
    }

    [HttpPost("assign-role")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRolesToMenu([FromBody] MenuRoleCreateDto input)
    {
        await _menuRoleService.AssignRolesAsync(input);
        return SuccessMessage("Roles assigned to menu.");
    }

    [HttpGet("{menuId:guid}/roles")]
    [ProducesResponseType(typeof(ApiResponse<List<MenuRoleResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesForMenu(Guid menuId)
    {
        var result = await _menuRoleService.GetRolesForMenuAsync(menuId);
        return Success(result);
    }

    [HttpDelete("{menuId:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveRole(Guid menuId, Guid roleId)
    {
        await _menuRoleService.RemoveRoleAsync(menuId, roleId);
        return SuccessMessage("Role removed from menu.");
    }
}
