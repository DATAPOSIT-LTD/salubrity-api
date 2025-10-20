using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Infrastructure.Persistence;

public class HealthCampServiceAssignmentRepository : IHealthCampServiceAssignmentRepository
{
    private readonly AppDbContext _db;
    public HealthCampServiceAssignmentRepository(AppDbContext db) => _db = db;

    public async Task<HealthCampServiceAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.HealthCampServiceAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }
    public async Task<List<HealthCampServiceAssignment>> GetBySubcontractorIdAsync(Guid subcontractorId, CancellationToken ct = default)
    {
        return await _db.HealthCampServiceAssignments
            .Include(x => x.HealthCamp)
            .Where(x => x.SubcontractorId == subcontractorId)
            .ToListAsync(ct);
    }
    public async Task<List<HealthCampServiceAssignment>> GetByCampIdAsync(Guid campId, CancellationToken ct = default)
    {
        return await _db.HealthCampServiceAssignments
            .Include(a => a.HealthCamp)
            .Where(a => a.HealthCampId == campId && !a.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<HealthCampServiceAssignment?> FirstOrDefaultAsync(
           Expression<Func<HealthCampServiceAssignment, bool>> predicate,
           CancellationToken ct = default)
    {
        return await _db.HealthCampServiceAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, ct);
    }


}
