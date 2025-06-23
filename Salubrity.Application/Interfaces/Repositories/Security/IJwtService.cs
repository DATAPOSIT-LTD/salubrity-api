using System;
using System.Security.Claims;

namespace Salubrity.Application.Interfaces.Security
{
    public interface IJwtService
    {
        string GenerateAccessToken(Guid userId, string email, string[] roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
