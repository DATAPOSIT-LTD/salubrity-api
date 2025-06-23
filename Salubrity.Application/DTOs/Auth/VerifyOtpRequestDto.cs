namespace Salubrity.Application.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
    }
}
