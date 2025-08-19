using AutoMapper;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Domain.Entities.Lookup;

namespace Salubrity.Application.Mappings;

public class SubcontractorProfile : Profile
{
    public SubcontractorProfile()
    {
        // Subcontractor -> DTO
        CreateMap<Subcontractor, SubcontractorDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.User.Phone)) // change to .PhoneNumber if your User has that prop
            .ForMember(d => d.IndustryName, o => o.MapFrom(s => s.Industry.Name))
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.Status.Name))
            .ForMember(d => d.Specialties, o => o.MapFrom(s => s.Specialties))
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.RoleAssignments))  // uses assignment -> dto map
            .ForMember(d => d.CampAssignmentCount, o => o.MapFrom(s => s.CampAssignments.Count));

        // Create/Update
        CreateMap<CreateSubcontractorDto, Subcontractor>();
        CreateMap<UpdateSubcontractorDto, Subcontractor>();

        // Specialties
        CreateMap<SubcontractorSpecialty, SubcontractorSpecialtyDto>()
            .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service.Name));
        CreateMap<SubcontractorSpecialtyDto, SubcontractorSpecialty>();

        // ROLES: map from the assignment join to the role dto
        CreateMap<SubcontractorRoleAssignment, SubcontractorRoleDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id)) // assignment id; switch to s.SubcontractorRole.Id if you prefer role id
            .ForMember(d => d.SubcontractorId, o => o.MapFrom(s => s.SubcontractorId))
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.SubcontractorRole.Name))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.SubcontractorRole.Description));


    }
}
