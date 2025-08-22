// File: Salubrity.Application/Interfaces/Security/IJwtService.cs
using System.Security.Claims;

namespace Salubrity.Application.Interfaces.Security
{
    public interface IJwtService
    {
        // Existing: token based on user ID + email + roles
        string GenerateAccessToken(Guid userId, string email, string[] roles);

        //  New: flexible token based on claims + expiry + roles
        string GenerateAccessToken(IEnumerable<Claim> claims, DateTimeOffset expiresUtc, string[] roles);

        //  Optional: most flexible if you want to control issuer/audience
        string GenerateAccessToken(IEnumerable<Claim> claims, DateTimeOffset expiresUtc, string issuer, string audience);

        string GenerateRefreshToken();

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
