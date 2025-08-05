using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Organizations;
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
            if (org != null)
            {
                _context.Organizations.Remove(org);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Organization?> FindByNameAsync(string name)
        {
            return await _context.Organizations
                .Where(o => !o.IsDeleted)
                .FirstOrDefaultAsync(o => o.BusinessName.ToLower() == name.ToLower());
        }



    }
}
