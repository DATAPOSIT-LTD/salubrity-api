using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Salubrity.Shared.Extensions
{
    public static class JwtAuthSetup
    {
        public static IServiceCollection AddJwtAuth(
            this IServiceCollection services,
            IConfiguration config)
        {
            var jwtSection = config.GetSection("Jwt");

            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var secret = jwtSection["Secret"];

            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("JWT Secret is missing.");

            if (string.IsNullOrWhiteSpace(issuer))
                throw new InvalidOperationException("JWT Issuer is missing.");

            if (string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("JWT Audience is missing.");

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret)
            );

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Core validation
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        RequireExpirationTime = true,

                        // Values
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = signingKey,

                        // No grace period
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}
