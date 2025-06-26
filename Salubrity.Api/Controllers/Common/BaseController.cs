using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Salubrity.Shared.Responses;
using System.IdentityModel.Tokens.Jwt;

namespace Salubrity.Api.Controllers.Common;

/// <summary>
/// Base controller providing helper methods for standardized responses.
/// </summary>
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.CreateSuccess(data, message));
    }

    protected IActionResult SuccessMessage(string message)
    {
        return Ok(ApiResponse<string>.CreateSuccessMessage(message));
    }

    protected IActionResult CreatedSuccess(string routeName, object routeValues, object result, string? message = null)
    {
        return CreatedAtRoute(routeName, routeValues, ApiResponse<object>.CreateSuccess(result, message ?? "Created successfully."));
    }

    protected IActionResult Failure(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<string>.CreateFailure(message));
    }

    protected IActionResult Failure<T>(string message, List<ErrorDetail>? errors, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<T>.CreateFailure(message, errors));
    }

    protected Guid GetCurrentUserId()
    {
       
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirst("sub"); // fallback


        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim missing or invalid.");
        }

        return userId;
    }
}
