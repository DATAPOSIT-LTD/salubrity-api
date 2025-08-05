using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormVersions")]
public class IntakeFormVersion : BaseAuditableEntity
{
    public Guid IntakeFormId { get; set; }

    [ForeignKey(nameof(IntakeFormId))]
    public IntakeForm IntakeForm { get; set; } = default!;

    /// <summary>
    /// Version number, e.g. 1, 2, 3
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Indicates whether this version is the currently active one for public use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional notes or internal description for administrators
    /// </summary>
    [MaxLength(500)]
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Allows defining rules or metadata at the form-level (e.g., expiration policy, theme, visibility settings)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? MetadataJson { get; set; }

    public ICollection<IntakeFormSection> Sections { get; set; } = [];
}
