namespace Salubrity.Application.DTOs.HealthcareServices;

public class ServiceSubcategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public Guid? IntakeFormId { get; set; }
    public decimal Price { get; set; }
    public int? DurationMinutes { get; set; }
}

// public class CreateServiceSubcategoryDto
// {
//     public string Name { get; set; } = default!;
//     public string? Description { get; set; }
//     public Guid ServiceCategoryId { get; set; }
//     public decimal Price { get; set; }
//     public int? DurationMinutes { get; set; }
// }

// public class UpdateServiceSubcategoryDto
// {
//     public string Name { get; set; } = default!;
//     public string? Description { get; set; }
//     public Guid ServiceCategoryId { get; set; }
//     public decimal Price { get; set; }
//     public int? DurationMinutes { get; set; }
// }
