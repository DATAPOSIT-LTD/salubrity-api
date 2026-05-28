using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Organizations;

public class OrganizationDepartmentRepository : IOrganizationDepartmentRepository
{
    private readonly AppDbContext _db;
    public OrganizationDepartmentRepository(AppDbContext db) => _db = db;

    public Task<List<Department>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default) =>
        _db.Set<Department>()
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.OrganizationId == organizationId)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Set<Department>().FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);

    public Task<bool> ExistsByNameAsync(Guid organizationId, string name, CancellationToken ct = default) =>
        _db.Set<Department>()
            .AsNoTracking()
            .AnyAsync(d => !d.IsDeleted && d.OrganizationId == organizationId && d.Name == name, ct);

    public async Task AddAsync(Department entity, CancellationToken ct = default)
    {
        _db.Set<Department>().Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<Department> entities, CancellationToken ct = default)
    {
        _db.Set<Department>().AddRange(entities);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Set<Department>().FindAsync(new object[] { id }, ct);
        if (entity == null || entity.IsDeleted) return;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
