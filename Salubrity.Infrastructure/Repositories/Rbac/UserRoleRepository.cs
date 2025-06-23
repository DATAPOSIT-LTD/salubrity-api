using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Application.Interfaces.Repositories.Rbac;

namespace Salubrity.Infrastructure.Repositories.Rbac;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly AppDbContext _db;

    public UserRoleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserRole>> GetAllAsync()
    {
        return await _db.UserRoles.ToListAsync();
    }

    public async Task<UserRole?> GetByIdAsync(Guid id)
    {
        return await _db.UserRoles.FindAsync(id);
    }

    public async Task AddAsync(UserRole userRole)
    {
        await _db.UserRoles.AddAsync(userRole);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserRole userRole)
    {
        _db.UserRoles.Update(userRole);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserRole userRole)
    {
        _db.UserRoles.Remove(userRole);
        await _db.SaveChangesAsync();
    }
}
