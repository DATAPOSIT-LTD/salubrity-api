// Salubrity.Application/Security/CurrentSubcontractorService.cs
using Microsoft.Extensions.Logging;
using Salubrity.Shared.Exceptions;

public class CurrentSubcontractorService : ICurrentSubcontractorService
{
    private readonly IUsersReadRepository _users;
    private readonly IUserRoleReadRepository _roles;
    private readonly ISubcontractorReadRepository _subs;
    private readonly ILogger<CurrentSubcontractorService> _logger;

    public CurrentSubcontractorService(
        IUsersReadRepository users,
        IUserRoleReadRepository roles,
        ISubcontractorReadRepository subs,
        ILogger<CurrentSubcontractorService> logger)
    {
        _users = users;
        _roles = roles;
        _subs = subs;
        _logger = logger;
    }

    /// <summary>
    /// STRICT: must be a subcontractor. Admin/Doctor/Concierge are NOT allowed.
    /// </summary>
    public async Task<Guid> GetRequiredSubcontractorIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        if (!await _users.IsActiveAsync(userId, ct))
        {
            _logger.LogWarning("User {UserId} is not active", userId);
            throw new UnauthorizedException("User not found or inactive.");
        }

        if (!await _roles.HasRoleAsync(userId, "Subcontractor", ct))
        {
            _logger.LogWarning(
                "User {UserId} attempted subcontractor-only access without role",
                userId);
            throw new UnauthorizedException("Requires Subcontractor role.");
        }

        var subId = await _subs.GetActiveIdByUserIdAsync(userId, ct);
        if (subId is null)
        {
            _logger.LogWarning(
                "User {UserId} has Subcontractor role but no active profile",
                userId);
            throw new UnauthorizedException("No active subcontractor profile.");
        }

        return subId.Value;
    }

    /// <summary>
    /// Flexible: returns subcontractorId or null (no filter).
    /// </summary>
    public async Task<Guid?> GetSubcontractorIdOrThrowAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var subId = await TryGetSubcontractorIdAsync(userId, ct);
        if (subId == null && !await IsPrivilegedRoleAsync(userId, ct))
        {
            _logger.LogWarning(
                "Access denied for user {UserId}: no valid role",
                userId);
            throw new UnauthorizedException("Access denied.");
        }

        return subId;
    }

    /// <summary>
    /// Core resolver: NEVER returns Guid.Empty.
    /// </summary>
    public async Task<Guid?> TryGetSubcontractorIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        if (!await _users.IsActiveAsync(userId, ct))
        {
            _logger.LogWarning("User {UserId} is not active", userId);
            return null;
        }

        if (await _roles.HasRoleAsync(userId, "Admin", ct) ||
            await _roles.HasRoleAsync(userId, "Doctor", ct) ||
            await _roles.HasRoleAsync(userId, "Concierge", ct))
        {
            _logger.LogInformation(
                "User {UserId} is privileged — no subcontractor filter",
                userId);
            return null;
        }

        if (await _roles.HasRoleAsync(userId, "Subcontractor", ct))
        {
            var subId = await _subs.GetActiveIdByUserIdAsync(userId, ct);

            if (subId == null)
            {
                _logger.LogWarning(
                    "User {UserId} has Subcontractor role but no active profile",
                    userId);
            }

            return subId;
        }

        _logger.LogWarning("User {UserId} has no recognized role", userId);
        return null;
    }


    private async Task<bool> IsPrivilegedRoleAsync(Guid userId, CancellationToken ct)
    {
        return await _roles.HasRoleAsync(userId, "Admin", ct)
            || await _roles.HasRoleAsync(userId, "Doctor", ct)
            || await _roles.HasRoleAsync(userId, "Concierge", ct);
    }
}
