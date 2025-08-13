// File: Salubrity.Application/DTOs/Forms/FormFieldDtos.cs
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
  // ---------- CREATE ----------

  public class CreateFormFieldDto
  {
    [Required]
    public Guid FormId { get; set; }                 // entity: required

    public Guid? SectionId { get; set; }             // entity: nullable

    [Required, StringLength(200)]
    public string Label { get; set; } = null!;       // entity: [Required, MaxLength(200)]

    [Required, StringLength(100)]
    public string FieldType { get; set; } = null!;   // entity allows null; we require it at API layer

    public bool IsRequired { get; set; }
    public int Order { get; set; }

    // Conditional logic
    public bool HasConditionalLogic { get; set; } = false;

    [StringLength(50)]
    public string? ConditionalLogicType { get; set; } // entity: MaxLength(50)

    public Guid? TriggerFieldId { get; set; }         // entity: nullable
    public Guid? TriggerValueOptionId { get; set; }   // entity: nullable

    // Validation
    [StringLength(100)]
    public string? ValidationType { get; set; }       // entity: MaxLength(100)

    [StringLength(500)]
    public string? ValidationPattern { get; set; }    // entity: MaxLength(500)

    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }

    [StringLength(500)]
    public string? CustomErrorMessage { get; set; }   // entity: MaxLength(500)

    // Layout
    [StringLength(50)]
    public string? LayoutPosition { get; set; }       // entity: MaxLength(50)

    public List<CreateFieldOptionDto> Options { get; set; } = [];
  }
}