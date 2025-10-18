using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthCamps
{
    public class HealthCampPackageRepository : IHealthCampPackageRepository
    {
        private readonly AppDbContext _context;

        public HealthCampPackageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCampPackage?> GetPackageByCampAsync(Guid campId, Guid packageId, CancellationToken ct = default)
        {
            return await _context.HealthCampPackages
                .FirstOrDefaultAsync(p => p.HealthCampId == campId && p.Id == packageId && p.IsActive, ct);
        }


        public async Task<IReadOnlyList<HealthCampPackage>> GetAllPackagesWithServicesByCampAsync(
            Guid campId, CancellationToken ct = default)
        {
            var packages = await _context.HealthCampPackages
                .Where(p => p.HealthCampId == campId && p.IsActive)
                .Include(p => p.ServicePackage)
                    .ThenInclude(sp => sp.Items)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync(ct);

            return packages;
        }



    }
}
