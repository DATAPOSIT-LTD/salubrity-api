namespace Salubrity.Application.DTOs.Lookups;

public class InsuranceProviderResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
}
