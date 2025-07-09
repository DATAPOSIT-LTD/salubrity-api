public class CampPatientSummaryDto
{
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string RiskStatus { get; set; } = default!;
    public int Outliers { get; set; }
    public int Progress { get; set; }
}
