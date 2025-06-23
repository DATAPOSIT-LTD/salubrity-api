using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Application.Interfaces.Repositories.Rbac;


namespace Salubrity.Infrastructure.Repositories.Rbac;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync() =>
        await _context.Roles.AsNoTracking().ToListAsync();

    public async Task<Role?> GetByIdAsync(Guid id) =>
        await _context.Roles.FindAsync(id);

    public async Task AddRoleAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRoleAsync(Role role)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Role role)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }
}
