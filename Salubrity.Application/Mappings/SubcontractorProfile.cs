using AutoMapper;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Domain.Entities.Subcontractor;

namespace Salubrity.Application.Mappings;

public class SubcontractorProfile : Profile
{
    public SubcontractorProfile()
    {
        CreateMap<Subcontractor, SubcontractorDto>();
        CreateMap<CreateSubcontractorDto, Subcontractor>();
        CreateMap<UpdateSubcontractorDto, Subcontractor>();


        CreateMap<SubcontractorSpecialty, SubcontractorSpecialtyDto>();


        CreateMap<SubcontractorSpecialtyDto, SubcontractorSpecialty>();
    }
}


