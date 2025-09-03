using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Users
{
    public class OnboardingStatusRepository : IOnboardingStatusRepository
    {
        private readonly AppDbContext _context;

        public OnboardingStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OnboardingStatus?> GetByUserIdAsync(Guid userId)
        {
            return await _context.OnboardingStatuses
                .Include(os => os.User)
                .FirstOrDefaultAsync(os => os.UserId == userId && !os.IsDeleted);
        }

        public async Task<OnboardingStatus> CreateAsync(OnboardingStatus onboardingStatus)
        {
            _context.OnboardingStatuses.Add(onboardingStatus);
            await _context.SaveChangesAsync();
            return onboardingStatus;
        }

        public async Task<OnboardingStatus> UpdateAsync(OnboardingStatus onboardingStatus)
        {
            _context.OnboardingStatuses.Update(onboardingStatus);
            await _context.SaveChangesAsync();
            return onboardingStatus;
        }

        public async Task DeleteAsync(Guid id)
        {
            var onboardingStatus = await _context.OnboardingStatuses.FindAsync(id);
            if (onboardingStatus != null)
            {
                onboardingStatus.IsDeleted = true;
                onboardingStatus.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}