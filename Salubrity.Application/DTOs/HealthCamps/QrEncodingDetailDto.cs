namespace Salubrity.Application.DTOs.HealthCamps
{
    public class QrEncodingDetailDto
    {
        public string Token { get; set; } = default!;
        public Guid CampId { get; set; }
        public string Role { get; set; } = default!;
        public string? UserId { get; set; }    // present for patient/provider tokens
        public bool IsPoster { get; set; }     // true if it came from CreatePosterToken
        public DateTimeOffset ExpiresAt { get; set; }
        public string Jti { get; set; } = default!;
    }
    public class DecodePosterTokenRequest
    {
        public string Token { get; set; } = default!;
    }
}
