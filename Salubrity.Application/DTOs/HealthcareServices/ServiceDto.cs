using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.HealthcareServices;

public class ServiceResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? PricePerPerson { get; set; }

    public Guid? IndustryId { get; set; }

    public Guid? IntakeFormId { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    // This holds either category or subcategory IDs
    public List<Guid>? LinkedServiceIds { get; set; }
}

public class CreateServiceDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or positive.")]
    public decimal? PricePerPerson { get; set; }

    public Guid? IndustryId { get; set; }

    public Guid? IntakeFormId { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Unified list of category/subcategory IDs
    public List<Guid> LinkedServiceIds { get; set; } = new();
}

public class UpdateServiceDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or positive.")]
    public decimal? PricePerPerson { get; set; }

    public Guid? IndustryId { get; set; }

    public Guid? IntakeFormId { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public List<Guid> LinkedServiceIds { get; set; } = new();
}
