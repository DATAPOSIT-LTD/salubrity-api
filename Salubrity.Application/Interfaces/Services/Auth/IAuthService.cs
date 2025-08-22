using Salubrity.Application.DTOs.Auth;

namespace Salubrity.Application.Interfaces.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto input);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto input);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input);
        Task LogoutAsync(Guid userId);
        Task RequestPasswordResetAsync(ForgotPasswordRequestDto input);
        Task ResetPasswordAsync(ResetPasswordRequestDto input);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto input);
        Task<SetupTotpResponseDto> SetupMfaAsync(string email);
        Task<bool> VerifyTotpCodeAsync(VerifyTotpCodeRequestDto input);
        Task<MeResponseDto> GetMeAsync(Guid userId);

    }
}
