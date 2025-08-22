// File: Salubrity.Infrastructure.Repositories/Configurations/EmailConfigurationRepository.cs

using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Configurations;
using Salubrity.Infrastructure.Persistence;

public class EmailConfigurationRepository : IEmailConfigurationRepository
{
    private readonly AppDbContext _db;

    public EmailConfigurationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EmailConfiguration?> GetActiveAsync(CancellationToken ct = default)
    {
        return await _db.EmailConfigurations
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}
