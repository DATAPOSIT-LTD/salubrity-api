                    using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Join;

namespace Salubrity.Domain.Entities.HealthCamps
{
    [Table("HealthCampParticipantPackages")]
    public class HealthCampParticipantPackage : BaseAuditableEntity
    {
        [Required]
        [ForeignKey(nameof(Participant))]
        public Guid ParticipantId { get; set; }

        [Required]
        [ForeignKey(nameof(HealthCampPackage))]
        public Guid HealthCampPackageId { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // 🔗 Navigation properties
        public virtual HealthCampParticipant Participant { get; set; } = null!;
        public virtual HealthCampPackage HealthCampPackage { get; set; } = null!;
    }
}
