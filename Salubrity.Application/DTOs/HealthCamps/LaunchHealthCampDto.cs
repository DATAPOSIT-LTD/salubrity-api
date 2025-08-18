using System.Text.Json.Serialization;

public class LaunchHealthCampDto
{
    [JsonIgnore] // This prevents it from being bound frdrom the request body
    public Guid HealthCampId { get; set; }

    public DateTime CloseDate { get; set; }
}
