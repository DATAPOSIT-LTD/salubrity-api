using System;
using System.Threading.Tasks;
using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User?> FindUserByEmailAsync(string email);
        Task<User?> FindUserByIdAsync(Guid userId);
        Task<User?> FindUserByRefreshTokenAsync(string refreshToken);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task RevokeUserRefreshTokenAsync(Guid userId);
    }
}
