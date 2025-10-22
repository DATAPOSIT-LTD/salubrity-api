namespace Salubrity.Application.DTOs.HealthCamps
{
    public class CreateCampPackageDto
    {
        public Guid PackageId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public List<CreatePackageItemDto> PackageItems { get; set; } = [];
        public List<CreateServiceAssignmentDto> ServiceAssignments { get; set; } = [];
    }

    public class CreatePackageItemDto
    {
        public Guid ReferenceId { get; set; }
    }

    public class CreateServiceAssignmentDto
    {
        public Guid ServiceId { get; set; }
        public Guid SubcontractorId { get; set; }
        public Guid ProfessionId { get; set; }
    }
}
