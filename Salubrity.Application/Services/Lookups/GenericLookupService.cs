// File: Application/Services/Lookups/GenericLookupService.cs

using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Services.Lookups;
using Salubrity.Domain.Common;

public class GenericLookupService<T> : ILookupService where T : BaseLookupEntity
{
    private readonly ILookupRepository<T> _repository;

    public GenericLookupService(ILookupRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task<List<BaseLookupResponse>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<BaseLookupResponse?> FindByNameAsync(string name)
    {
        var entity = await _repository.FindByNameAsync(name);
        return entity == null ? null : new BaseLookupResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }
}
