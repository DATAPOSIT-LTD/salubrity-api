using AutoMapper;
using Microsoft.Extensions.Logging;
using Salubrity.Application.Interfaces.Repositories.Forms;
using Salubrity.Application.Interfaces.Services.Forms;
using Salubrity.Domain.Entities.FormFields;
using Salubrity.Domain.Entities.Forms;
using Salubrity.Domain.Entities.FormSections;
using Salubrity.Domain.Entities.FormsOptions;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salubrity.Application.Services.Forms
{
    public class FormService : IFormService
    {
        private readonly IFormRepository _formRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FormService> _logger;

        public FormService(
            IFormRepository formRepository,
            IMapper mapper,
            ILogger<FormService> logger)
        {
            _formRepository = formRepository;
            _mapper = mapper;
            _logger = logger;
        }

        #region Form Operations
        public async Task<ApiResponse<FormResponseDto>> CreateFormAsync(CreateFormRequestDto request)
        {
            try
            {
                var form = _mapper.Map<Form>(request);

                // Initialize Fields if null
                form.Sections ??= new List<FormSection>();

                // Set initial order
                int order = 1;
                foreach (var field in form.Sections.SelectMany(s => s.SectionFields))
                {
                    field.Order = field.Order == 0 ? order : field.Order;
                    order++;
                }

                var createdForm = await _formRepository.CreateAsync(form);
                if (createdForm == null)
                {
                    return ApiResponse<FormResponseDto>.CreateFailure("Failed to create form in database");
                }

                var response = _mapper.Map<FormResponseDto>(createdForm);
                return ApiResponse<FormResponseDto>.CreateSuccess(response, "Form created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating form. Request: {@Request}", request);
                return ApiResponse<FormResponseDto>.CreateFailure("Failed to create form: " + ex.Message);
            }
        }
        public async Task<ApiResponse<IEnumerable<FormResponseDto>>> GetAllFormsAsync()
        {
            try
            {
                var forms = await _formRepository.GetAllAsync();
                var response = _mapper.Map<IEnumerable<FormResponseDto>>(forms);
                return ApiResponse<IEnumerable<FormResponseDto>>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving forms");
                return ApiResponse<IEnumerable<FormResponseDto>>.CreateFailure("Failed to retrieve forms");
            }
        }

        public async Task<ApiResponse<FormResponseDto>> GetFormByIdAsync(Guid formId)
        {
            try
            {
                var form = await _formRepository.GetFormWithFieldsAndOptionsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<FormResponseDto>.CreateFailure("Form not found");
                }

                var response = _mapper.Map<FormResponseDto>(form);
                return ApiResponse<FormResponseDto>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving form {FormId}", formId);
                return ApiResponse<FormResponseDto>.CreateFailure("Failed to retrieve form");
            }
        }

        public async Task<ApiResponse<FormResponseDto>> UpdateFormAsync(Guid formId, UpdateFormRequestDto request)
        {
            try
            {
                var existingForm = await _formRepository.GetFormWithSectionsAndFieldsAsync(formId);
                if (existingForm == null)
                    return ApiResponse<FormResponseDto>.CreateFailure("Form not found");

                // Update form properties
                _mapper.Map(request, existingForm);

                // Handle sections
                foreach (var sectionDto in request.Sections)
                {
                    var existingSection = existingForm.Sections.FirstOrDefault(s => s.Id == sectionDto.Id);
                    if (existingSection != null)
                    {
                        //if no section ID, it's a new section
                        if (sectionDto.Id == null)
                        {
                            sectionDto.Id = Guid.NewGuid(); // Generate new ID for new section
                        }
                        _mapper.Map(sectionDto, existingSection);
                    }
                    else
                    {
                        var newSection = _mapper.Map<FormSection>(sectionDto);
                        newSection.Id = sectionDto.Id ?? Guid.NewGuid(); // Generate new ID if not provided
                        existingForm.Sections.Add(newSection);
                    }
                }

                // Handle fields
                foreach (var sectionDto in request.Sections)
                {
                    var section = existingForm.Sections.First(s => s.Id == sectionDto.Id);

                    foreach (var fieldDto in sectionDto.SectionFields)
                    //if no field ID, it's a new field
                    {
                        var existingField = section.SectionFields.FirstOrDefault(f => f.Id == fieldDto.Id);
                        if (existingField != null)
                        {
                            _mapper.Map(fieldDto, existingField);
                        }
                        else
                        {
                            var newField = _mapper.Map<FormField>(fieldDto);
                            newField.SectionId = section.Id;
                            section.SectionFields.Add(newField);
                        }
                    }
                }

                await _formRepository.UpdateAsync(existingForm);
                var response = _mapper.Map<FormResponseDto>(existingForm);
                return ApiResponse<FormResponseDto>.CreateSuccess(response, "Form updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating form {FormId}", formId);
                return ApiResponse<FormResponseDto>.CreateFailure("Failed to update form");
            }
        }
        public async Task<ApiResponse<bool>> DeleteFormAsync(Guid formId)
        {
            try
            {
                if (!await _formRepository.FormExistsAsync(formId))
                    throw new NotFoundException("Form not found");

                await _formRepository.DeleteAsync(formId);
                return ApiResponse<bool>.CreateSuccess(true, "Form deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting form {FormId}", formId);
                return ApiResponse<bool>.CreateFailure("Failed to delete form");
            }
        }
        //form section service implementation
        public async Task<ApiResponse<IEnumerable<FormSectionResponseDto>>> GetFormSectionsAsync(Guid formId)
        {
            try
            {
                var form = await _formRepository.GetFormWithSectionsAndFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<IEnumerable<FormSectionResponseDto>>.CreateFailure("Form not found");
                }

                var response = _mapper.Map<IEnumerable<FormSectionResponseDto>>(form.Sections);
                return ApiResponse<IEnumerable<FormSectionResponseDto>>.CreateSuccess(response);
            }
            catch (System.Exception)
            {

                throw;
            }
        }
        public async Task<ApiResponse<FormSectionResponseDto>> UpdateFormSectionAsync(Guid formId, Guid sectionId, UpdateFormSectionRequestDto request)
        {
            try
            {
                var form = await _formRepository.GetFormWithSectionsAndFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<FormSectionResponseDto>.CreateFailure("Form not found");
                }

                var section = form.Sections.FirstOrDefault(s => s.Id == sectionId);
                if (section == null)
                {
                    return ApiResponse<FormSectionResponseDto>.CreateFailure("Section not found in the specified form");
                }

                // Update the section with the request data
                _mapper.Map(request, section);

                await _formRepository.UpdateAsync(form);

                var response = _mapper.Map<FormSectionResponseDto>(section);
                return ApiResponse<FormSectionResponseDto>.CreateSuccess(response, "Section updated successfully");
            }
            catch (System.Exception)
            {

                throw;
            }
        }
        public async Task<ApiResponse<bool>> DeleteFormSectionAsync(Guid formId, Guid sectionId)
        {
            try
            {
                var form = await _formRepository.GetFormWithSectionsAndFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<bool>.CreateFailure("Form not found");
                }

                var section = form.Sections.FirstOrDefault(s => s.Id == sectionId);
                if (section == null)
                {
                    return ApiResponse<bool>.CreateFailure("Section not found in the specified form");
                }

                form.Sections.Remove(section);
                await _formRepository.UpdateAsync(form);

                return ApiResponse<bool>.CreateSuccess(true, "Section deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting section {SectionId} from form {FormId}", sectionId, formId);
                return ApiResponse<bool>.CreateFailure("Failed to delete section");
            }
        }
        public async Task<ApiResponse<FormSectionResponseDto>> AddSectionToFormAsync(Guid formId, CreateFormSectionRequestDto request)
        {
            try
            {
                var form = await _formRepository.GetFormWithSectionsAndFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<FormSectionResponseDto>.CreateFailure("Form not found");
                }

                // Check if section with the same name already exists
                if (form.Sections.Any(s => s.Name != null && s.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return ApiResponse<FormSectionResponseDto>.CreateFailure("Section with this name already exists in the form");
                }

                // Map the request to a FormSection entity
                var section = _mapper.Map<FormSection>(request);
                section.Id = Guid.NewGuid();

                // Add the section to the form
                form.Sections.Add(section);

                await _formRepository.UpdateAsync(form);

                var response = _mapper.Map<FormSectionResponseDto>(section);
                return ApiResponse<FormSectionResponseDto>.CreateSuccess(response, "Section added successfully");
            }
            catch (System.Exception)
            {

                throw;
            }

        }
        public async Task<ApiResponse<FormSectionResponseDto>> GetFormSectionByIdAsync(Guid formId, Guid sectionId)
        {
            try
            {
                var form = await _formRepository.GetFormWithSectionsAndFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<FormSectionResponseDto>.CreateFailure("Form not found");
                }

                var section = form.Sections.FirstOrDefault(s => s.Id == sectionId);
                if (section == null)
                {
                    return ApiResponse<FormSectionResponseDto>.CreateFailure("Section not found in the specified form");
                }

                var response = _mapper.Map<FormSectionResponseDto>(section);
                return ApiResponse<FormSectionResponseDto>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving section {SectionId} from form {FormId}", sectionId, formId);
                return ApiResponse<FormSectionResponseDto>.CreateFailure("Failed to retrieve section");
            }
        }
        public async Task<ApiResponse<IEnumerable<FormSectionResponseDto>>> GetFormSectionsByFormIdAsync(Guid formId)
        {
            try
            {
                var form = await _formRepository.GetFormWithSectionsAndFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<IEnumerable<FormSectionResponseDto>>.CreateFailure("Form not found");
                }

                var response = _mapper.Map<IEnumerable<FormSectionResponseDto>>(form.Sections);
                return ApiResponse<IEnumerable<FormSectionResponseDto>>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sections for form {FormId}", formId);
                return ApiResponse<IEnumerable<FormSectionResponseDto>>.CreateFailure("Failed to retrieve sections");
            }
        }


        public async Task<ApiResponse<FormFieldResponseDto>> AddFieldToFormAsync(Guid formId, CreateFormFieldRequestDto request)
        {
            try
            {
                var form = await _formRepository.GetFormWithSectionsAndFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<FormFieldResponseDto>.CreateFailure("Form not found");
                }

                // Check if field already exists in the form
                if (form.Sections.Any(s => s.SectionFields.Any(f => f.Label != null && f.Label.Equals(request.Label, StringComparison.OrdinalIgnoreCase))))
                {
                    return ApiResponse<FormFieldResponseDto>.CreateFailure("Field with this label already exists in the form");
                }
                // Map the request to a FormField entity
                var field = _mapper.Map<FormField>(request);
                field.Id = Guid.NewGuid();
                field.SectionId = request.SectionId;

                // Find the section to add the field to
                var section = form.Sections.FirstOrDefault(s => s.Id == request.SectionId);
                if (section == null)
                {
                    return ApiResponse<FormFieldResponseDto>.CreateFailure("Section not found in the specified form");
                }

                section.SectionFields.Add(field);

                await _formRepository.UpdateAsync(form);

                var response = _mapper.Map<FormFieldResponseDto>(field);
                return ApiResponse<FormFieldResponseDto>.CreateSuccess(response, "Field added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding field to form {FormId}", formId);
                return ApiResponse<FormFieldResponseDto>.CreateFailure("Failed to add field");
            }
        }

        public async Task<ApiResponse<IEnumerable<FormFieldResponseDto>>> GetFormFieldsAsync(Guid formId)
        {
            try
            {
                var form = await _formRepository.GetFormWithFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<IEnumerable<FormFieldResponseDto>>.CreateFailure("Form not found");
                }

                var response = _mapper.Map<IEnumerable<FormFieldResponseDto>>(form.Sections.OrderBy(f => f.Order));
                return ApiResponse<IEnumerable<FormFieldResponseDto>>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fields for form {FormId}", formId);
                return ApiResponse<IEnumerable<FormFieldResponseDto>>.CreateFailure("Failed to retrieve fields");
            }
        }

        public async Task<ApiResponse<FormFieldResponseDto>> UpdateFormFieldAsync(Guid formId, Guid fieldId, UpdateFormFieldRequestDto request)
        {
            try
            {
                var form = await _formRepository.GetFormWithFieldsAsync(formId);
                if (form == null)
                {
                    return ApiResponse<FormFieldResponseDto>.CreateFailure("Form not found");
                }

                var sectionExists = form.Sections.Any(s => s.SectionFields.Any(f => f.Id == fieldId));
                if (!sectionExists)
                {
                    return ApiResponse<FormFieldResponseDto>.CreateFailure("Field not found in specified form");
                }
                else
                {
                    var field = form.Sections.SelectMany(s => s.SectionFields).FirstOrDefault(f => f.Id == fieldId);
                    if (field == null)
                    {
                        return ApiResponse<FormFieldResponseDto>.CreateFailure("Field not found");
                    }
                    _mapper.Map(request, field);

                    // Update the field in the repository
                    await _formRepository.UpdateFormFieldsAsync(formId, new List<FormField> { field });

                    var response = _mapper.Map<FormFieldResponseDto>(field);
                    return ApiResponse<FormFieldResponseDto>.CreateSuccess(response, "Field updated successfully");
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating field {FieldId} in form {FormId}", fieldId, formId);
                return ApiResponse<FormFieldResponseDto>.CreateFailure("Failed to update field");
            }
        }

        public async Task<ApiResponse<bool>> DeleteFormFieldAsync(Guid formId, Guid fieldId)
        {
            try
            {
                await _formRepository.RemoveFieldsAsync(formId, fieldId);
                return ApiResponse<bool>.CreateSuccess(true, "Field deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting field {FieldId} from form {FormId}", fieldId, formId);
                return ApiResponse<bool>.CreateFailure("Failed to delete field");
            }
        }
        #endregion

        #region Field Option Operations
        public async Task<ApiResponse<FieldOptionResponseDto>> AddOptionToFieldAsync(Guid formId, Guid fieldId, CreateFieldOptionRequestDto request)
        {
            try
            {
                var field = await _formRepository.GetFormFieldWithOptionsAsync(fieldId);
                if (field == null || !await _formRepository.FieldExistsInFormAsync(formId, fieldId))
                {
                    return ApiResponse<FieldOptionResponseDto>.CreateFailure("Field not found in specified form");
                }

                if (field.Options.Any(o => o.Value.Equals(request.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    return ApiResponse<FieldOptionResponseDto>.CreateFailure("Option with this value already exists");
                }

                var option = _mapper.Map<FieldOption>(request);
                field.Options.Add(option);

                // Need to update the field with new options
                await _formRepository.UpdateFormFieldsAsync(formId, new List<FormField> { field });

                var response = _mapper.Map<FieldOptionResponseDto>(option);
                return ApiResponse<FieldOptionResponseDto>.CreateSuccess(response, "Option added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding option to field {FieldId} in form {FormId}", fieldId, formId);
                return ApiResponse<FieldOptionResponseDto>.CreateFailure("Failed to add option");
            }
        }

        public async Task<ApiResponse<IEnumerable<FieldOptionResponseDto>>> GetFieldOptionsAsync(Guid formId, Guid fieldId)
        {
            try
            {
                var field = await _formRepository.GetFormFieldWithOptionsAsync(fieldId);
                if (field == null || !await _formRepository.FieldExistsInFormAsync(formId, fieldId))
                {
                    return ApiResponse<IEnumerable<FieldOptionResponseDto>>.CreateFailure("Field not found in specified form");
                }

                var response = _mapper.Map<IEnumerable<FieldOptionResponseDto>>(field.Options);
                return ApiResponse<IEnumerable<FieldOptionResponseDto>>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving options for field {FieldId} in form {FormId}", fieldId, formId);
                return ApiResponse<IEnumerable<FieldOptionResponseDto>>.CreateFailure("Failed to retrieve options");
            }
        }
        
        #endregion
    }
}