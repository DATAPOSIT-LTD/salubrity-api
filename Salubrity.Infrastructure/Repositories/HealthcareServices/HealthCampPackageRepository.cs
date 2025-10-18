using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Infrastructure.Repositories.HealthcareServices
{
    public class HealthCampPackageRepository : IHealthCampPackageRepository
    {
        private readonly AppDbContext _context;

        public HealthCampPackageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCampPackage> CreateAsync(Guid campId, CreateHealthCampPackageDto dto)
        {
            var package = new HealthCampPackage
            {
                Id = Guid.NewGuid(),
                HealthCampId = campId,
                ServicePackageId = dto.ServicePackageId,
                Services = dto.ServiceIds.Select(sid => new HealthCampPackageService
                {
                    Id = Guid.NewGuid(),
                    ServiceId = sid
                }).ToList()
            };

            _context.HealthCampPackages.Add(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<List<HealthCampPackage>> GetByCampIdAsync(Guid campId)
        {
            return await _context.HealthCampPackages
                .Where(p => p.HealthCampId == campId)
                .Include(p => p.Services)
                .Include(p => p.ServicePackage)
                .ToListAsync();
        }

        public async Task AssignPackageAsync(Guid participantId, Guid packageId)
        {
            var participant = await _context.HealthCampParticipants.FindAsync(participantId);
            if (participant == null) throw new NotFoundException("Participant not found");
            participant.HealthCampPackageId = packageId;
            await _context.SaveChangesAsync();
        }

        public async Task<HealthCampPackage> UpdateAsync(Guid packageId, UpdateHealthCampPackageDto dto)
        {
            var package = await _context.HealthCampPackages
                .Include(p => p.Services)
                .FirstOrDefaultAsync(p => p.Id == packageId);

            if (package == null) throw new Exception("Package not found");

            package.ServicePackageId = dto.ServicePackageId;

            // Remove old services
            package.Services.Clear();

            // Add new services
            foreach (var sid in dto.ServiceIds)
            {
                package.Services.Add(new HealthCampPackageService
                {
                    Id = Guid.NewGuid(),
                    ServiceId = sid,
                    HealthCampPackageId = packageId
                });
            }

            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<List<Guid>> GetServiceIdsByCampIdAsync(Guid campId)
        {
            return await _context.HealthCampPackages
                .Where(p => p.HealthCampId == campId)
                .SelectMany(p => p.Services)
                .Select(s => s.ServiceId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<AllocatedServiceDto>> GetAllocatedServicesByCampIdAsync(Guid campId)
        {
            return await (from pkg in _context.HealthCampPackages
                          where pkg.HealthCampId == campId
                          from svc in pkg.Services
                          join s in _context.Services on svc.ServiceId equals s.Id
                          select new AllocatedServiceDto
                          {
                              ServiceId = s.Id,
                              ServiceName = s.Name
                          })
                          .Distinct()
                          .ToListAsync();
        }
    }
}