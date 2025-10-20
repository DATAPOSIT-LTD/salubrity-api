// File: Salubrity.Domain.Entities.HealthCamps.HealthCampParticipantServiceStatus.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Join;

namespace Salubrity.Domain.Entities.HealthCamps;

public class HealthCampParticipantServiceStatus : BaseAuditableEntity
{
    public Guid ParticipantId { get; set; }
    public HealthCampParticipant Participant { get; set; } = null!;

    public Guid ServiceAssignmentId { get; set; }
    public HealthCampServiceAssignment ServiceAssignment { get; set; } = null!;

    public Guid? SubcontractorId { get; set; }
    public DateTime? ServedAt { get; set; }

    public string? Notes { get; set; }
}
