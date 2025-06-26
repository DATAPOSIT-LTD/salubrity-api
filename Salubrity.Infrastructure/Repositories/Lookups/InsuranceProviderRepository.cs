using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Lookups;

public class InsuranceProviderRepository : IInsuranceProviderRepository
{
    private readonly AppDbContext _db;

    public InsuranceProviderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<InsuranceProvider>> GetAllAsync()
    {
        return await _db.InsuranceProviders
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}
