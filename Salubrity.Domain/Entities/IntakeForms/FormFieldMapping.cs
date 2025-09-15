using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

public class FormFieldMapping
{
    public Guid Id { get; set; }

    public Guid IntakeFormVersionId { get; set; }

    public Guid IntakeFormFieldId { get; set; }

    public string Alias { get; set; } = null!; // CSV header column

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public IntakeFormVersion? IntakeFormVersion { get; set; }
    public IntakeFormField? IntakeFormField { get; set; }
}
