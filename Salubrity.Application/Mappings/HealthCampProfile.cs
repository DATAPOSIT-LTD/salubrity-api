// File: Salubrity.Application/Mappings/HealthCampProfile.cs

using AutoMapper;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Mappings;

public class HealthCampProfile : Profile
{
    public HealthCampProfile()
    {
        // Create DTO → Entity
        CreateMap<CreateHealthCampDto, HealthCamp>()
            .ForMember(dest => dest.PackageItems, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());

        // Entity → DTO
        CreateMap<HealthCamp, HealthCampDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.BusinessName));

        // Map package items (missing earlier)
        CreateMap<HealthCampPackageItem, HealthCampPackageItemDto>();
        CreateMap<CreateHealthCampPackageItemDto, HealthCampPackageItem>();
        CreateMap<UpdateHealthCampPackageItemDto, HealthCampPackageItem>();
    }
}
