using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Organizations
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _context;

        public OrganizationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Organization> CreateAsync(Organization entity)
        {
            _context.Organizations.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<Organization>> GetAllAsync()
        {
            return await _context.Organizations.ToListAsync();
        }

        public async Task<Organization?> GetByIdAsync(Guid id)
        {
            return await _context.Organizations.FindAsync(id);
        }

        public async Task UpdateAsync(Organization entity)
        {
            _context.Organizations.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return;

            var now = DateTime.UtcNow;

            await _context.Employees
                .Where(e => e.OrganizationId == id && !e.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.IsDeleted, true)
                    .SetProperty(e => e.DeletedAt, now));

            await _context.Departments
                .Where(d => d.OrganizationId == id && !d.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(d => d.IsDeleted, true)
                    .SetProperty(d => d.DeletedAt, now));

            await _context.OrganizationBranches
                .Where(b => b.OrganizationId == id && !b.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.IsDeleted, true)
                    .SetProperty(b => b.DeletedAt, now));

            await _context.OrganizationInsuranceProviders
                .Where(p => p.OrganizationId == id && !p.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.IsDeleted, true)
                    .SetProperty(p => p.DeletedAt, now));

            await _context.Set<OrganizationPackage>()
                .Where(p => p.OrganizationId == id && !p.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.IsDeleted, true)
                    .SetProperty(p => p.DeletedAt, now));

            org.IsDeleted = true;
            org.DeletedAt = now;
            await _context.SaveChangesAsync();
        }

        public async Task<Organization?> FindByNameAsync(string name)
        {
            return await _context.Organizations
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.BusinessName.ToLower() == name.ToLower());
        }



    }
}
