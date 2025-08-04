using System.ComponentModel.DataAnnotations;

public class CreateFormRequestDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public List<CreateFormSectionRequestDto> Sections { get; set; } = new();
   
}

public class CreateFormSectionRequestDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Order { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "FormId must be a non-negative integer.")]
        public int FormId { get; set; }

        //[ValidateComplexType]
        public List<CreateFormFieldRequestDto> Fields { get; set; } = new();
    }

public class CreateFormFieldRequestDto
{
    public Guid SectionId { get; set; }

    [Required(ErrorMessage = "Field label is required")]
    [StringLength(100, ErrorMessage = "Field label cannot exceed 100 characters")]
    public string Label { get; set; } = null!;
    public string FieldType { get; set; } = null!; // "text", "number", "select", "checkbox", etc.
    public bool IsRequired { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative integer.")]
    public int Order { get; set; }
    public List<CreateFieldOptionRequestDto> Options { get; set; } = new();
}
public class CreateFieldOptionRequestDto
{
    public string Value { get; set; } = null!;
    public string DisplayText { get; set; } = null!;
}