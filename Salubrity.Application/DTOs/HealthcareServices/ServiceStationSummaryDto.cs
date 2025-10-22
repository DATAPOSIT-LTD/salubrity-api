
using Salubrity.Domain.Entities.HealthcareServices;


public class ServiceStationSummaryDto
{
    public Guid Id { get; set; }
    public PackageItemType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? PackageId { get; set; }
    public string? PackageName { get; set; }
    public List<string> Staff { get; set; } = new();
    public int PatientsServed { get; set; }
    public int PendingPatients { get; set; }
    public string AverageTimePerPatient { get; set; } = "0 min";
    public int OutlierAlerts { get; set; }
    public List<ServiceStationSummaryDto> Children { get; set; } = new();
}

