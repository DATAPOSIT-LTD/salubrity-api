using Salubrity.Application.DTOs.Users;

namespace Salubrity.Application.DTOs.Subcontractor
{
    //public class UpdateSubcontractorDto
    //{
    //    public Guid? IndustryId { get; set; }

    //    public string? LicenseNumber { get; set; }

    //    public string? Bio { get; set; }

    //    public Guid? StatusId { get; set; }

    //    public List<Guid> SpecialtyIds { get; set; } = [];

    //    public List<Guid> RoleIds { get; set; } = [];
    //}

    public class UpdateSubcontractorDto
    {
        public UserUpdateRequest? User { get; set; }

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

    //public class UserUpdateRequest
    //{
    //    public Guid Id { get; set; }
    //    public string? FirstName { get; set; }
    //    public string? MiddleName { get; set; }
    //    public string? LastName { get; set; }
    //    public string? Email { get; set; }
    //    public string? Phone { get; set; }
    //    public Guid? GenderId { get; set; }
    //    public DateTime? DateOfBirth { get; set; }
    //}
}
