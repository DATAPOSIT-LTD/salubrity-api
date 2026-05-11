using Salubrity.Domain.Entities.Auth;

namespace Salubrity.Application.Interfaces.Repositories.Auth;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token, CancellationToken ct = default);
    Task<PasswordResetToken?> FindActiveByTokenAsync(string token, CancellationToken ct = default);
    Task MarkUsedAsync(Guid id, CancellationToken ct = default);
}
