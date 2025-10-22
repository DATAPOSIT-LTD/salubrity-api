// File: Salubrity.Application/Mappings/HealthCampProfile.cs
using AutoMapper;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Join;

namespace Salubrity.Application.Mappings;

public class HealthCampProfile : Profile
{
    public HealthCampProfile()
    {
        // DTO -> Entity
        // CreateMap<CreateHealthCampDto, HealthCamp>()
        //     .ForMember(dest => dest.PackageItems, opt => opt.Ignore())
        //     .ForMember(dest => dest.ServiceAssignments, opt => opt.Ignore())
        //     .ForMember(dest => dest.Organization, opt => opt.Ignore());


        CreateMap<CreateHealthCampDto, HealthCamp>()
            .ForMember(dest => dest.HealthCampPackages, opt => opt.Ignore())
            .ForMember(dest => dest.PackageItems, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());


        CreateMap<CreateHealthCampPackageItemDto, HealthCampPackageItem>();
        // NEW: Package-level mappings
        CreateMap<CreateCampPackageDto, HealthCampPackage>();
        CreateMap<HealthCampPackage, HealthCampPackageDto>()
            .ForMember(dest => dest.ServicePackageName, opt => opt.MapFrom(src => src.ServicePackage!.Name));

        CreateMap<UpdateHealthCampPackageItemDto, HealthCampPackageItem>();

        // Entity -> Child DTOs (MISSING ONE WAS HERE)
        CreateMap<HealthCampPackageItem, HealthCampPackageItemDto>();

        //  Add this:
        CreateMap<HealthCampServiceAssignment, HealthCampServiceAssignmentDto>()
             .ForMember(d => d.AssignmentId, m => m.MapFrom(s => s.AssignmentId))
             .ForMember(d => d.AssignmentType, m => m.MapFrom(s => s.AssignmentType))
             .ForMember(d => d.SubcontractorId, m => m.MapFrom(s => s.SubcontractorId))
             .ForMember(d => d.ProfessionId, m => m.MapFrom(s => s.ProfessionId))
             .ForMember(d => d.SubcontractorName, m => m.MapFrom(s => s.Subcontractor.User.FullName))
             .ForMember(d => d.AssignmentName, m => m.MapFrom<AssignmentNameResolver>());


        // Entity -> Generic DTO
        // CreateMap<HealthCamp, HealthCampDto>()
        //     // .ForMember(dest => dest.OrganizationName, m => m.MapFrom(src => src.Organization.BusinessName))
        //     .ForMember(dest => dest.ClientName, m => m.MapFrom(src => src.Organization.BusinessName))
        //     // Be explicit so AutoMapper uses the maps above for the child collections
        //     .ForMember(dest => dest.PackageItems, m => m.MapFrom(src => src.PackageItems))
        //     .ForMember(dest => dest.ServiceAssignments, m => m.MapFrom(src => src.ServiceAssignments));

        CreateMap<HealthCamp, HealthCampDto>()
            .ForMember(dest => dest.ClientName, m => m.MapFrom(src => src.Organization.BusinessName))
            .ForMember(dest => dest.Packages, m => m.MapFrom(src => src.HealthCampPackages))
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

        CreateMap<HealthCampParticipant, HealthCampParticipantDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.User.MiddleName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.Phone))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender.Name))

            .ForMember(dest => dest.CampId, opt => opt.MapFrom(src => src.HealthCampId))
            .ForMember(dest => dest.CampName, opt => opt.MapFrom(src => src.HealthCamp.Name))
            .ForMember(dest => dest.CampDate, opt => opt.MapFrom(src => src.HealthCamp.StartDate))

            // .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.IsEmployee, opt => opt.MapFrom(src => src.IsEmployee))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))

            // You can override these from reporting logic later
            .ForMember(dest => dest.IncompleteReports, opt => opt.Ignore())
            .ForMember(dest => dest.ActionCount, opt => opt.Ignore())
            .ForMember(dest => dest.ClaimCount, opt => opt.Ignore())
            .ForMember(dest => dest.ClaimStatus, opt => opt.Ignore());

        // HealthCampPackage mappings
        CreateMap<HealthCampPackage, HealthCampPackageDto>()
            .ForMember(dest => dest.ServicePackageName, opt => opt.MapFrom(src => src.ServicePackage.Name))
            .ForMember(dest => dest.ServiceIds, opt => opt.MapFrom(src => src.Services.Select(s => s.ServiceId).ToList()));

        CreateMap<HealthCampPackageService, Guid>()
            .ConvertUsing(src => src.ServiceId);

    }
}
