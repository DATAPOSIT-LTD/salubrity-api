using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampCheckInService : IHealthCampCheckInService
{
    private readonly IHealthCampCheckInRepository _repo;

    public HealthCampCheckInService(IHealthCampCheckInRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ParticipantStationStatusDto>> GetParticipantStationStatusesAsync(Guid participantId, CancellationToken ct = default)
    {
        var participant = await _repo.GetParticipantAsync(participantId, ct)
            ?? throw new NotFoundException("HealthCampParticipant", participantId.ToString());

        var assignments = await _repo.GetAssignmentsAsync(participant.HealthCampId, ct);
        var checkIns = await _repo.GetCheckInsAsync(participantId, ct);

        var result = assignments.Select(a =>
        {
            var checkIn = checkIns.FirstOrDefault(c => c.HealthCampServiceAssignmentId == a.Id);

            return new ParticipantStationStatusDto
            {
                ServiceName = a.AssignmentName ?? "Unknown Service",
                CheckInTime = checkIn?.StartedAt,
                CheckOutTime = checkIn?.FinishedAt,
                Status = checkIn?.Status switch
                {
                    null => "Not served",
                    string s when s.Equals("InService", StringComparison.OrdinalIgnoreCase) => "Ongoing",
                    string s when s.Equals("Completed", StringComparison.OrdinalIgnoreCase) => "Completed",
                    string s when s.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) => "Cancelled",
                    _ => checkIn.Status
                }
            };
        }).ToList();

        return result;
    }
}
