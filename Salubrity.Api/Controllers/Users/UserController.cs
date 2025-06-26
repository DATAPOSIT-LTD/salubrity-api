using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Users;
using Salubrity.Application.Interfaces.Services.Users;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Identity;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("User Management")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users in the system.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,UserManager")]
    [ProducesResponseType(typeof(ApiResponse<List<UserResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return Success(result);
    }

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetUserById")]
    [Authorize(Roles = "Admin,UserManager")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Success(result);
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest input)
    {
        var result = await _userService.CreateAsync(input);
        return CreatedSuccess(nameof(GetById), new { id = result.Data }, result);
    }

    /// <summary>
    /// Update user profile.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest input)
    {
        var result = await _userService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// Delete a user by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteAsync(id);
        return Success(result);
    }
}
