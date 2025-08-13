// File: Salubrity.Application/DTOs/Forms/UpdateFormFieldDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    // ---------- UPDATE (replace/upsert semantics for Options) ----------

    public class UpdateFormFieldDto
    {
        [Required]
        public Guid FormId { get; set; }                 // keep explicit; entity requires it

        public Guid? SectionId { get; set; }

        [Required, StringLength(200)]
        public string Label { get; set; } = null!;

        [Required, StringLength(100)]
        public string FieldType { get; set; } = null!;

        public bool IsRequired { get; set; }
        public int Order { get; set; }

        // Conditional logic
        public bool HasConditionalLogic { get; set; } = false;

        [StringLength(50)]
        public string? ConditionalLogicType { get; set; }

        public Guid? TriggerFieldId { get; set; }
        public Guid? TriggerValueOptionId { get; set; }

        // Validation
        [StringLength(100)]
        public string? ValidationType { get; set; }

        [StringLength(500)]
        public string? ValidationPattern { get; set; }

        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }

        [StringLength(500)]
        public string? CustomErrorMessage { get; set; }

        // Layout
        [StringLength(50)]
        public string? LayoutPosition { get; set; }

        // Upsert-by-Id: if Id is null -> create, if present -> update; missing -> delete
        public List<UpdateFieldOptionDto> Options { get; set; } = [];
    }

}
