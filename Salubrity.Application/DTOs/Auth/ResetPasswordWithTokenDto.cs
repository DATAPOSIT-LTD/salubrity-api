namespace Salubrity.Application.DTOs.Auth;

public class ResetPasswordWithTokenDto
{
    public string Token { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
