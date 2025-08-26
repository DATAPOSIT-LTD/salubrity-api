// File: Salubrity.Domain/Entities/HealthCamps/HealthCampStationCheckIn.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Join; // HealthCampParticipant
using Salubrity.Domain.Entities.HealthCamps; // HealthCampServiceAssignment
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthCamps;

[Table("HealthCampStationCheckIns")]
public class HealthCampStationCheckIn : BaseAuditableEntity
{
    public Guid HealthCampId { get; set; }
    public Guid HealthCampParticipantId { get; set; }
    public virtual HealthCampParticipant Participant { get; set; } = default!;

    // Station
    public Guid HealthCampServiceAssignmentId { get; set; }
    public virtual HealthCampServiceAssignment Assignment { get; set; } = default!;

    // Queue state
    // Queued → InService → Completed OR Canceled
    public string Status { get; set; } = "Queued";

    // Priority: higher number = higher priority, FIFO within same priority
    public int Priority { get; set; } = 0;

    // When service actually started/finished
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
}
