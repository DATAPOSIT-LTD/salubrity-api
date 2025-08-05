// File: Infrastructure/Repositories/Common/EfLookupRepository.cs

using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Domain.Common;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Common;

public class EfLookupRepository<T> : ILookupRepository<T> where T : BaseLookupEntity
{
    private readonly AppDbContext _context;

    public EfLookupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BaseLookupResponse>> GetAllAsync()
    {
        var entities = await _context.Set<T>().ToListAsync();

        return entities.Select(x => new BaseLookupResponse
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        }).ToList();
    }

    public async Task<T?> FindByNameAsync(string name)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
    }
}
