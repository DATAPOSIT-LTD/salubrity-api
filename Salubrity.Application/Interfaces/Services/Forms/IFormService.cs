using Salubrity.Shared.Responses;

namespace Salubrity.Application.Interfaces.Services.Forms;

public interface IFormService
{
    //form operations
    Task<ApiResponse<FormResponseDto>> CreateFormAsync(CreateFormRequestDto request);
    Task<ApiResponse<IEnumerable<FormResponseDto>>> GetAllFormsAsync();

    Task<ApiResponse<FormResponseDto>> GetFormByIdAsync(Guid formId);
    Task<ApiResponse<FormResponseDto>> UpdateFormAsync(Guid formId, UpdateFormRequestDto request);
    Task<ApiResponse<bool>> DeleteFormAsync(Guid formId);

    //form section operations
    Task<ApiResponse<FormSectionResponseDto>> AddSectionToFormAsync(Guid formId, CreateFormSectionRequestDto request);
    Task<ApiResponse<IEnumerable<FormSectionResponseDto>>> GetFormSectionsAsync(Guid formId);  
    Task<ApiResponse<FormSectionResponseDto>> UpdateFormSectionAsync(Guid formId, Guid sectionId, UpdateFormSectionRequestDto request);
    Task<ApiResponse<bool>> DeleteFormSectionAsync(Guid formId, Guid sectionId);
    Task<ApiResponse<FormSectionResponseDto>> GetFormSectionByIdAsync(Guid formId, Guid sectionId);
    Task<ApiResponse<IEnumerable<FormSectionResponseDto>>> GetFormSectionsByFormIdAsync(Guid formId);

    //form field operations
    Task<ApiResponse<FormFieldResponseDto>> AddFieldToFormAsync(Guid formId, CreateFormFieldRequestDto request);
    Task<ApiResponse<IEnumerable<FormFieldResponseDto>>> GetFormFieldsAsync(Guid formId);

    Task<ApiResponse<FormFieldResponseDto>> UpdateFormFieldAsync(Guid formId, Guid fieldId, UpdateFormFieldRequestDto request);
    Task<ApiResponse<bool>> DeleteFormFieldAsync(Guid formId, Guid fieldId);

    //form field option operations
    Task<ApiResponse<FieldOptionResponseDto>> AddOptionToFieldAsync(Guid formId, Guid fieldId, CreateFieldOptionRequestDto request);
    Task<ApiResponse<IEnumerable<FieldOptionResponseDto>>> GetFieldOptionsAsync(Guid formId, Guid fieldId);

    //submit form response



    
}