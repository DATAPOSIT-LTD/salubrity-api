using System.ComponentModel.DataAnnotations;
using Salubrity.Domain.Common;

namespace Salubrity.Domain.Common
{
    /// <summary>
    /// Base class for all lookup entities with name and description.
    /// </summary>
    public abstract class BaseLookupEntity : BaseAuditableEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
