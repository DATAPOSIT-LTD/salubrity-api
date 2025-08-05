using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Domain.Entities.Rbac;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Identity;

/// <summary>
/// Core user entity supporting polymorphism, multitenancy, audit, and RBAC.
/// </summary>
public class User : BaseAuditableEntity
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    [Required]
    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? NationalId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? PrimaryLanguage { get; set; }

    public string? ProfileImage { get; set; }

    // FK to Gender lookup table
    public Guid? GenderId { get; set; }
    public Gender? Gender { get; set; }

    // Multitenancy
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    // Polymorphic relationship
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; } // e.g., "Patient", "Subcontractor", etc.

    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;

    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastPasswordChangeAt { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = [];

    [NotMapped]
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ");

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }


    public string? TotpSecret { get; set; }
    public ICollection<UserLanguage> UserLanguages { get; set; } = [];

}
