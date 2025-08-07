using AutoMapper;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Domain.Entities.Lookup;

namespace Salubrity.Application.Mappings;

public class SubcontractorProfile : Profile
{
    public SubcontractorProfile()
    {
        CreateMap<Subcontractor, SubcontractorDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.Phone))
            .ForMember(dest => dest.IndustryName, opt => opt.MapFrom(src => src.Industry.Name))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.Specialties, opt => opt.MapFrom(src => src.Specialties))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.RoleAssignments.Select(ra => ra.SubcontractorRole)))
            .ForMember(dest => dest.CampAssignmentCount, opt => opt.MapFrom(src => src.CampAssignments.Count));

        CreateMap<CreateSubcontractorDto, Subcontractor>();
        CreateMap<UpdateSubcontractorDto, Subcontractor>();

        CreateMap<SubcontractorSpecialty, SubcontractorSpecialtyDto>()
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name));

        CreateMap<SubcontractorSpecialtyDto, SubcontractorSpecialty>();

        CreateMap<SubcontractorRole, SubcontractorRoleDto>();
    }
}
