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
        // DTO -> Entity
        CreateMap<CreateHealthCampDto, HealthCamp>()
            .ForMember(dest => dest.PackageItems, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());

        CreateMap<CreateHealthCampPackageItemDto, HealthCampPackageItem>();
        CreateMap<UpdateHealthCampPackageItemDto, HealthCampPackageItem>();

        // Entity -> Child DTOs (MISSING ONE WAS HERE)
        CreateMap<HealthCampPackageItem, HealthCampPackageItemDto>();

        //  Add this:
        CreateMap<HealthCampServiceAssignment, HealthCampServiceAssignmentDto>()
            // If your DTO only has Ids, keep just the two lines below
            .ForMember(d => d.ServiceId, m => m.MapFrom(s => s.ServiceId))
            .ForMember(d => d.SubcontractorId, m => m.MapFrom(s => s.SubcontractorId))
            // If your DTO exposes names, keep these; otherwise remove them
            .ForMember(d => d.ServiceName, m => m.MapFrom(s => s.Service != null ? s.Service.Name : null))
            .ForMember(d => d.SubcontractorName, m => m.MapFrom(s => s.Subcontractor != null ? s.Subcontractor.User.FullName : null));

        // Entity -> Generic DTO
        CreateMap<HealthCamp, HealthCampDto>()
            .ForMember(dest => dest.OrganizationName, m => m.MapFrom(src => src.Organization.BusinessName))
            // Be explicit so AutoMapper uses the maps above for the child collections
            .ForMember(dest => dest.PackageItems, m => m.MapFrom(src => src.PackageItems))
            .ForMember(dest => dest.ServiceAssignments, m => m.MapFrom(src => src.ServiceAssignments));

        // Entity -> List/Detail DTOs (unchanged)
        CreateMap<HealthCamp, HealthCampListDto>()
            .ForMember(dest => dest.ClientName, m => m.MapFrom(src => src.Organization.BusinessName))
            .ForMember(dest => dest.ExpectedPatients, m => m.MapFrom(src => 0))
            .ForMember(dest => dest.Venue, m => m.MapFrom(src => src.Location))
            .ForMember(dest => dest.DateRange, m => m.MapFrom(src => $"{src.StartDate:dd} - {src.EndDate:dd MMM, yyyy}"))
            .ForMember(dest => dest.SubcontractorCount, m => m.MapFrom(src => src.ServiceAssignments.Count))
            .ForMember(dest => dest.Status, m => m.MapFrom(src => src.HealthCampStatus.Name))
            .ForMember(dest => dest.PackageName, m => m.Ignore());

        CreateMap<HealthCamp, HealthCampDetailDto>()
            .ForMember(dest => dest.ClientName, m => m.MapFrom(src => src.Organization.BusinessName))
            .ForMember(dest => dest.Venue, m => m.MapFrom(src => src.Location))
            .ForMember(dest => dest.SubcontractorCount, m => m.MapFrom(src => src.ServiceAssignments.Count))
            .ForMember(dest => dest.PackageName, m => m.Ignore())
            .ForMember(dest => dest.PackageCost, m => m.Ignore())
            .ForMember(dest => dest.InsurerName, m => m.Ignore())
            .ForMember(dest => dest.ServiceStations, m => m.Ignore());
    }
}
