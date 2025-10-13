using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Join;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthCamps;

public class HealthCampCheckInRepository : IHealthCampCheckInRepository
{
    private readonly AppDbContext _context;

    public HealthCampCheckInRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCampParticipant?> GetParticipantAsync(Guid participantId, CancellationToken ct = default)
    {
        return await _context.HealthCampParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == participantId, ct);
    }

    public async Task<List<(Guid Id, string? AssignmentName)>> GetAssignmentsAsync(Guid healthCampId, CancellationToken ct = default)
    {
        return await _context.HealthCampServiceAssignments
            .Where(a => a.HealthCampId == healthCampId && !a.IsDeleted)
            .Select(a => new ValueTuple<Guid, string?>(a.Id, a.AssignmentName))
            .ToListAsync(ct);
    }

    public async Task<List<HealthCampStationCheckIn>> GetCheckInsAsync(Guid participantId, CancellationToken ct = default)
    {
        return await _context.HealthCampStationCheckIns
            .Where(c => c.HealthCampParticipantId == participantId && !c.IsDeleted)
            .ToListAsync(ct);
    }
}
