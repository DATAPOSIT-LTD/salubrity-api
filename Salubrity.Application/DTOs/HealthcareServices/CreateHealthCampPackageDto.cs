namespace Salubrity.Application.DTOs.HealthcareServices
{
    public class CreateHealthCampPackageDto
    {
        public Guid ServicePackageId { get; set; }
        public List<Guid> ServiceIds { get; set; } = new();
    }

    public class CreateHealthCampPackagesDto
    {
        public List<CreateHealthCampPackageDto> Packages { get; set; } = new();
    }
}