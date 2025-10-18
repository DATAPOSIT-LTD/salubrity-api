namespace Salubrity.Application.DTOs.HealthcareServices
{
    public class HealthCampPackageDto
    {
        public Guid Id { get; set; }
        public Guid ServicePackageId { get; set; }
        public string ServicePackageName { get; set; } = string.Empty;
        public List<Guid> ServiceIds { get; set; } = new();
    }
}