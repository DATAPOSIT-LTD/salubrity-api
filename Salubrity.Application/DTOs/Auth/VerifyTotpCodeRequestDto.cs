namespace Salubrity.Application.DTOs.Auth
{
    public class VerifyTotpCodeRequestDto
    {
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
    }
}
