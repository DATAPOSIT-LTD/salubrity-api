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
        // DTO → Entity
        CreateMap<CreateHealthCampDto, HealthCamp>()
            .ForMember(dest => dest.PackageItems, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());

        CreateMap<CreateHealthCampPackageItemDto, HealthCampPackageItem>();
        CreateMap<UpdateHealthCampPackageItemDto, HealthCampPackageItem>();

        // Entity → Generic DTO
        CreateMap<HealthCamp, HealthCampDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.BusinessName));

        CreateMap<HealthCampPackageItem, HealthCampPackageItemDto>();

        // Entity → List DTO
        CreateMap<HealthCamp, HealthCampListDto>()
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Organization.BusinessName))
            .ForMember(dest => dest.ExpectedPatients, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Venue, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.DateRange, opt => opt.MapFrom(src =>
                $"{src.StartDate:dd} - {src.EndDate:dd MMM, yyyy}"))
            .ForMember(dest => dest.SubcontractorCount, opt => opt.MapFrom(src =>
                src.ServiceAssignments.Count))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.ServiceAssignments.Any() ? "Ready" : "Incomplete"))
            // You’ll resolve the actual package name manually in service
            .ForMember(dest => dest.PackageName, opt => opt.Ignore());

        // Entity → Detail DTO
        CreateMap<HealthCamp, HealthCampDetailDto>()
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Organization.BusinessName))
            .ForMember(dest => dest.Venue, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.SubcontractorCount, opt => opt.MapFrom(src =>
                src.ServiceAssignments.Count))
            // These three require post-mapping logic with a resolver
            .ForMember(dest => dest.PackageName, opt => opt.Ignore())
            .ForMember(dest => dest.PackageCost, opt => opt.Ignore())
            .ForMember(dest => dest.InsurerName, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceStations, opt => opt.Ignore());
    }
}
