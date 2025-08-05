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
        var items = await _repository.GetAllAsync();
        return [.. items.Select(x => new BaseLookupResponse
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        })];
    }
}
