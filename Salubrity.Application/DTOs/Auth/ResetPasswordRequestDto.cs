namespace Salubrity.Application.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        public string Email { get; set; } = default!;
        public string Otp { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
