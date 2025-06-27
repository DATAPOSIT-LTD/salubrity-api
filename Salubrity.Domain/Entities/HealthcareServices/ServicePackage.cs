using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.HealthcareServices;

[Table("ServicePackages")]
public class ServicePackage : BaseAuditableEntity
{
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    public required List<Guid> ServiceSubcategoryIds { get; set; }

    [Column(TypeName = "jsonb")]
    [JsonIgnore]
    public string SubcategoryIdsJson
    {
        get => JsonSerializer.Serialize(ServiceSubcategoryIds);
        set => ServiceSubcategoryIds = JsonSerializer.Deserialize<List<Guid>>(value) ?? new();
    }

    [Column(TypeName = "numeric(10,2)")]
    public decimal? Price { get; set; }

    [MaxLength(100)]
    public string? RangeOfPeople { get; set; }

    public bool IsActive { get; set; } = true;
}
