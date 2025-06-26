namespace Salubrity.Application.DTOs.Lookups;

/// <summary>
/// Standard response for simple lookup entities (name + description).
/// </summary>
public class BaseLookupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
