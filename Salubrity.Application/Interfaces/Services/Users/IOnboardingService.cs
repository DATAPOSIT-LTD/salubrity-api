using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Services.Users
{
    public interface IOnboardingService
    {
        Task<bool> CheckAndUpdateOnboardingStatusAsync(Guid userId);
        Task<OnboardingStatus?> GetOnboardingStatusAsync(Guid userId);
    }
}