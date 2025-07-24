// File: Application/Interfaces/Repositories/Lookups/ILookupRepository.cs

using Salubrity.Application.DTOs.Lookups;
using Salubrity.Domain.Common;

namespace Salubrity.Application.Interfaces.Repositories.Lookups;

public interface ILookupRepository<T> where T : BaseLookupEntity
{
    Task<List<BaseLookupResponse>> GetAllAsync();
}
