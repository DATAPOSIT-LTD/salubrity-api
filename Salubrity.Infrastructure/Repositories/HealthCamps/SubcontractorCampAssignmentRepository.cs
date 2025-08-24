// File: Infrastructure/Persistence/Repositories/HealthCamps/SubcontractorCampAssignmentRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.Subcontractor;

namespace Salubrity.Infrastructure.Persistence.Repositories.HealthCamps;

public class SubcontractorCampAssignmentRepository : ISubcontractorCampAssignmentRepository
{
    private readonly AppDbContext _db;

    public SubcontractorCampAssignmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(SubcontractorHealthCampAssignment assignment, CancellationToken ct = default)
    {
        await _db.SubcontractorHealthCampAssignments.AddAsync(assignment, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<SubcontractorHealthCampAssignment>> GetByCampIdAsync(Guid healthCampId, CancellationToken ct = default)
    {
        return await _db.SubcontractorHealthCampAssignments
            .Where(a => !a.IsDeleted && a.HealthCampId == healthCampId)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid subcontractorId, Guid healthCampId, CancellationToken ct = default)
    {
        return await _db.SubcontractorHealthCampAssignments
            .AnyAsync(a => !a.IsDeleted &&
                           a.HealthCampId == healthCampId &&
                           a.SubcontractorId == subcontractorId, ct);
    }
}
