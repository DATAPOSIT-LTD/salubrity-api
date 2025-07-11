namespace Salubrity.Application.DTOs.HealthCamps
{
    public class HealthCampParticipantDto
    {
        // User Info
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();

        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Gender { get; set; }

        // Camp Info
        public Guid CampId { get; set; }
        public string CampName { get; set; } = null!;
        public DateTime CampDate { get; set; }
        public string? Status { get; set; }

        // Participation Metadata
        public bool IsEmployee { get; set; }
        public string? Notes { get; set; }

        // Optional Aggregates
        public int IncompleteReports { get; set; }
        public int ActionCount { get; set; }
        public int ClaimCount { get; set; }
        public string ClaimStatus { get; set; } = "Unpaid";
    }
}
