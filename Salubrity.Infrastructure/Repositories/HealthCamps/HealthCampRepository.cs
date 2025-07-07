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

    public async Task<List<HealthCamp>> GetAllAsync() =>
        await _context.HealthCamps.Where(c => !c.IsDeleted).ToListAsync();

    public async Task<HealthCamp?> GetByIdAsync(Guid id) =>
        await _context.HealthCamps.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

    public async Task<HealthCamp> CreateAsync(HealthCamp entity)
    {
        _context.HealthCamps.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<HealthCamp> UpdateAsync(HealthCamp entity)
    {
        _context.HealthCamps.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var camp = await _context.HealthCamps.FindAsync(id);
        if (camp != null)
        {
            camp.IsDeleted = true;
            camp.DeletedAt = DateTime.UtcNow;
            _context.HealthCamps.Update(camp);
            await _context.SaveChangesAsync();
        }
    }
}
