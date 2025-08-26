using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Domain.Entities.Join;

namespace Salubrity.Infrastructure.Repositories
{
    public class HealthCampParticipantRepository : IHealthCampParticipantRepository
    {
        private readonly AppDbContext _context;

        public HealthCampParticipantRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<Guid?> GetPatientIdByParticipantIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Patients
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(ct);
        }

    }
}
