using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using System;
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Domain.Entities.Organizations
{
    /// <summary>
    /// Represents a client organization or company in the system.
    /// </summary>
    public class Organization : BaseAuditableEntity
    {
        [Required]
        public string BusinessName { get; set; } = null!;

        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string Phone { get; set; } = null!;

        public string? ClientLogoPath { get; set; }

        /// <summary>
        /// FK to the User that manages the organization (e.g., account owner).
        /// </summary>
        public Guid? ContactPersonId { get; set; }

        /// <summary>
        /// Optional enum or string status (e.g., active/inactive). 
        /// Suggest making it a Lookup table later for flexibility.
        /// </summary>
        public Guid? StatusId { get; set; }
        public OrganizationStatus? Status { get; set; }

    }
}
