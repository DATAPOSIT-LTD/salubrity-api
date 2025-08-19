// Salubrity.Application/DTOs/HealthCamps/CampParticipantListDto.cs
namespace Salubrity.Application.DTOs.HealthCamps;

public class CampParticipantListDto
{
    public Guid Id { get; set; }               // HealthCampParticipant Id
    public Guid UserId { get; set; }
    public Guid? PatientId { get; set; }
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string CompanyName { get; set; } = default!; // camp's Organization
    public bool Served { get; set; }
    public DateTime? ParticipatedAt { get; set; }
}
