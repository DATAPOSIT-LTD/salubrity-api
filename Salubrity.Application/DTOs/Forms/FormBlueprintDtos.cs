// File: Salubrity.Application/DTOs/Forms/FormBlueprintDtos.cs
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    // Top-level create (a whole form)
    public class CreateFormBlueprintDto
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        // Optional sections, each with fields
        public List<CreateFormBlueprintSectionDto> Sections { get; set; } = [];

        // Optional unsectioned fields (go straight under the form)
        public List<CreateFormBlueprintFieldDto> Fields { get; set; } = [];
    }

    public class CreateFormBlueprintSectionDto
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        public int Order { get; set; } = 0;

        public List<CreateFormBlueprintFieldDto> Fields { get; set; } = [];
    }

    // NOTE: This is a blueprint field (no FormId/SectionId in the payload).
    public class CreateFormBlueprintFieldDto
    {
        [Required, StringLength(200)]
        public string Label { get; set; } = null!;

        [Required, StringLength(100)]
        public string FieldType { get; set; } = null!;

        public bool IsRequired { get; set; }
        public int Order { get; set; }

        // Conditional logic (string-based references so we can resolve after create)
        public bool HasConditionalLogic { get; set; } = false;

        [StringLength(50)]
        public string? ConditionalLogicType { get; set; } // "Show","Hide",...

        // Use label/value to reference other created items within the SAME form
        public string? TriggerFieldLabel { get; set; }       // will resolve to TriggerFieldId
        public string? TriggerValueOptionValue { get; set; } // will resolve to TriggerValueOptionId

        // Validation
        [StringLength(100)]
        public string? ValidationType { get; set; }        // "Range","Regex"
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
        public string? LayoutPosition { get; set; } // "Left","Right","FullWidth"

        public List<CreateFieldOptionDto> Options { get; set; } = [];
    }


    // Minimal readback
    public class FormBlueprintResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
