// File: Salubrity.Application/Interfaces/Security/IJwtService.cs
using System.Security.Claims;

namespace Salubrity.Application.Interfaces.Security
{
    public interface IJwtService
    {
        // ============================================================
        // SIGNING
        // ============================================================

        string GenerateAccessToken(Guid userId, string email, string[] roles);

        string GenerateAccessToken(
            IEnumerable<Claim> claims,
            DateTimeOffset expiresUtc,
            string[] roles
        );

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
        // VALIDATION
        // ============================================================

        ClaimsPrincipal ValidateToken(string token);

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        // ============================================================
        // DECODE ONLY (NO CRYPTO)
        // ============================================================

        ClaimsPrincipal DecodeTokenWithoutValidation(string token);
    }
}
