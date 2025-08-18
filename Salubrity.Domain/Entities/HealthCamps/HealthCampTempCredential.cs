using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Identity;

[Table("HealthCampTempCredentials")]
public class HealthCampTempCredential : BaseAuditableEntity
{
    public Guid HealthCampId { get; set; }
    public Guid UserId { get; set; }

    [Required, MaxLength(32)]
    public string Role { get; set; } = null!; // "patient" | "subcontractor"

    public string? TempPasswordHash { get; set; }
    public DateTimeOffset? TempPasswordExpiresAt { get; set; }

    [MaxLength(64)]
    public string? SignInJti { get; set; }
    public DateTimeOffset? TokenExpiresAt { get; set; }

    public HealthCamp HealthCamp { get; set; } = null!;
    public User User { get; set; } = null!;
}
