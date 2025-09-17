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

    // public async Task<Guid> GetRequiredSubcontractorIdAsync(Guid userId, CancellationToken ct = default)
    // {
    //     if (!await _users.IsActiveAsync(userId, ct))
    //         throw new UnauthorizedException("User not found or inactive.");

    //     // Admin override
    //     if (await _roles.HasRoleAsync(userId, "Admin", ct))
    //         return Guid.Empty;

    //     if (!await _roles.HasRoleAsync(userId, "Subcontractor", ct))
    //         throw new UnauthorizedException("Requires Subcontractor role.");

    //     var subId = await _subs.GetActiveIdByUserIdAsync(userId, ct);
    //     if (subId is null)
    //         throw new UnauthorizedException("No active subcontractor profile for this user.");

    //     return subId.Value;
    // }

    // public async Task<Guid> GetSubcontractorIdOrThrowAsync(Guid userId, CancellationToken ct = default)
    // {
    //     var subId = await TryGetSubcontractorIdAsync(userId, ct);
    //     if (subId == null)
    //         throw new UnauthorizedException("Access denied: not a subcontractor or admin.");
    //     return subId.Value;
    // }

    // public async Task<Guid?> TryGetSubcontractorIdAsync(Guid userId, CancellationToken ct = default)
    // {
    //     //ACTIVATE THIS IF I FORGET IT COMMENTED LIKE THIS
    //     // if (!await _users.IsActiveAsync(userId, ct))
    //     //     return null;

    //     if (await _roles.HasRoleAsync(userId, "Admin", ct))
    //         return Guid.Empty;

    //     if (!await _roles.HasRoleAsync(userId, "Subcontractor", ct))
    //         return null;

    //     if (await _roles.HasRoleAsync(userId, "Concierge", ct))
    //         return Guid.Empty;

    //     if (await _roles.HasRoleAsync(userId, "Doctor", ct))
    //         return Guid.Empty;

    //     return await _subs.GetActiveIdByUserIdAsync(userId, ct);
    // }

    public async Task<Guid> GetRequiredSubcontractorIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (!await _users.IsActiveAsync(userId, ct))
        {
            _logger.LogWarning("User {UserId} is not active", userId);
            throw new UnauthorizedException("User not found or inactive.");
        }

        if (await _roles.HasRoleAsync(userId, "Admin", ct))
        {
            _logger.LogInformation("User {UserId} is Admin — override with Guid.Empty", userId);
            return Guid.Empty;
        }

        if (!await _roles.HasRoleAsync(userId, "Subcontractor", ct))
        {
            _logger.LogWarning("User {UserId} is missing Subcontractor role", userId);
            throw new UnauthorizedException("Requires Subcontractor role.");
        }

        var subId = await _subs.GetActiveIdByUserIdAsync(userId, ct);
        if (subId is null)
        {
            _logger.LogWarning("User {UserId} has no active Subcontractor profile", userId);
            throw new UnauthorizedException("No active subcontractor profile for this user.");
        }

        return subId.Value;
    }

    public async Task<Guid> GetSubcontractorIdOrThrowAsync(Guid userId, CancellationToken ct = default)
    {
        var subId = await TryGetSubcontractorIdAsync(userId, ct);
        if (subId == null)
        {
            _logger.LogWarning("Access denied for user {UserId}: no matching role", userId);
            throw new UnauthorizedException("Access denied: not a subcontractor or admin.");
        }

        return subId.Value;
    }

    public async Task<Guid?> TryGetSubcontractorIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (!await _users.IsActiveAsync(userId, ct))
        {
            _logger.LogWarning("User {UserId} is not active", userId);
            return null;
        }

        if (await _roles.HasRoleAsync(userId, "Admin", ct))
        {
            _logger.LogInformation("User {UserId} is Admin — returning Guid.Empty", userId);
            return Guid.Empty;
        }

        if (await _roles.HasRoleAsync(userId, "Subcontractor", ct))
        {
            _logger.LogInformation("User {UserId} is Subcontractor", userId);
            return await _subs.GetActiveIdByUserIdAsync(userId, ct);
        }

        if (await _roles.HasRoleAsync(userId, "Doctor", ct))
        {
            _logger.LogInformation("User {UserId} is Doctor — returning Guid.Empty", userId);
            return Guid.Empty;
        }

        if (await _roles.HasRoleAsync(userId, "Concierge", ct))
        {
            _logger.LogInformation("User {UserId} is Concierge — returning Guid.Empty", userId);
            return Guid.Empty;
        }

        _logger.LogWarning("User {UserId} has no recognized role", userId);
        return null;
    }
}
