namespace Salubrity.Application.DTOs.Users
{
    public class OnboardingStatusDto
    {
        public Guid UserId { get; set; }

        public bool IsProfileComplete { get; set; }
        public bool IsRoleSpecificDataComplete { get; set; }
        public bool IsOnboardingComplete { get; set; }

        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
    }
}
