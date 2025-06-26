using AutoMapper;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Services.Lookups;
using Salubrity.Shared.Responses;

namespace Salubrity.Application.Services.Lookups;

public class InsuranceProviderService : IInsuranceProviderService
{
    private readonly IInsuranceProviderRepository _repository;
    private readonly IMapper _mapper;

    public InsuranceProviderService(IInsuranceProviderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<InsuranceProviderResponse>>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        var dto = _mapper.Map<List<InsuranceProviderResponse>>(entities);
        return ApiResponse<List<InsuranceProviderResponse>>.CreateSuccess(dto);
    }
}
