namespace Salubrity.Application.DTOs.Subcontractor
{
    public class UpdateSubcontractorDto
    {
        public Guid? IndustryId { get; set; }

        public string? LicenseNumber { get; set; }

        public string? Bio { get; set; }

        public Guid? StatusId { get; set; }

        public List<Guid> SpecialtyIds { get; set; } = [];

        public List<Guid> RoleIds { get; set; } = [];
    }
}
