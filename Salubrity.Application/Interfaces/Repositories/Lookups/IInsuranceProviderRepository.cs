using Salubrity.Domain.Entities.Lookup;

namespace Salubrity.Application.Interfaces.Repositories.Lookups;

public interface IInsuranceProviderRepository
{
    Task<List<InsuranceProvider>> GetAllAsync();
}
