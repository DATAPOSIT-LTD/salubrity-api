public class ServiceStationSummaryDto
{
    public string Name { get; set; } = default!;
    public List<string> Staff { get; set; } = new();
    public int PatientsServed { get; set; }
    public int PendingPatients { get; set; }
    public string AverageTimePerPatient { get; set; } = default!;
    public int OutlierAlerts { get; set; }
}
