public class ServiceStationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<string> AssignedStaff { get; set; } = []; // Names
    public int PatientsServed { get; set; }
    public int PendingService { get; set; }
    public string AvgTimePerPatient { get; set; } = "3 min";
    public int OutlierAlerts { get; set; } = 0;
}