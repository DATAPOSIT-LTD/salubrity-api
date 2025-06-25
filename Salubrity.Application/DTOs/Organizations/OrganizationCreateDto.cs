using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Organizations
{
    public class OrganizationCreateDto
    {
        [Required]
        public string BusinessName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, Phone]
        public string Phone { get; set; } = null!;

        public string? ClientLogoPath { get; set; }

        public Guid? ContactPersonId { get; set; }

        public Guid? StatusId { get; set; }
    }
}
