using AutoMapper;
using Salubrity.Application.DTOs.Organizations.Branches;
using Salubrity.Domain.Entities.Organizations;

namespace Salubrity.Application.MappingProfiles;

public class OrganizationBranchProfile : Profile
{
    public OrganizationBranchProfile()
    {
        CreateMap<OrganizationBranchCreateDto, OrganizationBranch>();
        CreateMap<OrganizationBranchUpdateDto, OrganizationBranch>();
        CreateMap<OrganizationBranch, OrganizationBranchResponseDto>();
    }
}
