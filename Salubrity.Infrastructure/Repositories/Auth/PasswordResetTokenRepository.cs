using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Auth;
using Salubrity.Domain.Entities.Auth;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Auth;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _db;
    public PasswordResetTokenRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PasswordResetToken?> FindActiveByTokenAsync(string token, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _db.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.Token == token && !t.IsUsed && !t.IsDeleted && t.ExpiresAt > now)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task MarkUsedAsync(Guid id, CancellationToken ct = default)
    {
        var token = await _db.PasswordResetTokens.FindAsync([id], ct);
        if (token == null) return;
        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
