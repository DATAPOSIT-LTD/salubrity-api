using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Services.Notifications;
using Salubrity.Application.Interfaces.Services.Users;
using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Services.Users
{
    public class OnboardingService : IOnboardingService
    {
        private readonly IOnboardingStatusRepository _onboardingStatusRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISubcontractorRepository _subcontractorRepository;
        private readonly INotificationService _notificationService;

        public OnboardingService(
            IOnboardingStatusRepository onboardingStatusRepository,
            IUserRepository userRepository,
            ISubcontractorRepository subcontractorRepository,
            INotificationService notificationService)
        {
            _onboardingStatusRepository = onboardingStatusRepository;
            _userRepository = userRepository;
            _subcontractorRepository = subcontractorRepository;
            _notificationService = notificationService;
        }

        //public async Task<bool> CheckAndUpdateOnboardingStatusAsync(Guid userId, CancellationToken ct = default)
        //{
        //    var user = await _userRepository.FindUserByIdAsync(userId);
        //    if (user == null) return false;

        //    var onboardingStatus = await _onboardingStatusRepository.GetByUserIdAsync(userId, ct);
        //    if (onboardingStatus == null)
        //    {
        //        onboardingStatus = new OnboardingStatus
        //        {
        //            UserId = userId,
        //            IsProfileComplete = false,
        //            IsRoleSpecificDataComplete = false,
        //            IsOnboardingComplete = false
        //        };
        //        onboardingStatus = await _onboardingStatusRepository.CreateAsync(onboardingStatus, ct);
        //    }

        //    // Check profile and role-specific completion
        //    bool profileComplete = CheckProfileCompletion(user);
        //    bool roleSpecificComplete = await CheckRoleSpecificCompletionAsync(user, ct);
        //    bool overallComplete = profileComplete && roleSpecificComplete;

        //    // Update if status has changed
        //    if (onboardingStatus.IsProfileComplete != profileComplete ||
        //        onboardingStatus.IsRoleSpecificDataComplete != roleSpecificComplete ||
        //        onboardingStatus.IsOnboardingComplete != overallComplete)
        //    {
        //        onboardingStatus.IsProfileComplete = profileComplete;
        //        onboardingStatus.IsRoleSpecificDataComplete = roleSpecificComplete;
        //        onboardingStatus.IsOnboardingComplete = overallComplete;

        //        if (overallComplete && onboardingStatus.CompletedAt == null)
        //        {
        //            onboardingStatus.CompletedAt = DateTime.UtcNow;
        //        }

        //        await _onboardingStatusRepository.UpdateAsync(onboardingStatus, ct);

        //        if (overallComplete)
        //        {
        //            await _notificationService.TriggerNotificationAsync(
        //                title: "Onboarding Complete",
        //                message: $"User '{user.FullName}' has completed onboarding.",
        //                type: "Onboarding",
        //                entityId: user.Id,
        //                entityType: "User",
        //                ct: ct
        //            );
        //        }
        //    }

        //    return overallComplete;
        //}

        public async Task<bool> CheckAndUpdateOnboardingStatusAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userRepository.FindUserByIdAsync(userId);
            if (user == null) return false;

            var onboardingStatus = await _onboardingStatusRepository.GetByUserIdAsync(userId, ct);
            if (onboardingStatus == null)
            {
                onboardingStatus = new OnboardingStatus
                {
                    UserId = userId
                };
                onboardingStatus = await _onboardingStatusRepository.CreateAsync(onboardingStatus, ct);
            }

            bool profileComplete = CheckProfileCompletion(user);
            bool roleSpecificComplete = await CheckRoleSpecificCompletionAsync(user, ct);
            bool overallComplete = profileComplete && roleSpecificComplete;

            onboardingStatus.IsProfileComplete = profileComplete;
            onboardingStatus.IsRoleSpecificDataComplete = roleSpecificComplete;
            onboardingStatus.IsOnboardingComplete = overallComplete;

            if (overallComplete && onboardingStatus.CompletedAt == null)
            {
                onboardingStatus.CompletedAt = DateTime.UtcNow;
            }

            await _onboardingStatusRepository.UpdateAsync(onboardingStatus, ct);

            if (overallComplete)
            {
                await _notificationService.TriggerNotificationAsync(
                    title: "Onboarding Complete",
                    message: $"User '{user.FullName}' has completed onboarding.",
                    type: "Onboarding",
                    entityId: user.Id,
                    entityType: "User",
                    ct: ct
                );
            }

            return overallComplete;
        }

        public async Task<OnboardingStatus?> GetOnboardingStatusAsync(Guid userId, CancellationToken ct = default)
        {
            return await _onboardingStatusRepository.GetByUserIdAsync(userId, ct);
        }

        private static bool CheckProfileCompletion(User user)
        {
            if (string.IsNullOrWhiteSpace(user.FirstName)) Console.WriteLine("FirstName missing");
            if (string.IsNullOrWhiteSpace(user.Email)) Console.WriteLine("Email missing");
            if (string.IsNullOrWhiteSpace(user.Phone)) Console.WriteLine("Phone missing");
            if (string.IsNullOrWhiteSpace(user.NationalId)) Console.WriteLine("NationalId missing");
            if (!user.DateOfBirth.HasValue) Console.WriteLine("DateOfBirth missing");
            if (!user.GenderId.HasValue || user.GenderId.Value == Guid.Empty) Console.WriteLine("GenderId missing");
            //if (!user.OrganizationId.HasValue || user.OrganizationId.Value == Guid.Empty) Console.WriteLine("OrganizationId missing");

            return !string.IsNullOrWhiteSpace(user.FirstName) &&
                   !string.IsNullOrWhiteSpace(user.Email) &&
                   !string.IsNullOrWhiteSpace(user.Phone) &&
                   !string.IsNullOrWhiteSpace(user.NationalId) &&
                   user.DateOfBirth.HasValue &&
                   user.GenderId.HasValue && user.GenderId.Value != Guid.Empty &&
                   //user.OrganizationId.HasValue && user.OrganizationId.Value != Guid.Empty;
        }

        private async Task<bool> CheckRoleSpecificCompletionAsync(User user, CancellationToken ct)
        {
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            if (roles.Contains("Patient"))
            {
                return true; // Profile completion is sufficient for patients
            }
            else if (roles.Contains("Subcontractor") && user.RelatedEntityType == "Subcontractor")
            {
                if (user.RelatedEntityId == null) return false;

                var subcontractor = await _subcontractorRepository.GetByIdAsync(user.RelatedEntityId.Value);
                if (subcontractor == null) return false;

                var hasRoles = subcontractor.RoleAssignments?.Any() ?? false;
                var hasSpecialties = subcontractor.Specialties?.Any() ?? false;

                return hasRoles && hasSpecialties;
            }

            return true; // For other roles, assume no additional requirements
        }
    }
}
