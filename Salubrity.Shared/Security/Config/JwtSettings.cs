namespace Salubrity.Shared.Security.Config
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int AccessTokenExpiryMinutes { get; set; } = 8400;
    }
}
