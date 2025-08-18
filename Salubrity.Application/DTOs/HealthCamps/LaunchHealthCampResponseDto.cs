public class LaunchHealthCampResponseDto
{
    public Guid HealthCampId { get; set; }
    public DateTime CloseDate { get; set; }
    public string ParticipantPosterQrBase64 { get; set; } = null!;
    public string SubcontractorPosterQrBase64 { get; set; } = null!;
}
