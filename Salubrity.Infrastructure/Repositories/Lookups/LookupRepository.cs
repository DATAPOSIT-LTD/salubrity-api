// File: Infrastructure/Persistence/Repositories/Lookups/LookupRepository.cs

using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Domain.Common;
using Salubrity.Infrastructure.Persistence;

public class LookupRepository<T> : ILookupRepository<T> where T : BaseLookupEntity
{
    private readonly AppDbContext _context;

    public LookupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().AsNoTracking().ToListAsync();
    }
}
