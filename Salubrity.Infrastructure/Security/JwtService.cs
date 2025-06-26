using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Salubrity.Application.Interfaces.Security;

namespace Salubrity.Infrastructure.Security
{
    public class JwtService : IJwtService
    {

        private readonly IKeyProvider _keyProvider;

        public JwtService(IKeyProvider keyProvider)
        {
            _keyProvider = keyProvider;
        }

        public string GenerateAccessToken(Guid userId, string email, string[] roles)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),               // standard
            new(ClaimTypes.NameIdentifier, userId.ToString()),                // compatibility
            new(JwtRegisteredClaimNames.Email, email),
            new("user_id", userId.ToString())                                 // optional custom
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var credentials = new SigningCredentials(
                _keyProvider.GetPrivateKey(),
                SecurityAlgorithms.RsaSha256
            );

            var token = new JwtSecurityToken(
                issuer: "Salubrity",
                audience: "SalubrityClient",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Bypass expiry check
                ValidateIssuerSigningKey = true,
                ValidIssuer = "Salubrity",
                ValidAudience = "SalubrityClient",
                IssuerSigningKey = _keyProvider.GetPublicKey()
            };

            try
            {
                return handler.ValidateToken(token, validationParams, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
