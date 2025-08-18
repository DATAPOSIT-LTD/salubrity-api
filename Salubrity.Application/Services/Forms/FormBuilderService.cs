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

            var formId = Guid.NewGuid();
            var versionId = Guid.NewGuid();

            // Root form
            var form = new IntakeForm
            {
                Id = formId,
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                Sections = [],       // if your model keeps top-level unversioned sections, we still fill versioned ones below
                Fields = [],         // keep empty; we attach fields under the version/sections
                Versions =           // ensure this matches your domain model
                [
                    new IntakeFormVersion
                    {
                        Id = versionId,
                        IntakeFormId = formId,
                        VersionNumber = 1,
                        Sections = [],
                    }
                ]
            };

            var version = form.Versions.First();

            // 1) Sections + fields (version-scoped)
            foreach (var secDto in dto.Sections ?? Enumerable.Empty<CreateFormBlueprintSectionDto>())
            {
                var sectionId = Guid.NewGuid();
                var section = new IntakeFormSection
                {
                    Id = sectionId,
                    IntakeFormId = form.Id,            // keep if your schema has it
                    IntakeFormVersionId = version.Id,  // REQUIRED to satisfy FK
                    Name = secDto.Name,
                    Description = secDto.Description ?? string.Empty,
                    Order = secDto.Order,
                    Fields = []
                };

                foreach (var fldDto in secDto.Fields ?? Enumerable.Empty<CreateFormBlueprintFieldDto>())
                {
                    section.Fields.Add(BuildField(formId: form.Id, formVersionId: version.Id, sectionId: section.Id, fldDto));
                }

                version.Sections.Add(section);
            }

            // 2) Unsectioned fields (still version-scoped)
            foreach (var fldDto in dto.Fields ?? Enumerable.Empty<CreateFormBlueprintFieldDto>())
            {
                version.IntakeForm.Fields.Add(BuildField(formId: form.Id, formVersionId: version.Id, sectionId: null, fldDto));
            }

            // Persist the entire graph once
            await _repo.AddFormGraphAsync(form, ct);

            // 3) Resolve triggers by label/value (second pass) if provided
            var hasLogic =
                (dto.Fields?.Any(f => f.HasConditionalLogic) == true) ||
                (dto.Sections?.SelectMany(s => s.Fields ?? []).Any(f => f.HasConditionalLogic) == true);

            if (hasLogic)
            {
                await ResolveTriggersByLabelAsync(form.Id, version.Id, dto, ct);
            }

            return new FormBlueprintResponseDto { Id = form.Id, Name = form.Name };
        }

        private static IntakeFormField BuildField(Guid formId, Guid formVersionId, Guid? sectionId, CreateFormBlueprintFieldDto dto)
        {
            var field = new IntakeFormField
            {
                Id = Guid.NewGuid(),
                FormId = formId,                 // keep if present in schema
                // FormVersionId = formVersionId,   // REQUIRED to satisfy FK if fields are versioned
                SectionId = sectionId,           // null for unsectioned
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

            foreach (var opt in dto.Options ?? Enumerable.Empty<CreateFieldOptionDto>())
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
        private async Task ResolveTriggersByLabelAsync(Guid formId, Guid formVersionId, CreateFormBlueprintDto dto, CancellationToken ct)
        {
            // Prefer a version-scoped loader if you have it:
            // var version = await _repo.LoadFormVersionWithFieldsAsync(formVersionId, ct)
            //              ?? throw new NotFoundException("Form version not found after creation.");

            // Otherwise: load form with full graph and pick the version we just created
            var form = await _repo.LoadFormWithFieldsAsync(formId, ct)
                       ?? throw new NotFoundException("Form not found after creation.");

            var version = form.Versions?.FirstOrDefault(v => v.Id == formVersionId)
                          ?? form.Versions?.OrderByDescending(v => v.VersionNumber).FirstOrDefault()
                          ?? throw new NotFoundException("Form version not found after creation.");

            // All fields in this version
            var allFields = (version.IntakeForm.Fields ?? Enumerable.Empty<IntakeFormField>())
                .Concat(version.Sections?.SelectMany(s => s.Fields) ?? Enumerable.Empty<IntakeFormField>())
                .ToList();

            // Label -> field lookup (labels should be unique per form/version)
            var byLabel = allFields
                .GroupBy(f => f.Label, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // Flatten blueprint fields in the same logical order
            static IEnumerable<CreateFormBlueprintFieldDto> FlattenBlueprint(CreateFormBlueprintDto bp)
            {
                foreach (var s in bp.Sections ?? Enumerable.Empty<CreateFormBlueprintSectionDto>())
                {
                    foreach (var f in s.Fields ?? Enumerable.Empty<CreateFormBlueprintFieldDto>())
                        yield return f;
                }
                foreach (var f in bp.Fields ?? Enumerable.Empty<CreateFormBlueprintFieldDto>())
                    yield return f;
            }

            foreach (var created in allFields)
            {
                var bp = FlattenBlueprint(dto).FirstOrDefault(x =>
                    string.Equals(x.Label, created.Label, StringComparison.OrdinalIgnoreCase));

                if (bp is null || !bp.HasConditionalLogic) continue;

                if (!string.IsNullOrWhiteSpace(bp.TriggerFieldLabel) &&
                    byLabel.TryGetValue(bp.TriggerFieldLabel, out var triggerField))
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

            // Save the updates on the already-tracked graph
            await _repo.SaveChangesAsync(ct);
        }
    }
}
