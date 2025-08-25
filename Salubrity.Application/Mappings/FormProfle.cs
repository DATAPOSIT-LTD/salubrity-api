using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Domain.Entities.IntakeForms;


public class FormProfile : Profile
{
    public FormProfile()
    {
        // Create mappings from request DTOs to entities
        CreateMap<CreateFormDto, IntakeForm>();
        CreateMap<CreateFormSectionDto, IntakeFormSection>();
        CreateMap<CreateFormFieldDto, IntakeFormField>();
        CreateMap<CreateFieldOptionDto, IntakeFormFieldOption>();

        // Update mappings
        CreateMap<UpdateFormDto, IntakeForm>();
        CreateMap<UpdateFormSectionDto, IntakeFormSection>();
        CreateMap<UpdateFormFieldDto, IntakeFormField>();

        // Entity to response DTOs
        CreateMap<IntakeForm, FormResponseDto>();
        CreateMap<IntakeFormSection, FormSectionResponseDto>();
        CreateMap<IntakeFormField, FormFieldResponseDto>();
        CreateMap<IntakeFormFieldOption, FieldOptionResponseDto>();
    }
}
