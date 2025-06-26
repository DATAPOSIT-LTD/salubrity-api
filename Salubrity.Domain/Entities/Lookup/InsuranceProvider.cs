using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Lookup;

/// <summary>
/// Lookup entity for supported insurance providers.
/// </summary>
[Table("InsuranceProviders")]
public class InsuranceProvider : BaseLookupEntity
{
    /// <summary>
    /// Publicly accessible logo URL (SVG/PNG).
    /// </summary>
    [MaxLength(2048)]
    public string? LogoUrl { get; set; }

}
