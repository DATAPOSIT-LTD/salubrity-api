using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories
{
    public class SubcontractorRepository : ISubcontractorRepository
    {
        private readonly AppDbContext _db;

        public SubcontractorRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Subcontractor entity)
        {
            await _db.Subcontractors.AddAsync(entity);
        }
        public async Task<Subcontractor?> GetByIdAsync(Guid id)
        {
            return await _db.Subcontractors
                .Include(s => s.User)
                .Include(s => s.Status)
                .Include(s => s.Industry)
                .Include(s => s.Specialties)
                    .ThenInclude(ss => ss.Service)
                .Include(s => s.RoleAssignments)
                    .ThenInclude(ra => ra.SubcontractorRole)
                .Include(s => s.CampAssignments)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<List<Subcontractor>> GetAllWithDetailsAsync()
        {
            return await _db.Subcontractors
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .Include(s => s.Status)
                .Include(s => s.Industry)
                .Include(s => s.Specialties)
                    .ThenInclude(ss => ss.Service)
                .Include(s => s.RoleAssignments)
                    .ThenInclude(ra => ra.SubcontractorRole)
                .Include(s => s.CampAssignments)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Subcontractor?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _db.Subcontractors
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .Include(s => s.Status)
                .Include(s => s.Industry)
                .Include(s => s.Specialties)
                    .ThenInclude(ss => ss.Service)
                .Include(s => s.RoleAssignments)
                    .ThenInclude(ra => ra.SubcontractorRole)
                .Include(s => s.CampAssignments)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AssignRoleAsync(Guid subcontractorId, Guid roleId, bool isPrimary, CancellationToken ct = default)
        {
            var exists = await _db.SubcontractorRoleAssignments
                .AnyAsync(r => r.SubcontractorId == subcontractorId && r.SubcontractorRoleId == roleId, ct);

            if (!exists)
            {
                if (isPrimary)
                {
                    // Unset other primary roles
                    var existingPrimaries = await _db.SubcontractorRoleAssignments
                        .Where(r => r.SubcontractorId == subcontractorId && r.IsPrimary)
                        .ToListAsync(ct);

                    foreach (var r in existingPrimaries)
                        r.IsPrimary = false;
                }

                var assignment = new SubcontractorRoleAssignment
                {
                    SubcontractorId = subcontractorId,
                    SubcontractorRoleId = roleId,
                    IsPrimary = isPrimary
                };

                await _db.SubcontractorRoleAssignments.AddAsync(assignment, ct);
            }

            await _db.SaveChangesAsync(ct);
        }


        public async Task AssignSpecialtyAsync(Guid subcontractorId, Guid serviceId)
        {
            var exists = await _db.SubcontractorSpecialties
                .AnyAsync(s => s.SubcontractorId == subcontractorId && s.ServiceId == serviceId);

            if (!exists)
            {
                var specialty = new SubcontractorSpecialty
                {
                    SubcontractorId = subcontractorId,
                    ServiceId = serviceId
                };

                await _db.SubcontractorSpecialties.AddAsync(specialty);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }



        public async Task UpdateSubAsync(Subcontractor subcontractor, CancellationToken ct = default)
        {
            _db.Subcontractors.Update(subcontractor);
            await _db.SaveChangesAsync(ct);
        }
        public async Task<Subcontractor?> GetByUserIdAsync(Guid userId)
        {
            return await _db.Subcontractors
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        public async Task<Subcontractor?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _db.Subcontractors
                .Include(s => s.User)
                .Include(s => s.Specialties)
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);
        }


        //Fixes for update subcontractor

        public async Task RemoveAllSpecialtiesAsync(Guid subcontractorId, CancellationToken ct = default)
        {
            var specialties = await _db.SubcontractorSpecialties
                .Where(s => s.SubcontractorId == subcontractorId)
                .ToListAsync(ct);

            _db.SubcontractorSpecialties.RemoveRange(specialties);
        }

        public async Task RemoveAllRoleAssignmentsAsync(Guid subcontractorId, CancellationToken ct = default)
        {
            var roleAssignments = await _db.SubcontractorRoleAssignments
                .Where(r => r.SubcontractorId == subcontractorId)
                .ToListAsync(ct);

            _db.SubcontractorRoleAssignments.RemoveRange(roleAssignments);
        }

        public async Task AddSpecialtiesAsync(Guid subcontractorId, List<Guid> serviceIds, CancellationToken ct = default)
        {
            var specialties = serviceIds.Select(serviceId => new SubcontractorSpecialty
            {
                SubcontractorId = subcontractorId,
                ServiceId = serviceId
            }).ToList();

            await _db.SubcontractorSpecialties.AddRangeAsync(specialties, ct);
        }

    }
}
