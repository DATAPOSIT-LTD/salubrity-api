// File: Salubrity.Application/Interfaces/Services/HealthCamps/ICampTokenFactory.cs
public interface ICampTokenFactory
{
    string BuildSignInUrl(string token);
    string CreateUserToken(Guid campId, Guid userId, string role, string jti, DateTimeOffset expiresUtc);
    string CreatePosterToken(Guid campId, string role, Guid roleId, string jti, DateTimeOffset expiresUtc);
}
