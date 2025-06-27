namespace Salubrity.Application.DTOs.HealthcareServices;

public class ServicePackageResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<Guid> ServiceSubcategoryIds { get; set; } = [];
    public decimal? Price { get; set; }
    public string? RangeOfPeople { get; set; }
    public bool IsActive { get; set; }
}

public class CreateServicePackageDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required List<Guid> ServiceSubcategoryIds { get; set; }
    public decimal? Price { get; set; }
    public string? RangeOfPeople { get; set; }
}

public class UpdateServicePackageDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required List<Guid> ServiceSubcategoryIds { get; set; }
    public decimal? Price { get; set; }
    public string? RangeOfPeople { get; set; }
    public bool IsActive { get; set; }
}
