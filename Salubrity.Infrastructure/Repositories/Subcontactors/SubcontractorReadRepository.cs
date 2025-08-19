// Salubrity.Infrastructure/Repositories/Subcontractors/SubcontractorReadRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Infrastructure.Persistence;

public class SubcontractorReadRepository : ISubcontractorReadRepository
{
    private readonly AppDbContext _db;
    public SubcontractorReadRepository(AppDbContext db) => _db = db;

    public Task<Guid?> GetActiveIdByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        _db.Subcontractors.AsNoTracking()
            .Where(s => s.UserId == userId && s.Status.IsActive)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);
}