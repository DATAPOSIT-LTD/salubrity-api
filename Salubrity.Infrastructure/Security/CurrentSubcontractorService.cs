// Salubrity.Infrastructure/Security/CurrentSubcontractorService.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Exceptions;

public class CurrentSubcontractorService : ICurrentSubcontractorService
{
    private readonly AppDbContext _db;
    public CurrentSubcontractorService(AppDbContext db) => _db = db;

    public async Task<Guid> GetRequiredSubcontractorIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => new { u.Id })
            .FirstOrDefaultAsync(ct)
            ?? throw new UnauthorizedException("User not found or inactive.");

        var hasRole = await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.Role.IsActive && ur.Role.Name == "Subcontractor")
            .AnyAsync(ct);

        if (!hasRole)
            throw new UnauthorizedException("Requires Subcontractor role.");

        var subcontractorId = await _db.Subcontractors
            .Where(s => s.UserId == userId && s.Status.IsActive)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new UnauthorizedException("No subcontractor profile bound to this user.");

        return subcontractorId;
    }
}
