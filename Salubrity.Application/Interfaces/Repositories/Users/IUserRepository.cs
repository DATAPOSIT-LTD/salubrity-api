using Salubrity.Application.DTOs.Users;
using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User?> FindUserByEmailAsync(string email);
        Task<User?> FindUserByIdAsync(Guid userId);
        Task<User?> FindUserByRefreshTokenAsync(string refreshToken);
        Task<User?> FindUserByPhoneAsync(string phone); // 🔎 NEW METHOD
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task RevokeUserRefreshTokenAsync(Guid userId);
        Task<List<UserListItemResponse>> GetAllUsersAsync(CancellationToken ct);

        Task DeleteUserAsync(Guid userId);

        Task<int> BackfillSubcontractorLinksAsync(CancellationToken ct = default);
        Task<int> BackfillPatientLinksAsync(CancellationToken ct = default);

    }
}
