public class HealthCampListDto
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = default!;
    public int ExpectedPatients { get; set; }
    public string PackageName { get; set; } = default!;
    public string Venue { get; set; } = default!;
    public string DateRange { get; set; } = default!; // e.g., "8 - 10 Sep, 2020"
    public int SubcontractorCount { get; set; }
    public string Status { get; set; } = default!;   // e.g., "Ready", "Incomplete"

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? CloseDate { get; set; }
}
