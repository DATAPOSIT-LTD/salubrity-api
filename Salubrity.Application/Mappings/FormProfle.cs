using AutoMapper;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Domain.Entities.FormFields;
using Salubrity.Domain.Entities.Forms;
using Salubrity.Domain.Entities.FormSections;
using Salubrity.Domain.Entities.FormsOptions;

public class FormProfile : Profile
{
    public FormProfile()
    {
        // Create mappings from request DTOs to entities
        CreateMap<CreateFormDto, Form>();
        CreateMap<CreateFormSectionDto, FormSection>();
        CreateMap<CreateFormFieldDto, FormField>();
        CreateMap<CreateFieldOptionDto, FieldOption>();

        // Update mappings
        CreateMap<UpdateFormDto, Form>();
        CreateMap<UpdateFormSectionDto, FormSection>();
        CreateMap<UpdateFormFieldDto, FormField>();

        // Entity to response DTOs
        CreateMap<Form, FormResponseDto>();
        CreateMap<FormSection, FormSectionResponseDto>();
        CreateMap<FormField, FormFieldResponseDto>();
        CreateMap<FieldOption, FieldOptionResponseDto>();
    }
}
