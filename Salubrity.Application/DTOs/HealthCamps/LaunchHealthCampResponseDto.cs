public class LaunchHealthCampResponseDto
{
    public Guid HealthCampId { get; set; }
    public DateTime CloseDate { get; set; }
    public string? ParticipantPosterQrUrl { get; set; }
    public string? SubcontractorPosterQrUrl { get; set; }
}
