public class MyQueuedParticipantDto
{
    public Guid CheckInId { get; set; }
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string Status { get; set; } = "Queued";
    public int Priority { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
