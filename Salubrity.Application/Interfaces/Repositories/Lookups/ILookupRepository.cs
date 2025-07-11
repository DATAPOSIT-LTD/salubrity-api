// File: Application/Interfaces/Repositories/Lookups/ILookupRepository.cs

using Salubrity.Domain.Common;

namespace Salubrity.Application.Interfaces.Repositories.Lookups;

public interface ILookupRepository<T> where T : BaseLookupEntity
{
    Task<List<T>> GetAllAsync();
}
