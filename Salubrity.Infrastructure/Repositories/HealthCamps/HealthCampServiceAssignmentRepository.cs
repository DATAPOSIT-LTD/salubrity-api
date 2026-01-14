using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthCamps;

public class HealthCampServiceAssignmentRepository
    : IHealthCampServiceAssignmentRepository
{
    private readonly AppDbContext _db;

    public HealthCampServiceAssignmentRepository(AppDbContext db)
    {
        _db = db;
    }

    // ─────────────────────────────────────────────
    // READ OPERATIONS
    // ─────────────────────────────────────────────

    public async Task<HealthCampServiceAssignment?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _db.HealthCampServiceAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<List<HealthCampServiceAssignment>> GetBySubcontractorIdAsync(
        Guid subcontractorId,
        CancellationToken ct = default)
    {
        return await _db.HealthCampServiceAssignments
            .AsNoTracking()
            .Include(x => x.HealthCamp)
            .Where(x =>
                x.SubcontractorId == subcontractorId &&
                !x.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<List<HealthCampServiceAssignment>> GetByCampIdAsync(
        Guid campId,
        CancellationToken ct = default)
    {
        return await _db.HealthCampServiceAssignments
            .AsNoTracking()
            .Include(x => x.HealthCamp)
            .Where(x =>
                x.HealthCampId == campId &&
                !x.IsDeleted)
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

    public async Task<bool> ExistsAsync(
        Guid campId,
        Guid subcontractorId,
        Guid assignmentId,
        CancellationToken ct = default)
    {
        return await _db.HealthCampServiceAssignments
            .AsNoTracking()
            .AnyAsync(x =>
                x.HealthCampId == campId &&
                x.SubcontractorId == subcontractorId &&
                x.AssignmentId == assignmentId &&
                !x.IsDeleted,
                ct);
    }

    // ─────────────────────────────────────────────
    // WRITE OPERATIONS
    // ─────────────────────────────────────────────

    public async Task AddAsync(
        HealthCampServiceAssignment assignment,
        CancellationToken ct = default)
    {
        _db.HealthCampServiceAssignments.Add(assignment);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(
        IEnumerable<HealthCampServiceAssignment> assignments,
        CancellationToken ct = default)
    {
        _db.HealthCampServiceAssignments.AddRange(assignments);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
