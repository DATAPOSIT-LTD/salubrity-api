using Salubrity.Application.DTOs.Users;
using Salubrity.Shared.Responses;

namespace Salubrity.Application.Interfaces.Services.Users;

/// <summary>
/// Service interface for high-level user management logic.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get a user by their ID.
    /// </summary>
    /// <param name="id">User ID (GUID)</param>
    Task<ApiResponse<UserResponse>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all users in the system.
    /// </summary>
    Task<ApiResponse<List<UserResponse>>> GetAllAsync();

    /// <summary>
    /// Create a new user (register).
    /// </summary>
    /// <param name="request">User creation DTO</param>
    Task<ApiResponse<Guid>> CreateAsync(UserCreateRequest request);

    /// <summary>
    /// Update an existing user.
    /// </summary>
    /// <param name="id">User ID (GUID)</param>
    /// <param name="request">User update DTO</param>
    Task<ApiResponse<string>> UpdateAsync(Guid id, UserUpdateRequest request);

    /// <summary>
    /// Delete a user from the system.
    /// </summary>
    /// <param name="id">User ID (GUID)</param>
    Task<ApiResponse<string>> DeleteAsync(Guid id);

    /// <summary>
    /// Check if a user is assigned to a specific role.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleName">Name of the role to check</param>
    Task<bool> IsInRoleAsync(Guid userId, string roleName);

}
