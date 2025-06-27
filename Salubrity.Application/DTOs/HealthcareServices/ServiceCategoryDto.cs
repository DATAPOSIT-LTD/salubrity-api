namespace Salubrity.Application.DTOs.HealthcareServices;

public class CreateServiceCategoryDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid ServiceId { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
}

public class UpdateServiceCategoryDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
}

public class ServiceCategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid ServiceId { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsActive { get; set; }
}
