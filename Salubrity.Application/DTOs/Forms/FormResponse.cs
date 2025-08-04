public class FormResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; } = true;
   
    public ICollection<FormSectionResponseDto> Sections { get; set; } = new List<FormSectionResponseDto>();
}

public class FormSectionResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Order { get; set; }
        public ICollection<FormFieldResponseDto> Fields { get; set; } = new List<FormFieldResponseDto>();
    }


public class FormFieldResponseDto
{
    public Guid Id { get; set; }
    public string Label { get; set; }
    public string FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public List<FieldOptionResponseDto> Options { get; set; } = new();
}

public class FieldOptionResponseDto
{
    public Guid Id { get; set; }
    public string Value { get; set; }
    public string DisplayText { get; set; }
}