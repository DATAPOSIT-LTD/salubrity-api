namespace Salubrity.Shared.Security.Config
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = "Salubrity";
        public string Audience { get; set; } = "SalubrityClient";

        // REQUIRED for symmetric JWT (HMAC)
        public string Secret { get; set; } = "PnKsjeXK7BkNABRCa2pb8f+3MkyDnQZxVKCBvAp8AS6bm7LyPSps66+S7JJrYMWXNlz5baLybKjVub5bpx8Ykg==";

        public int AccessTokenExpiryMinutes { get; set; } = 8400;
    }
}
