using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Lookup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormResponses")]
public class IntakeFormResponse : BaseAuditableEntity
{
    [Required]
    public Guid IntakeFormVersionId { get; set; }

    [ForeignKey(nameof(IntakeFormVersionId))]
    public IntakeFormVersion Version { get; set; } = default!;

    [Required]
    public Guid SubmittedByUserId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = default!;

    // WHAT USER ACTUALLY SELECTED
    [Required]
    public Guid SubmittedServiceId { get; set; }

    [Required]
    public PackageItemType SubmittedServiceType { get; set; }

    // RESOLVED FK TO Service
    [Required]
    public Guid ResolvedServiceId { get; set; }

    [ForeignKey(nameof(ResolvedServiceId))]
    public Service ResolvedService { get; set; } = default!;

    [Required]
    public Guid ResponseStatusId { get; set; }

    [ForeignKey(nameof(ResponseStatusId))]
    public IntakeFormResponseStatus Status { get; set; } = default!;

    public ICollection<IntakeFormFieldResponse> FieldResponses { get; set; } = [];
}
