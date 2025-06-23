using Microsoft.AspNetCore.Mvc;
using Salubrity.Shared.Responses;

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
}
