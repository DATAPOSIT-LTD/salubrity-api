namespace Salubrity.Application.DTOs.HealthcareServices
{
    public class UpdateHealthCampPackageDto
    {
        public Guid ServicePackageId { get; set; }
        public List<Guid> ServiceIds { get; set; } = new();
    }
}
