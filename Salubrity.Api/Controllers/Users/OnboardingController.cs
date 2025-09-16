using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Users;
using Salubrity.Application.Interfaces.Services.Users;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Identity;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users/onboarding")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("User Management")]
[Authorize]
public class UserOnboardingController : BaseController
{
    private readonly IOnboardingService _onboardingService;

    public UserOnboardingController(IOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>
    /// Get onboarding status for the current logged-in user.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<OnboardingStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOnboardingStatus(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var status = await _onboardingService.GetOnboardingStatusAsync(userId);

        if (status == null)
            return NotFound(ApiResponse<OnboardingStatusDto>.CreateFailure("Onboarding status not found"));

        var dto = new OnboardingStatusDto
        {
            UserId = status.UserId,
            IsProfileComplete = status.IsProfileComplete,
            IsRoleSpecificDataComplete = status.IsRoleSpecificDataComplete,
            IsOnboardingComplete = status.IsOnboardingComplete,
            CompletedAt = status.CompletedAt,
            Notes = status.Notes
        };

        return Success(dto);
    }

    /// <summary>
    /// Check and update onboarding status for the current logged-in user.
    /// </summary>
    [HttpPost("me/check")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckAndUpdateMyOnboarding(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await _onboardingService.CheckAndUpdateOnboardingStatusAsync(userId);
        return Success(result);
    }
}
