// File: Salubrity.Application/Services/Forms/FormBuilderService.cs
#nullable enable
using Salubrity.Application.DTOs.Forms;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Forms
{
    public class FormBuilderService : IFormBuilderService
    {
        private readonly IFormBuilderRepository _repo;

        public FormBuilderService(IFormBuilderRepository repo)
        {
            _repo = repo;
        }

        public async Task<FormBlueprintResponseDto> CreateFromBlueprintAsync(CreateFormBlueprintDto dto, CancellationToken ct = default)
        {
            if (await _repo.ExistsByNameAsync(dto.Name, ct))
                throw new ValidationException(["A form with the same name already exists."]);

            Guid formId = Guid.NewGuid();
            // Build empty form
            var form = new IntakeForm
            {
                Id = formId,
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                Sections = [],
                Fields = []
            };

            // 1) Sections + fields
            foreach (var secDto in dto.Sections ?? [])
            {
                var section = new IntakeFormSection
                {
                    Id = Guid.NewGuid(),
                    IntakeFormId = form.Id,
                    Name = secDto.Name,
                    Description = secDto.Description ?? string.Empty,
                    Order = secDto.Order,
                    Fields = []
                };

                foreach (var fldDto in secDto.Fields ?? [])
                {
                    section.Fields.Add(BuildField(formId: form.Id, sectionId: section.Id, fldDto));
                }

                form.Sections.Add(section);
            }

            // 2) Unsectioned fields
            foreach (var fldDto in dto.Fields ?? [])
            {
                form.Fields.Add(BuildField(formId: form.Id, sectionId: null, fldDto));
            }

            // Persist the graph so we have real IDs on fields/options
            await _repo.AddFormGraphAsync(form, ct);

            // 3) Resolve triggers by label/value (second pass) if provided
            if ((dto.Fields?.Any(f => f.HasConditionalLogic) == true) ||
                (dto.Sections?.SelectMany(s => s.Fields).Any(f => f.HasConditionalLogic) == true))
            {
                await ResolveTriggersByLabelAsync(form.Id, dto, ct);
            }

            return new FormBlueprintResponseDto { Id = form.Id, Name = form.Name };
        }

        private static IntakeFormField BuildField(Guid formId, Guid? sectionId, CreateFormBlueprintFieldDto dto)
        {
            var field = new IntakeFormField
            {
                Id = Guid.NewGuid(),
                FormId = formId,
                SectionId = sectionId,
                Label = dto.Label,
                FieldType = dto.FieldType,
                IsRequired = dto.IsRequired,
                Order = dto.Order,
                HasConditionalLogic = dto.HasConditionalLogic,
                ConditionalLogicType = dto.ConditionalLogicType,
                // TriggerFieldId / TriggerValueOptionId set in second pass
                ValidationType = dto.ValidationType,
                ValidationPattern = dto.ValidationPattern,
                MinValue = dto.MinValue,
                MaxValue = dto.MaxValue,
                MinLength = dto.MinLength,
                MaxLength = dto.MaxLength,
                CustomErrorMessage = dto.CustomErrorMessage,
                LayoutPosition = dto.LayoutPosition,
                Options = []
            };

            foreach (var opt in dto.Options ?? [])
            {
                field.Options.Add(new IntakeFormFieldOption
                {
                    Id = Guid.NewGuid(),
                    FieldId = field.Id,
                    Value = opt.Value,
                    DisplayText = opt.DisplayText,
                    Order = opt.Order,
                    IsActive = opt.IsActive
                });
            }

            return field;
        }

        // Second pass: map TriggerFieldLabel -> TriggerFieldId and TriggerValueOptionValue -> TriggerValueOptionId
        private async Task ResolveTriggersByLabelAsync(Guid formId, CreateFormBlueprintDto dto, CancellationToken ct)
        {
            var form = await _repo.LoadFormWithFieldsAsync(formId, ct)
                       ?? throw new NotFoundException("Form not found after creation.");

            // Build label -> field lookup (labels must be unique within a form)
            var allFields = (form.Fields ?? Enumerable.Empty<IntakeFormField>())
                .Concat(form.Sections?.SelectMany(s => s.Fields) ?? Enumerable.Empty<IntakeFormField>())
                .ToList();

            var byLabel = allFields
                .GroupBy(f => f.Label)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // Flatten blueprint fields in same order to parallel the created list
            IEnumerable<CreateFormBlueprintFieldDto> FlattenBlueprint()
            {
                foreach (var s in dto.Sections ?? [])
                    foreach (var f in s.Fields ?? [])
                        yield return f;

                foreach (var f in dto.Fields ?? [])
                    yield return f;
            }

            // For each created field, if the blueprint had trigger refs, set them
            foreach (var created in allFields)
            {
                // Find the matching blueprint by label (assumes unique labels)
                var bp = FlattenBlueprint().FirstOrDefault(x => string.Equals(x.Label, created.Label, StringComparison.OrdinalIgnoreCase));
                if (bp is null || !bp.HasConditionalLogic) continue;

                if (!string.IsNullOrWhiteSpace(bp.TriggerFieldLabel) && byLabel.TryGetValue(bp.TriggerFieldLabel, out var triggerField))
                {
                    created.TriggerFieldId = triggerField.Id;

                    if (!string.IsNullOrWhiteSpace(bp.TriggerValueOptionValue))
                    {
                        var triggerOpt = triggerField.Options.FirstOrDefault(o =>
                            string.Equals(o.Value, bp.TriggerValueOptionValue, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(o.DisplayText, bp.TriggerValueOptionValue, StringComparison.OrdinalIgnoreCase));

                        if (triggerOpt != null)
                            created.TriggerValueOptionId = triggerOpt.Id;
                    }
                }
            }

            // Save updates
            await _repo.AddFormGraphAsync(form, ct); // reuses SaveChanges; tracked entities will update
        }
    }
}
