using AutoMapper;
using Salubrity.Domain.Entities.FormFields;
using Salubrity.Domain.Entities.Forms;
using Salubrity.Domain.Entities.FormSections;
using Salubrity.Domain.Entities.FormsOptions;

public class FormProfile : Profile
{ 
    public FormProfile()
    {
        CreateMap<CreateFormRequestDto, Form>();
        CreateMap<CreateFormSectionRequestDto, FormSection>();
        CreateMap<CreateFormFieldRequestDto, FormField>();
        CreateMap<CreateFieldOptionRequestDto, FieldOption>();
        
        CreateMap<Form, FormResponseDto>();
        CreateMap<FormSection, FormSectionResponseDto>();
        CreateMap<FormField, FormFieldResponseDto>();
        CreateMap<FieldOption, FieldOptionResponseDto>();
    }
}
