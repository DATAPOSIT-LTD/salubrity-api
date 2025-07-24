// File: Infrastructure/Persistence/Repositories/Lookups/LookupRepository.cs

using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Domain.Common;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Lookups
{
    public class LookupRepository<T> : ILookupRepository<T> where T : BaseLookupEntity
    {
        private readonly AppDbContext _context;

        public LookupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BaseLookupResponse>> GetAllAsync()
        {
            var entities = await _context.Set<T>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            return entities.Select(x => new BaseLookupResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            }).ToList();
        }
    }
}
