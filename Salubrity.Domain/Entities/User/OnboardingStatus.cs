using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Identity
{
    public class OnboardingStatus : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public bool IsProfileComplete { get; set; }
        public bool IsRoleSpecificDataComplete { get; set; }
        public bool IsOnboardingComplete { get; set; }

        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
    }
}
