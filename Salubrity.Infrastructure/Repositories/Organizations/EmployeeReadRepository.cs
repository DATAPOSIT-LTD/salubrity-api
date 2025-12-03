// Salubrity.Infrastructure/Repositories/Organizations/EmployeeReadRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Infrastructure.Persistence;

public class EmployeeReadRepository : IEmployeeReadRepository
{
    private readonly AppDbContext _db;
    public EmployeeReadRepository(AppDbContext db) => _db = db;

    public Task<List<Guid>> GetActiveEmployeeUserIdsAsync(Guid organizationId, CancellationToken ct = default)
    {
        // Adjust property names if yours differ (e.g., IsActive, OrganizationId, UserId)
        return _db.Employees
            .AsNoTracking()
            .Where(e => e.OrganizationId == organizationId && !e.IsDeleted && e.UserId != Guid.Empty)
            .Select(e => e.UserId!)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<Employee?> FindByUserIdAsync(Guid userId)
    {
        return await _db.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

}
