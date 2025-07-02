namespace Salubrity.Application.DTOs.HealthcareServices;

public class ServiceResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? PricePerPerson { get; set; }
    public Guid? IndustryId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateServiceDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? PricePerPerson { get; set; }
    public Guid IndustryId { get; set; }
}

public class UpdateServiceDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? PricePerPerson { get; set; }
    public Guid IndustryId { get; set; }
}
