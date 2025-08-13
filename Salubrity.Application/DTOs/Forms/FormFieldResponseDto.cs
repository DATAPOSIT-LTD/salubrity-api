// File: Salubrity.Application/DTOs/Forms/FormFieldResponseDto.cs
namespace Salubrity.Application.DTOs.Forms
{
    public class FormFieldResponseDto
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public Guid? SectionId { get; set; }
        public string Label { get; set; } = null!;
        public string? FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }

        public bool HasConditionalLogic { get; set; }
        public string? ConditionalLogicType { get; set; }
        public Guid? TriggerFieldId { get; set; }
        public Guid? TriggerValueOptionId { get; set; }

        public string? ValidationType { get; set; }
        public string? ValidationPattern { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? CustomErrorMessage { get; set; }

        public string? LayoutPosition { get; set; }

        public List<FieldOptionResponseDto> Options { get; set; } = [];
    }
}
