// File: Salubrity.Application/Interfaces/Security/IJwtService.cs
using System.Security.Claims;

namespace Salubrity.Application.Interfaces.Security
{
    public interface IJwtService
    {
        // ============================================================
        // SIGNING
        // ============================================================

        // Existing: token based on user ID + email + roles
        string GenerateAccessToken(Guid userId, string email, string[] roles);

        // New: flexible token based on claims + expiry + roles
        string GenerateAccessToken(
            IEnumerable<Claim> claims,
            DateTimeOffset expiresUtc,
            string[] roles
        );

        // Optional: most flexible (custom issuer/audience)
        string GenerateAccessToken(
            IEnumerable<Claim> claims,
            DateTimeOffset expiresUtc,
            string issuer,
            string audience
        );

        // ============================================================
        // REFRESH
        // ============================================================
        string GenerateRefreshToken();

        // ============================================================
        // VALIDATION (FULL CRYPTO)
        // ============================================================
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        ClaimsPrincipal ValidateToken(
            string token,
            string expectedAudience,
            string expectedIssuer
        );

        // ============================================================
        // DECODE ONLY (NO SIGNATURE / NO CRYPTO)
        // ============================================================
        ClaimsPrincipal DecodeTokenWithoutValidation(string token);
    }
}
