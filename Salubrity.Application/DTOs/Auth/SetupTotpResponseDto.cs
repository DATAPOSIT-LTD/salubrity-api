namespace Salubrity.Application.DTOs.Auth
{
    public class SetupTotpResponseDto
    {
        public string SecretKey { get; set; } = default!;
        public string QrCodeUri { get; set; } = default!;
    }
}
