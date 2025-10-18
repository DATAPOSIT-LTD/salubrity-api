namespace Salubrity.Domain.Entities.HealthcareServices
{
    public class HealthCampPackage
    {
        public Guid Id { get; set; }
        public Guid HealthCampId { get; set; }

        // Reference to ServicePackage
        public Guid ServicePackageId { get; set; }
        public ServicePackage ServicePackage { get; set; } = default!;

        public ICollection<HealthCampPackageService> Services { get; set; } = new List<HealthCampPackageService>();
    }
}