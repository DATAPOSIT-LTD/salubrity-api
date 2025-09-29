using AutoMapper;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Application.DTOs.Users;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Subcontractor;

namespace Salubrity.Application.Mappings;

public class SubcontractorProfile : Profile
{
    public SubcontractorProfile()
    {
        // Main Subcontractor → DTO
        CreateMap<Subcontractor, SubcontractorDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.User.Phone)) // Use .PhoneNumber if that's the property
            .ForMember(d => d.IndustryName, o => o.MapFrom(s => s.Industry.Name))
            .ForMember(d => d.StatusName, o => o.MapFrom(s => s.Status.Name))
            .ForMember(d => d.Specialties, o => o.MapFrom(s => s.Specialties))
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.RoleAssignments))
            .ForMember(d => d.CampAssignmentCount, o => o.MapFrom(s => s.CampAssignments.Count));

        // Create and Update DTO → Entity
        CreateMap<CreateSubcontractorDto, Subcontractor>();
        CreateMap<UpdateSubcontractorDto, Subcontractor>();

        // Specialties
        CreateMap<SubcontractorSpecialty, SubcontractorSpecialtyDto>()
            .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service.Name));
        CreateMap<SubcontractorSpecialtyDto, SubcontractorSpecialty>();

        // Role assignments → Role DTO (profession)
        CreateMap<SubcontractorRoleAssignment, SubcontractorRoleDto>()
            .ForMember(d => d.RoleId, o => o.MapFrom(s => s.SubcontractorRole.Id))
            .ForMember(d => d.SubcontractorId, o => o.MapFrom(s => s.SubcontractorId))
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.SubcontractorRole.Name))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.SubcontractorRole.Description));

        // Fixes for update subcontractor
        CreateMap<UserUpdateRequest, User>();
    }
}
