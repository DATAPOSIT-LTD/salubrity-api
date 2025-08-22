using Salubrity.Application.DTOs.Users;

namespace Salubrity.Application.DTOs.Subcontractor
{
    public class CreateSubcontractorDto
    {
        public UserCreateRequest User { get; set; } = default!;

        public Guid? IndustryId { get; set; }

        public string? LicenseNumber { get; set; }

        public string? Bio { get; set; }

        public Guid? StatusId { get; set; }

        public List<Guid> SpecialtyIds { get; set; } = [];

        public List<Guid> SubcontractorRoleIds { get; set; } = [];

        /// <summary>
        /// One of the role IDs above, marked as primary (optional)
        /// </summary>
        public Guid? PrimaryRoleId { get; set; }
    }
}
