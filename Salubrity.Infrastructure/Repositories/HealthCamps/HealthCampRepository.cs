using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthCamps;

public class HealthCampRepository : IHealthCampRepository
{
    private readonly AppDbContext _context;

    public HealthCampRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Camp>> GetAllAsync() =>
        await _context.Camps.Where(c => !c.IsDeleted).ToListAsync();

    public async Task<Camp?> GetByIdAsync(Guid id) =>
        await _context.Camps.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

    public async Task<Camp> CreateAsync(Camp entity)
    {
        _context.Camps.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Camp> UpdateAsync(Camp entity)
    {
        _context.Camps.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var camp = await _context.Camps.FindAsync(id);
        if (camp != null)
        {
            camp.IsDeleted = true;
            camp.DeletedAt = DateTime.UtcNow;
            _context.Camps.Update(camp);
            await _context.SaveChangesAsync();
        }
    }
}
