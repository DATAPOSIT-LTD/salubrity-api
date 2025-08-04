using System.ComponentModel.DataAnnotations;

public class UpdateFormRequestDto
{
    [Required(ErrorMessage = "Form name is required.")]
    [StringLength(100, ErrorMessage = "Form name cannot exceed 100 characters")]
     public Guid? Id { get; set; }
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public List<UpdateFormSectionRequestDto> Sections { get; set; } = new();
}
public class UpdateFormSectionRequestDto
{
    public Guid? Id { get; set; } // Null for new sections, populated for existing ones 

    [Required(ErrorMessage = "Section name is required")]
    [StringLength(100, ErrorMessage = "Section name cannot exceed 100 characters")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative integer")]
    public int Order { get; set; }

    public List<UpdateFormFieldRequestDto> SectionFields { get; set; } = new();
}
public class UpdateFormFieldRequestDto
{
    public Guid? Id { get; set; } // Null for new fields, populated for existing ones
    
     public Guid SectionId { get; set; }

    [Required(ErrorMessage = "Field label is required")]
    [StringLength(100, ErrorMessage = "Field label cannot exceed 100 characters")]
    public string Label { get; set; } = null!;

    [Required(ErrorMessage = "Field type is required")]
    [RegularExpression("text|number|select|checkbox|radio|date", 
        ErrorMessage = "Invalid field type")]
    public string FieldType { get; set; } = null!;

    public bool IsRequired { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative integer")]
    public int Order { get; set; }

    
    public List<UpdateFieldOptionRequestDto> Options { get; set; } = new();
}

public class UpdateFieldOptionRequestDto
{
    public Guid? Id { get; set; } // Null for new options, populated for existing ones

    [Required(ErrorMessage = "Option value is required")]
    [StringLength(100, ErrorMessage = "Option value cannot exceed 100 characters")]
    public string Value { get; set; } = null!;

    [Required(ErrorMessage = "Display text is required")]
    [StringLength(100, ErrorMessage = "Display text cannot exceed 100 characters")]
    public string DisplayText { get; set; } = null!;
}