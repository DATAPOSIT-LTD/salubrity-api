// File: Application/DTOs/HealthCamps/PublishFinalReportsResultDto.cs
namespace Salubrity.Application.DTOs.HealthCamps;

public class PublishFinalReportsResultDto
{
    public DateTime PublishedAt { get; set; }
    public Guid PublishedById { get; set; }
    public string PublishedByName { get; set; } = string.Empty;
    public int RecipientCount { get; set; }
    public int EmailsSent { get; set; }
}
