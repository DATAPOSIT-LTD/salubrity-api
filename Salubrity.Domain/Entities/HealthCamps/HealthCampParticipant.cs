// File: Salubrity.Domain.Entities.HealthCamps.HealthCampParticipant.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Domain.Entities.Join;

public class HealthCampParticipant : BaseAuditableEntity
{
    public Guid HealthCampId { get; set; }
    public HealthCamp HealthCamp { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? PatientId { get; set; }
    public Patient? Patient { get; set; }

    public bool IsEmployee { get; set; } = false;

    public string? Notes { get; set; }

    public DateTime? ParticipatedAt { get; set; }
}
