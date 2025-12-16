using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Shared.Security.Config;

namespace Salubrity.Infrastructure.Security
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;
        private readonly SymmetricSecurityKey _signingKey;

        public JwtService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;

            if (string.IsNullOrWhiteSpace(_settings.Secret))
                throw new InvalidOperationException("Jwt:Secret is missing.");

            _signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.Secret)
            );
        }

        // ============================================================
        // TOKEN GENERATION
        // ============================================================

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

            return BuildToken(claims, _settings.Issuer, _settings.Audience);
        }

        public string GenerateAccessToken(
            IEnumerable<Claim> claims,
            DateTimeOffset expiresUtc,
            string[] roles)
        {
            var allClaims = new List<Claim>(claims);

            foreach (var role in roles)
                allClaims.Add(new Claim(ClaimTypes.Role, role));

            return BuildToken(
                allClaims,
                _settings.Issuer,
                _settings.Audience,
                expiresUtc.UtcDateTime
            );
        }

        public string GenerateAccessToken(
            IEnumerable<Claim> claims,
            DateTimeOffset expiresUtc,
            string issuer,
            string? audience)
        {
            return BuildToken(
                claims,
                issuer,
                audience ?? _settings.Audience,
                expiresUtc.UtcDateTime
            );
        }

        private string BuildToken(
            IEnumerable<Claim> claims,
            string issuer,
            string audience,
            DateTime? expires = null)
        {
            var credentials = new SigningCredentials(
                _signingKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires ?? DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        public ClaimsPrincipal ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                IssuerSigningKey = _signingKey
            };

            return handler.ValidateToken(token, parameters, out _);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                IssuerSigningKey = _signingKey
            };

            try
            {
                return handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // UTILITIES
        // ============================================================

        public ClaimsPrincipal DecodeTokenWithoutValidation(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return new ClaimsPrincipal(
                new ClaimsIdentity(jwt.Claims, "JWT")
            );
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
