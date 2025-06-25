using AutoMapper;
using Salubrity.Application.DTOs.Organizations;
using Salubrity.Domain.Entities.Organizations;

namespace Salubrity.Application.Mappings
{
    public class OrganizationMappingProfile : Profile
    {
        public OrganizationMappingProfile()
        {
            CreateMap<OrganizationCreateDto, Organization>();
            CreateMap<OrganizationUpdateDto, Organization>();
            CreateMap<Organization, OrganizationResponseDto>();
        }
    }
}
