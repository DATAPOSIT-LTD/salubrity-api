public sealed class DynamicFieldResponseDto
{
    public Guid FieldId { get; init; } // ID of IntakeFormField
    public string? Value { get; init; } // Text/textarea/number/etc
    public Guid? SelectedOptionId { get; init; } // For Radio, Dropdown, etc
    public Guid? SectionId { get; init; } // ID of IntakeFormSection
}
