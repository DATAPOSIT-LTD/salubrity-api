// Salubrity.Application/Security/CurrentSubcontractorService.cs
using Salubrity.Shared.Exceptions;

public class CurrentSubcontractorService : ICurrentSubcontractorService
{
    private readonly IUsersReadRepository _users;
    private readonly IUserRoleReadRepository _roles;
    private readonly ISubcontractorReadRepository _subs;

    public CurrentSubcontractorService(
        IUsersReadRepository users,
        IUserRoleReadRepository roles,
        ISubcontractorReadRepository subs)
    {
        _users = users;
        _roles = roles;
        _subs = subs;
    }

    public async Task<Guid> GetRequiredSubcontractorIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (!await _users.IsActiveAsync(userId, ct))
            throw new UnauthorizedException("User not found or inactive.");

        var hasRole = await _roles.HasRoleAsync(userId, "Subcontractor", ct);
        if (!hasRole)
            throw new UnauthorizedException("Requires Subcontractor role.");

        var subId = await _subs.GetActiveIdByUserIdAsync(userId, ct);
        if (subId is null)
            throw new UnauthorizedException("No active subcontractor profile for this user.");

        return subId.Value;
    }
}
