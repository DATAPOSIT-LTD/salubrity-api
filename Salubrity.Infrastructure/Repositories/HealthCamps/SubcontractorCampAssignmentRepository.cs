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

        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine("[REPO CTOR] SubcontractorCampAssignmentRepository");
        Console.WriteLine($"DbContext Hash: {_db.GetHashCode()}");
        Console.WriteLine("-------------------------------------------------");
    }

    public async Task AddAsync(SubcontractorHealthCampAssignment assignment, CancellationToken ct = default)
    {


        // Add entity
        await _db.SubcontractorHealthCampAssignments.AddAsync(assignment, ct);

        var affectedRows = await _db.SaveChangesAsync(ct);

    }

    public async Task<List<SubcontractorHealthCampAssignment>> GetByCampIdAsync(
        Guid healthCampId,
        CancellationToken ct = default)
    {
        return await _db.SubcontractorHealthCampAssignments
            .Where(a => !a.IsDeleted && a.HealthCampId == healthCampId)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(
        Guid subcontractorId,
        Guid healthCampId,
        CancellationToken ct = default)
    {
        return await _db.SubcontractorHealthCampAssignments
            .AnyAsync(a =>
                !a.IsDeleted &&
                a.HealthCampId == healthCampId &&
                a.SubcontractorId == subcontractorId,
                ct);
    }

    public async Task<bool> HasActiveAssignmentsAsync(
        Guid subcontractorId,
        CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        return await _db.SubcontractorHealthCampAssignments
            .Where(a =>
                !a.IsDeleted &&
                a.SubcontractorId == subcontractorId &&
                a.StartDate <= today &&
                (a.EndDate == null || a.EndDate >= today))
            .AnyAsync(ct);
    }


    public async Task<List<SubcontractorHealthCampAssignment>> GetByCampAndSubcontractorAsync(
        Guid campId,
        Guid subcontractorId,
        CancellationToken ct = default)
    {
        return await _db.SubcontractorHealthCampAssignments
            .Where(a =>
                a.HealthCampId == campId &&
                a.SubcontractorId == subcontractorId)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        Console.WriteLine(">>> [REPO] SaveChangesAsync (explicit) called");
        var rows = await _db.SaveChangesAsync(ct);
        Console.WriteLine($">>> [REPO] SaveChangesAsync rows affected: {rows}");
    }
}
