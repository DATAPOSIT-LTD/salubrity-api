// Salubrity.Infrastructure/Repositories/Identity/UserRoleReadRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Infrastructure.Persistence;

public class UserRoleReadRepository : IUserRoleReadRepository
{
    private readonly AppDbContext _db;
    public UserRoleReadRepository(AppDbContext db) => _db = db;

    public Task<bool> HasRoleAsync(Guid userId, string roleName, CancellationToken ct = default) =>
        _db.UserRoles.AsNoTracking()
            .AnyAsync(ur => ur.UserId == userId
                         && ur.Role.IsActive
                         && ur.Role.Name == roleName, ct);
}