// Salubrity.Infrastructure/Repositories/Identity/UsersReadRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Infrastructure.Persistence;

public class UsersReadRepository : IUsersReadRepository
{
    private readonly AppDbContext _db;
    public UsersReadRepository(AppDbContext db) => _db = db;

    public Task<bool> IsActiveAsync(Guid userId, CancellationToken ct = default) =>
        _db.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.IsActive, ct);
}