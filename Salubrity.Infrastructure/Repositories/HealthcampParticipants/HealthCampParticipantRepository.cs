using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Join;
using Salubrity.Infrastructure.Persistence;

public class HealthCampParticipantRepository : IHealthCampParticipantRepository
{
    private readonly AppDbContext _db;

    public HealthCampParticipantRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCampParticipant?> GetPatientParticipantAsync(
        Guid campId,
        Guid patientId,
        CancellationToken ct)
    {
        return await _db.HealthCampParticipants
            .FirstOrDefaultAsync(p =>
                p.HealthCampId == campId &&
                p.PatientId == patientId &&
                !p.IsDeleted,
                ct);
    }

    public async Task<List<Guid>> GetServiceIdsForCampAsync(
        Guid campId,
        CancellationToken ct)
    {
        return await _db.HealthCampServiceAssignments
            .Where(a =>
                a.HealthCampId == campId &&
                !a.IsDeleted &&
                a.AssignmentType == PackageItemType.Service)
            .Select(a => a.AssignmentId)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<List<IntakeFormResponse>> GetFormResponsesForPatientAndServicesAsync(
        Guid patientId,
        List<Guid> serviceIds,
        CancellationToken ct)
    {
        return await _db.IntakeFormResponses
            .Include(r => r.FieldResponses)
            .Where(r =>
                r.PatientId == patientId &&
                serviceIds.Contains(r.ResolvedServiceId) &&
                !r.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}
