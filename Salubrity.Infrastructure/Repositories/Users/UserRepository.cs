using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> FindUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> FindUserByRefreshTokenAsync(string refreshToken)
        {
            // TODO: Replace with real refresh token handling logic
            return await Task.FromResult<User?>(null);
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeUserRefreshTokenAsync(Guid userId)
        {
            // TODO: Implement actual refresh token revocation
            await Task.CompletedTask;
        }
    }
}
