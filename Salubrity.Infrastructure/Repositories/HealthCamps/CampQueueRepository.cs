// File: Salubrity.Infrastructure/Repositories/Camps/CampQueueRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Join;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Camps;

public class CampQueueRepository : ICampQueueRepository
{
    private readonly AppDbContext _db;
    public CampQueueRepository(AppDbContext db) => _db = db;

    private async Task<Guid> GetParticipantIdAsync(Guid userId, Guid campId, CancellationToken ct)
    {
        var pid = await _db.Set<HealthCampParticipant>()
            .Where(p => p.UserId == userId && p.HealthCampId == campId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(ct);

        if (pid == Guid.Empty)
            throw new UnauthorizedAccessException("Not a participant of this camp.");

        return pid;
    }

    public async Task CheckInAsync(Guid userId, CheckInRequestDto dto, CancellationToken ct = default)
    {
        var participantId = await GetParticipantIdAsync(userId, dto.CampId, ct);

        // allow only one active check-in across the camp
        var hasActive = await _db.Set<HealthCampStationCheckIn>().AnyAsync(x =>
            x.HealthCampId == dto.CampId &&
            x.HealthCampParticipantId == participantId &&
            (x.Status == "Queued" || x.Status == "InService"), ct);

        if (hasActive)
            throw new InvalidOperationException("You already have an active station.");

        // validate assignment belongs to camp
        var belongs = await _db.Set<HealthCampServiceAssignment>().AnyAsync(a =>
            a.Id == dto.AssignmentId && a.HealthCampId == dto.CampId, ct);

        if (!belongs) throw new InvalidOperationException("Invalid station.");

        var checkIn = new HealthCampStationCheckIn
        {
            Id = Guid.NewGuid(),
            HealthCampId = dto.CampId,
            HealthCampParticipantId = participantId,
            HealthCampServiceAssignmentId = dto.AssignmentId,
            Status = "Queued",
            Priority = dto.Priority ?? 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.Add(checkIn);
        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelMyCheckInAsync(Guid userId, Guid campId, CancellationToken ct = default)
    {
        var participantId = await GetParticipantIdAsync(userId, campId, ct);

        var active = await _db.Set<HealthCampStationCheckIn>()
            .Where(x => x.HealthCampId == campId
                     && x.HealthCampParticipantId == participantId
                     && (x.Status == "Queued" || x.Status == "InService"))
            .FirstOrDefaultAsync(ct);

        if (active == null) return;

        active.Status = "Canceled";
        active.FinishedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<CheckInStateDto> GetMyCheckInStateAsync(Guid userId, Guid campId, CancellationToken ct = default)
    {
        var participantId = await GetParticipantIdAsync(userId, campId, ct);

        var active = await _db.Set<HealthCampStationCheckIn>()
            .Include(x => x.Assignment).ThenInclude(a => a.Service)
            .Where(x => x.HealthCampId == campId
                     && x.HealthCampParticipantId == participantId
                     && (x.Status == "Queued" || x.Status == "InService"))
            .FirstOrDefaultAsync(ct);

        if (active == null)
            return new CheckInStateDto { CampId = campId, Status = "None" };

        return new CheckInStateDto
        {
            CampId = campId,
            ActiveAssignmentId = active.HealthCampServiceAssignmentId,
            ActiveStationName = active.Assignment.Service.Name,
            Status = active.Status
        };
    }

    public async Task<QueuePositionDto> GetMyPositionAsync(Guid userId, Guid campId, Guid assignmentId, CancellationToken ct = default)
    {
        var participantId = await GetParticipantIdAsync(userId, campId, ct);

        // whole queue for that station, ordered by priority desc, created asc
        var q = _db.Set<HealthCampStationCheckIn>()
            .Where(x => x.HealthCampId == campId
                     && x.HealthCampServiceAssignmentId == assignmentId
                     && x.Status == "Queued");

        var queue = await q
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new { x.Id, x.HealthCampParticipantId })
            .ToListAsync(ct);

        var yourIndex = queue.FindIndex(x => x.HealthCampParticipantId == participantId);
        var stationName = await _db.Set<HealthCampServiceAssignment>()
            .Where(a => a.Id == assignmentId)
            .Select(a => a.Service.Name)
            .FirstAsync(ct);

        // Get active state (Queued/InService/Completed/None)
        var meActive = await _db.Set<HealthCampStationCheckIn>()
            .Where(x => x.HealthCampId == campId
                     && x.HealthCampParticipantId == participantId
                     && x.HealthCampServiceAssignmentId == assignmentId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Status)
            .FirstOrDefaultAsync(ct);

        return new QueuePositionDto
        {
            AssignmentId = assignmentId,
            StationName = stationName,
            QueueLength = queue.Count,
            YourPosition = yourIndex >= 0 ? yourIndex + 1 : 0,
            Status = meActive ?? "None"
        };
    }

    public async Task StartServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default)
    {
        // Staff auth/ownership checks live in your policy layer (omitted here)
        var ci = await _db.Set<HealthCampStationCheckIn>().FirstOrDefaultAsync(x => x.Id == checkInId, ct);
        if (ci == null) throw new InvalidOperationException("Check-in not found.");
        if (ci.Status != "Queued") return;

        ci.Status = "InService";
        ci.StartedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task CompleteServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default)
    {
        var ci = await _db.Set<HealthCampStationCheckIn>().FirstOrDefaultAsync(x => x.Id == checkInId, ct);
        if (ci == null) throw new InvalidOperationException("Check-in not found.");
        if (ci.Status != "InService") return;

        ci.Status = "Completed";
        ci.FinishedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
