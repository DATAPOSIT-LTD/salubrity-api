// File: Salubrity.Domain.Entities.HealthcareServices.Service.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.IntakeForms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthcareServices;

[Table("Services")]
public class Service : BaseAuditableEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal? PricePerPerson { get; set; }

    public Guid? IndustryId { get; set; }

    [ForeignKey(nameof(IndustryId))]
    public Industry? Industry { get; set; }

    public Guid? IntakeFormId { get; set; }

    [ForeignKey(nameof(IntakeFormId))]
    public IntakeForm? IntakeForm { get; set; }

    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property for linked categories
    public ICollection<ServiceCategory> Categories { get; set; } = new List<ServiceCategory>();
}
