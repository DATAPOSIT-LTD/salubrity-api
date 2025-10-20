// File: Salubrity.Infrastructure.Persistence.Repositories.HealthCamps.HealthCampParticipantServiceStatusRepository.cs

using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Infrastructure.Persistence.Repositories.HealthCamps;

public class HealthCampParticipantServiceStatusRepository : IHealthCampParticipantServiceStatusRepository
{
    private readonly AppDbContext _context;

    public HealthCampParticipantServiceStatusRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCampParticipantServiceStatus?> GetByParticipantAndAssignmentAsync(
        Guid participantId,
        Guid serviceAssignmentId,
        CancellationToken ct)
    {
        return await _context.HealthCampParticipantServiceStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ParticipantId == participantId &&
                x.ServiceAssignmentId == serviceAssignmentId,
                ct);
    }

    public async Task AddAsync(HealthCampParticipantServiceStatus entity, CancellationToken ct)
    {
        await _context.HealthCampParticipantServiceStatuses.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(HealthCampParticipantServiceStatus entity, CancellationToken ct)
    {
        _context.HealthCampParticipantServiceStatuses.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<HealthCampParticipantServiceStatus>> GetByParticipantAsync(Guid participantId, CancellationToken ct)
    {
        return await _context.HealthCampParticipantServiceStatuses
            .Include(x => x.ServiceAssignment)
            .ThenInclude(a => a.Role)
            .Where(x => x.ParticipantId == participantId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<List<HealthCampParticipantServiceStatus>> GetByCampAsync(Guid campId, CancellationToken ct)
    {
        return await _context.HealthCampParticipantServiceStatuses
            .Include(x => x.ServiceAssignment)
            .ThenInclude(a => a.HealthCamp)
            .Where(x => x.ServiceAssignment.HealthCampId == campId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
