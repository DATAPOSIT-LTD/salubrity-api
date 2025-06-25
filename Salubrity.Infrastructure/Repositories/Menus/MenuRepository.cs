using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Menus;
using Salubrity.Domain.Entities.Menus;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Menus;

public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _context;

    public MenuRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Menu> AddAsync(Menu entity)
    {
        _context.Menus.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Menu?> GetByIdAsync(Guid id)
    {
        return await _context.Menus.FindAsync(id);
    }

    public async Task<List<Menu>> GetAllAsync()
    {
        return await _context.Menus
            .AsNoTracking()
            .OrderBy(m => m.Order)
            .ToListAsync();
    }

    public async Task<Menu> UpdateAsync(Menu entity)
    {
        _context.Menus.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Menu entity)
    {
        _context.Menus.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
