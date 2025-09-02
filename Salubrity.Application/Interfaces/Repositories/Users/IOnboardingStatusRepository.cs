using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Repositories.Users
{
    public interface IOnboardingStatusRepository
    {
        Task<OnboardingStatus?> GetByUserIdAsync(Guid userId);
        Task<OnboardingStatus> CreateAsync(OnboardingStatus onboardingStatus);
        Task<OnboardingStatus> UpdateAsync(OnboardingStatus onboardingStatus);
        Task DeleteAsync(Guid id);
    }
}
