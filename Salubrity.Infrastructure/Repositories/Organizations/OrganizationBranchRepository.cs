using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Organizations;

public class OrganizationBranchRepository : IOrganizationBranchRepository
{
    private readonly AppDbContext _db;

    public OrganizationBranchRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OrganizationBranch> CreateAsync(OrganizationBranch branch)
    {
        _db.OrganizationBranches.Add(branch);
        await _db.SaveChangesAsync();
        return branch;
    }

    public async Task<List<OrganizationBranch>> GetByOrganizationAsync(Guid organizationId)
    {
        return await _db.OrganizationBranches
            .Where(b => b.OrganizationId == organizationId)
            .OrderBy(b => b.BranchName)
            .ToListAsync();
    }

    public async Task<OrganizationBranch?> GetByIdAsync(Guid id)
    {
        return await _db.OrganizationBranches
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(OrganizationBranch branch)
    {
        _db.OrganizationBranches.Update(branch);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.OrganizationBranches.FindAsync(id);
        if (entity != null)
        {
            _db.OrganizationBranches.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
