public class PatientAssessmentResponseProjection
{
    public string FormName { get; set; } = default!;
    public string SectionName { get; set; } = default!;
    public int SectionOrder { get; set; }
    public string FieldLabel { get; set; } = default!;
    public int FieldOrder { get; set; }
    public string? Value { get; set; }
    public string? SelectedOption { get; set; }
}
