﻿using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Organizations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthCamps;

[Table("HealthCamps")]
public class HealthCamp : BaseAuditableEntity
{
    [Required]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    [Column(TypeName = "date")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? EndDate { get; set; }

    public TimeSpan? StartTime { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey("Organization")]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; } = default!;

    // This defines the actual camp package dynamically
    public virtual ICollection<HealthCampPackageItem> PackageItems { get; set; } = new List<HealthCampPackageItem>();

    // Assignments per subcontractor per service
    public virtual ICollection<HealthCampServiceAssignment> ServiceAssignments { get; set; } = new List<HealthCampServiceAssignment>();
    public ICollection<HealthCampParticipant> Participants { get; set; } = new List<HealthCampParticipant>();

}
