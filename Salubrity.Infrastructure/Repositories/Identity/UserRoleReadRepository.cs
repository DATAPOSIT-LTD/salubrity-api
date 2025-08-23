// Salubrity.Infrastructure/Repositories/Identity/UserRoleReadRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Infrastructure.Persistence;

public class UserRoleReadRepository : IUserRoleReadRepository
{
    private readonly AppDbContext _db;

    public UserRoleReadRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
    {
        return await _db.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName && ur.Role.IsActive, ct);
    }

    public async Task<bool> HasAnyRoleAsync(Guid userId, IEnumerable<string> roleNames, CancellationToken ct = default)
    {
        return await _db.UserRoles
            .AnyAsync(ur => ur.UserId == userId && roleNames.Contains(ur.Role.Name) && ur.Role.IsActive, ct);
    }
}