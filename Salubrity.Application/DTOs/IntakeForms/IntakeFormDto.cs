namespace Salubrity.Application.DTOs.IntakeForms;

public class IntakeFormDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateIntakeFormDto
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateIntakeFormDto
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}
