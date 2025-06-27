using AutoMapper;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Mappings;

public class IndustryProfile : Profile
{
    public IndustryProfile()
    {
        CreateMap<Industry, IndustryResponseDto>();
        CreateMap<CreateIndustryDto, Industry>();
    }
}
