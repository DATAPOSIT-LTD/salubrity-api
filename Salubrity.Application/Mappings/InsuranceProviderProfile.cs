using AutoMapper;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Domain.Entities.Lookup;

namespace Salubrity.Application.Mappings;

public class InsuranceProviderProfile : Profile
{
    public InsuranceProviderProfile()
    {
        CreateMap<InsuranceProvider, InsuranceProviderResponse>();
    }
}
