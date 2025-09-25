
using Salubrity.Domain.Entities.HealthcareServices;

public class ServiceStationSummaryDto
{

    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<string> Staff { get; set; } = new();
    public int PatientsServed { get; set; }
    public int PendingPatients { get; set; }
    public string AverageTimePerPatient { get; set; } = default!;
    public int OutlierAlerts { get; set; }
    public PackageItemType Type { get; set; }

    // New for grouping
    public List<ServiceStationSummaryDto> Children { get; set; } = new();
}
