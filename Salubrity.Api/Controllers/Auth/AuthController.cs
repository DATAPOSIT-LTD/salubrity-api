using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Auth;
using Salubrity.Application.Interfaces.Services.Auth;
using Salubrity.Shared.Responses;
using System.Security.Claims;

namespace Salubrity.Api.Controllers.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json")]
[Tags("Authentication & Security")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return CreatedSuccess(nameof(Login), new { }, result, "Registration successful.");
    }

    [HttpPost("login", Name = nameof(Login))]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Success(result, "Login successful.");
    }

    //[Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<MeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var userIdValue = User.FindFirst("user_id")?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Failure("Invalid or missing user_id claim", StatusCodes.Status401Unauthorized);

        var result = await _authService.GetMeAsync(userId);
        return Success(result);
    }




    [Authorize]
    [HttpGet("me/debug")]
    public IActionResult MeDebug()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }



    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);
        return Success(result, "Token refreshed.");
    }

    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromQuery] Guid userId)
    {
        await _authService.LogoutAsync(userId);
        return SuccessMessage("Logged out.");
    }

  


    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        await _authService.RequestPasswordResetAsync(dto);
        return SuccessMessage("OTP sent to email.");
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return SuccessMessage("Password reset successfully.");
    }

    [HttpPut("change-password")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromQuery] Guid userId, [FromBody] ChangePasswordRequestDto dto)
    {
        await _authService.ChangePasswordAsync(userId, dto);
        return SuccessMessage("Password changed.");
    }

    [HttpPost("setup-mfa")]
    [ProducesResponseType(typeof(ApiResponse<SetupTotpResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetupMfa([FromQuery] string email)
    {
        var result = await _authService.SetupMfaAsync(email);
        return Success(result, "MFA setup successful.");
    }

    [HttpPost("verify-mfa-code")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyMfaCode([FromBody] VerifyTotpCodeRequestDto dto)
    {
        var valid = await _authService.VerifyTotpCodeAsync(dto);
        return valid ? SuccessMessage("MFA code verified.") : Unauthorized("Invalid MFA code.");
    }
}


