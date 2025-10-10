// File: Salubrity.Infrastructure/Security/Adapters/CampTokenFactoryAdapter.cs
using System.Security.Claims;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Infrastructure.Security; // your JwtService namespace
using Microsoft.Extensions.Options;
using Salubrity.Application.Interfaces.Security;

public sealed class CampTokenOptions
{
    public string AppBaseUrl { get; set; } = "https://app.salubritycentre.com/register"; //update with actual base url
    public string Audience { get; set; } = "camp-signin";
    public string Issuer { get; set; } = "salubrity-api";
}

public sealed class CampTokenFactoryAdapter : ICampTokenFactory
{
    private readonly IJwtService _jwt;           // your existing service
    private readonly CampTokenOptions _opt;

    public CampTokenFactoryAdapter(IJwtService jwt, IOptions<CampTokenOptions> opt)
    {
        _jwt = jwt;
        _opt = opt.Value;
    }

    public string BuildSignInUrl(string token) =>
        $"{_opt.AppBaseUrl.TrimEnd('/')}/?token={Uri.EscapeDataString(token)}";

    public string CreateUserToken(Guid campId, Guid userId, string role, string jti, DateTimeOffset expiresUtc)
        => CreateTokenBase(campId, role, jti, expiresUtc,
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

    public string CreatePosterToken(Guid campId, string role, Guid roleId, string jti, DateTimeOffset expiresUtc, Guid? organizationId = null)
        => CreateTokenBase(campId, role, jti, expiresUtc,
            new Claim("poster", "1"),
            new Claim("roleId", roleId.ToString()),
            organizationId.HasValue ? new Claim("organizationId", organizationId.Value.ToString()) : null);

    private string CreateTokenBase(Guid campId, string role, string jti, DateTimeOffset exp, params Claim[] extra)
    {
        var claims = new List<Claim>
    {
        new("campId", campId.ToString()),
        new(ClaimTypes.Role, role),
        new("jti", jti)
    };
        claims.AddRange(extra);

        return _jwt.GenerateAccessToken(
            claims,
            exp,
            issuer: _opt.Issuer,
            audience: _opt.Audience
        );
    }

}
