using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Repositories.Users
{
    public interface IOnboardingStatusRepository
    {
        Task<OnboardingStatus?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<OnboardingStatus> CreateAsync(OnboardingStatus onboardingStatus, CancellationToken ct = default);
        Task<OnboardingStatus> UpdateAsync(OnboardingStatus onboardingStatus, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
