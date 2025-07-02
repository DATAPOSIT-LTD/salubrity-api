using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Shared.Security.Config;

namespace Salubrity.Infrastructure.Security
{
    public class JwtService : IJwtService
    {
        private readonly IKeyProvider _keyProvider;
        private readonly JwtSettings _settings;

        public JwtService(IKeyProvider keyProvider, IOptions<JwtSettings> options)
        {
            _keyProvider = keyProvider;
            _settings = options.Value;
        }

        public string GenerateAccessToken(Guid userId, string email, string[] roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(JwtRegisteredClaimNames.Email, email),
                new("user_id", userId.ToString())
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var credentials = new SigningCredentials(
                _keyProvider.GetPrivateKey(),
                SecurityAlgorithms.RsaSha256
            );


            var token = new JwtSecurityToken(
                issuer: "Salubrity",//_settings.Issuer,
                audience: "SalubrityClient",//_settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
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
                ValidateLifetime = false, 
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
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
