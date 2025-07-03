using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormResponses")]
public class IntakeFormResponse : BaseAuditableEntity
{
    public Guid IntakeFormVersionId { get; set; }

    [ForeignKey(nameof(IntakeFormVersionId))]
    public IntakeFormVersion Version { get; set; } = default!;

    public Guid SubmittedByUserId { get; set; }

    public Guid? ServiceId { get; set; } // For linking with a service context

    public ICollection<IntakeFormFieldResponse> FieldResponses { get; set; } = new List<IntakeFormFieldResponse>();
}
